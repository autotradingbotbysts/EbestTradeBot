using EbestTradeBot.Client.EventArgs;
using EbestTradeBot.Client.Services.OpenApi;
using EbestTradeBot.Client.Services.Trade;
using EbestTradeBot.Client.Services.XingApi;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Windows.Threading;

namespace EbestTradeBot.Client.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        private readonly ITradeService _trade;

        private bool _isRun = false;
        public bool IsRun
        {
            get => _isRun;
            set => SetProperty(ref _isRun, value);
        }

        private string _board = string.Empty;
        public string Board
        {
            get
            {
                return _board;
            }
            set
            {
                SetProperty(ref _board, value);
            }
        }

        public DelegateCommand RunCommand { get; set; }
        public DelegateCommand ClearCommand { get; set; }

        private async Task OnStarted()
        {
            try
            {
                await _trade.StartTrade();
            }
            catch(Exception ex)
            {
                AddBoard($"[ERROR] {ex.Message}");
                await OnStopped();
            }
        }

        private async Task OnStopped()
        {
            try
            {
                await _trade.StopTrade();
            }
            catch (Exception ex)
            {
                AddBoard($"[ERROR] {ex.Message}");
            }
        }

        public MainWindowViewModel(
            ITradeService trade
            )
        {
            _trade = trade;
            _trade.WriteLog += (sender, e) =>
            {
                var arg = e as LogEventArgs;
                AddBoard(arg.Message ?? string.Empty);
            };
            _trade.StopTradeEvent += _trade_StopTradeEvent;
            RunCommand = new DelegateCommand(async () => await ExecuteRunCommand());
            ClearCommand = new DelegateCommand(() => Dispatcher.CurrentDispatcher.Invoke(() =>
            {
                Board = string.Empty;
            }));
        }

        private void _trade_StopTradeEvent(object? sender, System.EventArgs e)
        {
            Dispatcher.CurrentDispatcher.Invoke(() =>
            {
                IsRun = false;
            });
        }

        private async Task SetRun(bool isRun)
        {
            Dispatcher.CurrentDispatcher.Invoke(() =>
            {
                IsRun = isRun;
            });

            if (IsRun)
            {
                OnStarted();
            }
            else
            {
                await OnStopped();
            }
        }

        private async Task ExecuteRunCommand()
        {
            await SetRun(!IsRun);
        }

        private void AddBoard(string board)
        {
            Dispatcher.CurrentDispatcher.Invoke(() =>
            {
                Board += $"[{DateTime.Now.ToShortDateString()} {DateTime.Now.ToShortTimeString()}] {board}\r\n";
            });
        }
    }
}
