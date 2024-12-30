using CsvHelper;
using CsvHelper.Configuration;
using EbestTradeBot.Shared.Models.Log;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;

namespace EbestTradeBot.Client.Services.Log
{
    public class LogService : ILogService
    {
        private readonly string _filePath = @".\";
        private readonly static object _lock = new object();

        public async Task WriteLog(LogModel model)
        {
            lock (_lock)
            {
                var filePath = $"{_filePath}log.csv";
                var fileExists = File.Exists(filePath);
                if (!fileExists)
                {
                    File.Create(filePath).Dispose();
                }

                using var writer = new StreamWriter(filePath, true);
                using var csv = new CsvWriter(writer, new CsvConfiguration(CultureInfo.InvariantCulture) { HasHeaderRecord = !fileExists });

                if (!fileExists)
                {
                    csv.WriteHeader<LogModel>();
                    csv.NextRecord();
                }

                csv.WriteRecord(model);
                csv.NextRecord();
            }
        }

        public async Task<List<LogModel>> GetLogs()
        {
            lock(_lock)
            {
                var filePath = $"{_filePath}log.csv";
                if (!File.Exists(filePath))
                {
                    File.Create(filePath).Dispose();
                }

                var ret = new List<LogModel>();

                using var reader = new StreamReader(filePath);
                using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture));
                while (csv.Read())
                {
                    ret.Add(csv.GetRecord<LogModel>());
                }

                return ret;
            }
        }
    }
}
