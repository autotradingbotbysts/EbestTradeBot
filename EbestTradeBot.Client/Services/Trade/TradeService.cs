﻿using CsvHelper;
using CsvHelper.Configuration;
using EbestTradeBot.Client.EventArgs;
using EbestTradeBot.Client.Services.Log;
using EbestTradeBot.Client.Services.OpenApi;
using EbestTradeBot.Client.Services.XingApi;
using EbestTradeBot.Shared.Exceptions;
using EbestTradeBot.Shared.Models.Trade;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using XA_DATASETLib;
using XA_SESSIONLib;

namespace EbestTradeBot.Client.Services.Trade
{
    public class TradeService : ITradeService
    {
        private readonly IOptionsMonitor<XingApiOptions> _xingApiOptionsMonitor;
        private readonly IOptionsMonitor<DefaultOptions> _defaultOptionsMonitor;
        private XingApiOptions _xingApiOptions => _xingApiOptionsMonitor.CurrentValue;
        private DefaultOptions _defaultOptions => _defaultOptionsMonitor.CurrentValue;

        private XASession _xaSession = new();
        private XAQuery _xaQuery_t1857 = new();
        private readonly string _filePath = @".\";

        private TaskCompletionSource<(string szCode, string szMsg)> _loginTaskCompletionSource;
        private TaskCompletionSource<List<Stock>> _searchTaskCompletionSource;

        private readonly IOpenApiService _openApi;
        private readonly ILogService _log;

        private CancellationTokenSource _cancellationTokenSource = new();

        public event EventHandler WriteLog;
        public event EventHandler StopTradeEvent;

        public TradeService(
            IOptionsMonitor<DefaultOptions> defaultOptionsMonitor,
            IOptionsMonitor<XingApiOptions> xingApiOptionsMonitor,
            IOpenApiService openApi,
            ILogService log)
        {
            _defaultOptionsMonitor = defaultOptionsMonitor;
            _xingApiOptionsMonitor = xingApiOptionsMonitor;
            _openApi = openApi;
            _log = log;
            ((_IXASessionEvents_Event)_xaSession).Login += OnLogin;
            ((_IXAQueryEvents_Event)_xaQuery_t1857).ReceiveData += _xaQuery_t1857_OnReceiveData;
            _xaQuery_t1857.ResFileName = @"Res\t1857.res";
        }

        public async Task StartTrade()
        {
            WriteLog?.Invoke(this, new LogEventArgs("매매를 시작합니다."));
            _cancellationTokenSource = new();

            await InitToken();

            while (!_cancellationTokenSource.Token.IsCancellationRequested)
            {
                Task? buyTask = null;
                Task? sellTask = null;

                _loginTaskCompletionSource = new();
                _searchTaskCompletionSource = new();

                try
                {
                    DateTime now = DateTime.Now;
                    // 09:00 ~ 15:30 일경우 StopTrade() 호출
                    if (now.Hour < 9 || now.TimeOfDay >= new TimeSpan(15, 31, 00))
                    {
                        throw new MarketClosedException();
                    }

                    var searchedStocks = await SearchStocks();
                    if (_cancellationTokenSource.Token.IsCancellationRequested) break;

                    var accountStocksForBuying = await GetAccountStocks();
                    if (_cancellationTokenSource.Token.IsCancellationRequested) break;

                    var accountStocksForSelling = DeepCopyStocks(accountStocksForBuying);
                    if (_cancellationTokenSource.Token.IsCancellationRequested) break;

                    var tradingStocks = await GetTradingShcodes();
                    if (_cancellationTokenSource.Token.IsCancellationRequested) break;

                    var recentlyTradingStocks = await GetRecentlyTradingStocks();
                    if (_cancellationTokenSource.Token.IsCancellationRequested) break;

                    buyTask = Task.Run(async () =>
                    {
                        // 1차 매수
                        foreach (var stock in searchedStocks)
                        {
                            if (
                                stock.매수가_1차 >= stock.현재가 && // 1차 매수가 도착
                                !(stock.현재가 > stock.익절가 || stock.현재가 < stock.손절가) && // 익절가, 손절가 범위 내
                                !accountStocksForBuying.Any(x => x.Shcode == stock.Shcode) && // 보유중인 종목이 아님
                                !recentlyTradingStocks.Contains(stock.Shcode) && // 최근 매매한 종목이 아님
                                !tradingStocks.Contains(stock.Shcode)) // 현재 매매중인 종목이 아님
                            {
                                // 매수
                                WriteLog?.Invoke(this, new LogEventArgs($"[{stock.Hname}({stock.Shcode})] [1차 매수]"));
                                await _log.WriteLog(new() { StockName = stock.Hname, StockCode = stock.Shcode, Note = "1차 매수" });
                                await BuyStock(stock);
                                if (_cancellationTokenSource.Token.IsCancellationRequested) break;
                            }
                        }

                        if (_cancellationTokenSource.Token.IsCancellationRequested) return;

                        // 2차 매수
                        foreach (var stock in accountStocksForBuying)
                        {
                            if (
                                stock.매수가_2차 >= stock.현재가 && // 2차 매수가 도착
                                !(stock.현재가 > stock.익절가 || stock.현재가 < stock.손절가) && // 익절가, 손절가 범위 내
                                CheckFirstTrade(stock) && // 1차 매수 체크
                                !tradingStocks.Contains(stock.Shcode)) // 현재 매매중인 종목이 아님
                            {
                                // 매수
                                WriteLog?.Invoke(this, new LogEventArgs($"[{stock.Hname}({stock.Shcode})] [2차 매수]"));
                                await _log.WriteLog(new() { StockName = stock.Hname, StockCode = stock.Shcode, Note = "2차 매수" });
                                await BuyStock(stock);
                                if (_cancellationTokenSource.Token.IsCancellationRequested) break;
                            }
                        }
                    });

                    sellTask = Task.Run(async () =>
                    {
                        foreach (var stock in accountStocksForSelling)
                        {
                            if (
                                (stock.현재가 <= stock.손절가 || stock.현재가 >= stock.익절가) &&
                                !tradingStocks.Contains(stock.Shcode)) // 현재 매매중인 종목이 아님
                            {
                                // 매도
                                WriteLog?.Invoke(this, new LogEventArgs($"[{stock.Hname}({stock.Shcode})] [매도]"));
                                await _log.WriteLog(new() { StockName = stock.Hname, StockCode = stock.Shcode, Note = "매도" });
                                await SellStock(stock);
                                if (_cancellationTokenSource.Token.IsCancellationRequested) break;
                            }
                        }
                    });
                }
                catch (TooManyRequestException ex)
                {
                    WriteLog?.Invoke(this, new LogEventArgs(($"[{ex.Code}] {ex.Message}")));
                }
                catch (ArgumentException ex)
                {
                    WriteLog?.Invoke(this, new LogEventArgs(ex.Message));
                }
                catch(InvalidTokenException ex)
                {
                    WriteLog?.Invoke(this, new LogEventArgs($"[{ex.Code}] {ex.Message}"));
                    await InitToken();
                }
                catch(MarketClosedException)
                {
                    continue;
                }
                catch(Exception)
                {
                    throw;
                }
                finally
                {
                    if(buyTask != null)
                    {
                        await buyTask;
                    }
                    if(sellTask != null)
                    {
                        await sellTask;
                    }

                    if(!_loginTaskCompletionSource.Task.IsCompleted)
                    {
                        _loginTaskCompletionSource.SetCanceled();
                    }

                    if (!_searchTaskCompletionSource.Task.IsCompleted)
                    {
                        _searchTaskCompletionSource.SetCanceled();
                    }
                    await Task.Delay(_defaultOptions.ReplySecond * 1000);
                }
            }

        }

        public async Task StopTrade()
        {
            WriteLog?.Invoke(this, new LogEventArgs("매매를 종료합니다."));

            _cancellationTokenSource.Cancel();
            await RevokeToken();
            StopTradeEvent?.Invoke(this, new());

            WriteLog?.Invoke(this, new LogEventArgs("매매를 성공적으로 종료했습니다."));
        }

        private async Task<List<string>> GetRecentlyTradingStocks()
        {
            var logs = await _log.GetLogs();

            var baseDate = DateTime.Now.AddDays((_defaultOptions.CooldownDay * -1));

            return logs.Where(x => x.Note.Equals("매도") && x.Date > baseDate).Select(x => x.StockCode).Distinct().ToList();
        }

        private async Task RevokeToken()
        {
            await _openApi.RevokeToken();
            WriteLog?.Invoke(this, new LogEventArgs("토큰을 성공적으로 폐기했습니다."));
        }
        private async Task InitToken()
        {
            await _openApi.InitToken(_cancellationTokenSource.Token);
            WriteLog?.Invoke(this, new LogEventArgs("토큰을 성공적으로 초기화했습니다."));
        }

        private async Task<List<string>> GetTradingShcodes()
        {
            var tradingStocks = await _openApi.GetTradingStocks(_cancellationTokenSource.Token);

            return tradingStocks.Select(x => x.Shcode).Distinct().ToList();
        }

        private async Task SellStock(Stock stock)
        {
            await _openApi.SellStock(stock.Shcode, stock.보유량, _defaultOptions.IsTestTrade, _cancellationTokenSource.Token);
        }

        private async Task BuyStock(Stock stock)
        {
            int count = int.MinValue;

            if(stock.보유량 <= 0) // 1차 매수
            {
                count = (int)(_defaultOptions.TradePrice / stock.현재가);
            }
            else // 2차 매수
            {
                count = stock.보유량;
            }

            await _openApi.BuyStock(stock.Shcode, count, _defaultOptions.IsTestTrade, _cancellationTokenSource.Token);
        }
        private async Task<List<Stock>> GetAccountStocks()
        {
            var stocks = await _openApi.GetAccountStocks(_cancellationTokenSource.Token);
            if (_cancellationTokenSource.Token.IsCancellationRequested) return [];

            await _openApi.GetCurrentPrice(stocks, _cancellationTokenSource.Token);
            if (_cancellationTokenSource.Token.IsCancellationRequested) return [];

            var tradingPriceData = ReadDataFromCsv().Where(x => x.Date == DateTime.Now.ToString("yyyyMMdd"));
            var shcodes = tradingPriceData.Select(x => x.Shcode).ToList();

            var calcStocks = stocks.Where(x => shcodes.Contains(x.Shcode)).ToList();
            var nonCalcStocks = stocks.Where(x => !shcodes.Contains(x.Shcode)).ToList();
            foreach (var stock in calcStocks)
            {
                var tradingPrice = tradingPriceData.FirstOrDefault(x => x.Shcode == stock.Shcode);
                if (tradingPrice != null)
                {
                    stock.매수가_1차 = tradingPrice.매수가_1차;
                    stock.매수가_2차 = tradingPrice.매수가_2차;
                    stock.익절가 = tradingPrice.익절가;
                    stock.손절가 = tradingPrice.손절가;
                }
            }

            await _openApi.SetTradingPrice(nonCalcStocks, _cancellationTokenSource.Token);
            if (_cancellationTokenSource.Token.IsCancellationRequested) return [];
            var nonCalcTradingPriceData = nonCalcStocks.Select(x => new TradingPriceData
            {
                Date = DateTime.Now.ToString("yyyyMMdd"),
                Hname = x.Hname,
                Shcode = x.Shcode,
                매수가_1차 = x.매수가_1차,
                매수가_2차 = x.매수가_2차,
                익절가 = x.익절가,
                손절가 = x.손절가
            }).ToList();
            WriteDataToCsv(nonCalcTradingPriceData);

            return stocks;
        }

        private async Task<List<Stock>> SearchStocks()
        {
            var stocks = await GetSearchShcodes(_defaultOptions.IsTestTrade);
            await _openApi.GetCurrentPrice(stocks, _cancellationTokenSource.Token);
            if (_cancellationTokenSource.Token.IsCancellationRequested) return [];

            var tradingPriceData = ReadDataFromCsv().Where(x => x.Date == DateTime.Now.ToString("yyyyMMdd"));
            var shcodes = tradingPriceData.Select(x => x.Shcode).ToList();

            var calcStocks = stocks.Where(x => shcodes.Contains(x.Shcode)).ToList();
            var nonCalcStocks = stocks.Where(x => !shcodes.Contains(x.Shcode)).ToList();
            foreach (var stock in calcStocks)
            {
                var tradingPrice = tradingPriceData.FirstOrDefault(x => x.Shcode == stock.Shcode);
                if (tradingPrice != null)
                {
                    stock.매수가_1차 = tradingPrice.매수가_1차;
                    stock.매수가_2차 = tradingPrice.매수가_2차;
                    stock.익절가 = tradingPrice.익절가;
                    stock.손절가 = tradingPrice.손절가;
                }
            }

            await _openApi.SetTradingPrice(nonCalcStocks, _cancellationTokenSource.Token);
            if (_cancellationTokenSource.Token.IsCancellationRequested) return [];

            nonCalcStocks.RemoveAll(x => x.매수가_1차 < 0);
            stocks.RemoveAll(x => x.매수가_1차 < 0);
            var nonCalcTradingPriceData = nonCalcStocks.Select(x => new TradingPriceData
            {
                Date = DateTime.Now.ToString("yyyyMMdd"),
                Hname = x.Hname,
                Shcode = x.Shcode,
                매수가_1차 = x.매수가_1차,
                매수가_2차 = x.매수가_2차,
                익절가 = x.익절가,
                손절가 = x.손절가
            }).ToList();
            WriteDataToCsv(nonCalcTradingPriceData);

            return stocks;
        }

        private async Task<List<Stock>> GetSearchShcodes(bool isTestTrade)
        {
            if (!_xaSession.IsConnected())
                await Login();

            _xaQuery_t1857.SetFieldData("t1857InBlock", "sRealFlag", 0, "0");
            _xaQuery_t1857.SetFieldData("t1857InBlock", "sSearchFlag", 0, "F");
            _xaQuery_t1857.SetFieldData("t1857InBlock", "query_index", 0, _xingApiOptions.AcfFilePath);

            int nSuccess = _xaQuery_t1857.RequestService("t1857", "");
            if (nSuccess < 0)
            {
                ThrowXingApiErrorCode(nSuccess);
            }

            return await _searchTaskCompletionSource.Task;
        }

        private void ThrowXingApiErrorCode(int errorCode)
        {
            /*
            -1 소켓생성 실패
            -2 서버연결 실패
            -3 서버주소가 맞지 않습니다.
            -4 서버 연결시간 초과
            -5 이미 서버에 연결중입니다.
            -6 해당 TR은 사용할 수 없습니다.
            -7 로그인이 필요합니다.
            -8 시세전용에서는 사용이 불가능합니다.
            -9 해당 계좌번호를 가지고 있지 않습니다.
            -10 Packet의 크기가 잘못되었습니다.
            -11 Data 크기가 다릅니다.
            -12 계좌가 존재하지 않습니다.
            -13 Request ID 부족
            -14 소켓이 생성되지 않았습니다.
            -15 암호화 생성에 실패했습니다.
            -16 데이터 전송에 실패했습니다.
            -17 암호화(RTN)처리에 실패했습니다.
            -18 공인인증 파일이 없습니다.
            -19 공인인증 Function이 없습니다.
            -20 메모리가 충분하지 않습니다.
            -21 TR의 초당 사용횟수 초과로 사용이 불가능합니다.
            -22 해당 TR은 해당 함수를 이용할 수 없습니다.
            -23 TR에 대한 정보를 찾을 수 없습니다.
            -24 계좌위치가 지정되지 않았습니다.
            -25 계좌를 가지고 있지 않습니다.
            -26 파일 읽기에 실패했습니다. (종목 검색 조회 시, 파일이 없는 경우)
            -27 실시간 종목검색 조건 등록이 10건을 초과하였습니다.
            -28 등록 키에 대한 정보를 찾을 수 없습니다.(API->HTS 종목 연동 키 오류)
             */
            switch (errorCode)
            {
                case -21:
                    throw new TooManyRequestException(errorCode.ToString(), "TR의 초당 사용횟수 초과로 사용이 불가능합니다.(XingAPI)");
            }
        }

        private async Task Login()
        {
            if (_xaSession.IsConnected())
                return;

            if (_defaultOptions.IsTestTrade)
            {
                _xaSession.ConnectServer("demo.ebestsec.co.kr", 20001);
            }
            else
            {
                _xaSession.ConnectServer("hts.ebestsec.co.kr", 20001);
            }

            if (!_xaSession.Login(_xingApiOptions.XingApiId, _xingApiOptions.XingApiPassword, _xingApiOptions.CertificationPassword, 0, true))
            {
                throw new Exception("로그인 실패");
            }

            var (szCode, szMsg) = await _loginTaskCompletionSource.Task;
            if (szCode != "0000")
            {
                throw new Exception($"XingAPI 로그인 실패 [({szCode}){szMsg}]");
            }
        }

        private void _xaQuery_t1857_OnReceiveData(string szTrCode)
        {
            var stocks = new List<Stock>();

            if (!int.TryParse(_xaQuery_t1857.GetFieldData("t1857OutBlock", "result_count", 0), out var count))
            {
                _searchTaskCompletionSource?.SetResult(stocks);
                return;
            }

            for (int i = 0; i < count; i++)
            {
                string shcode = _xaQuery_t1857.GetFieldData("t1857OutBlock1", "shcode", i);
                string hname = _xaQuery_t1857.GetFieldData("t1857OutBlock1", "hname", i);

                stocks.Add(new() 
                {

                    Shcode = shcode,
                    Hname = hname
                });
            }

            _xaQuery_t1857.ClearBlockdata("t1857OutBlock");
            _xaQuery_t1857.ClearBlockdata("t1857OutBlock1");

            _searchTaskCompletionSource?.SetResult(stocks);
        }

        private void OnLogin(string szCode, string szMsg)
        {
            _loginTaskCompletionSource.SetResult((szCode, szMsg));
        }

        private bool CheckFirstTrade(Stock stock)
        {
            return (_defaultOptions.TradePrice * (decimal)1.1) - (stock.매수가_1차 * stock.보유량) < 0;
        }

        private static List<Stock> DeepCopyStocks(List<Stock> original)
        {
            ArgumentNullException.ThrowIfNull(original);

            var copy = new List<Stock>();
            foreach (var stock in original)
            {
                copy.Add(stock.Clone());
            }
            return copy;
        }
        private void WriteDataToCsv(List<TradingPriceData> data)
        {
            var filePath = Path.Combine(_filePath, "TradingPriceData.csv");
            bool fileExists = File.Exists(filePath);

            foreach(var stock in data)
            {
                WriteLog?.Invoke(this, new LogEventArgs(
                                $"[{stock.Hname}({stock.Shcode})] " +
                                $"[익절가:{stock.익절가}] " +
                                $"[손절가:{stock.손절가}] " +
                                $"[1차 매수가:{stock.매수가_1차}] " +
                                $"[2차 매수가:{stock.매수가_2차}]"));
            }

            using var writer = new StreamWriter(filePath, true);
            using var csv = new CsvWriter(writer, new CsvConfiguration(CultureInfo.InvariantCulture) { HasHeaderRecord = !fileExists });
            csv.Context.RegisterClassMap<TradingPriceDataMap>();
            if (!fileExists)
            {
                csv.WriteHeader<TradingPriceData>();
                csv.NextRecord();
            }

            csv.WriteRecords(data);
        }
        private List<TradingPriceData> ReadDataFromCsv()
        {
            var filePath = Path.Combine(_filePath, "TradingPriceData.csv");
            if (!File.Exists(filePath))
            {
                return [];
            }

            using var reader = new StreamReader(filePath);
            using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
            
            csv.Context.RegisterClassMap<TradingPriceDataMap>();
            var records = csv.GetRecords<TradingPriceData>();
            return [..records];
        }
    }
}
