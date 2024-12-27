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

        public async Task<List<LogModel>> GetLogs()
        {
            var ret = new List<LogModel>();

            using var reader = new StreamReader(_filePath + "log.csv");
            using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture));
            while (csv.Read())
            {
                ret.Add(csv.GetRecord<LogModel>());
            }

            return ret;
        }
    }
}
