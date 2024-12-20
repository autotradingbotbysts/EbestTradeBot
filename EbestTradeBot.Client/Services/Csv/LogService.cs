using CsvHelper;
using CsvHelper.Configuration;
using EbestTradeBot.Shared.Models.Log;
using System;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;

namespace EbestTradeBot.Client.Services.Log
{
    public class LogService : ILogService
    {
        private readonly string _filePath = @".\";

        public async Task WriteLog(LogModel model)
        {
            bool fileExists = File.Exists(_filePath + "log.csv");

            using var writer = new StreamWriter(_filePath + "log.csv", true);
            using var csv = new CsvWriter(writer, new CsvConfiguration(CultureInfo.InvariantCulture) { HasHeaderRecord = !fileExists });
            
            if(!fileExists)
            {
                csv.WriteHeader<LogModel>();
                csv.NextRecord();
            }

            csv.WriteRecord(model);
            csv.NextRecord();
        }
    }
}
