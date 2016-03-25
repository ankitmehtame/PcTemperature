using PcTemperature.Helpers;
using PcTemperature.Models;
using log4net;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace PcTemperature.Services
{
    public class SpeedFanFileReader
    {
        private TempReading[] _readings = new TempReading[0];
        private string _filePath;
        private DateTime _date;

        private ILog Log = LogManager.GetLogger(typeof(SpeedFanFileReader));

        public SpeedFanFileReader(string filePath)
        {
            _filePath = filePath;
            _date = GetDate();
        }

        public async Task Init()
        {
            _readings = await ReadAll();
        }

        private async Task<TempReading[]> ReadAll()
        {
            var readings = new List<TempReading>();
            string[] lines;
            try
            {
                lines = await FileReader.ReadAllLinesAsync(_filePath);
            }
            catch (Exception ex)
            {
                Log.Debug($"Unable to read file {_filePath}. Error: {ex}");
                throw;
            }
            if (lines.Length <= 1)
            {
                Log.Debug($"There are no values to read in file {_filePath}");
                return new TempReading[0];
            }
            var headerLine = lines[0];
            var headerFields = GetFields(headerLine);
            int secondsIndex = 0;
            var hardDiskIndices = new List<int>();
            var cpuIndices = new List<int>();

            for (var index = 0; index < headerFields.Length; index++)
            {
                var headerField = headerFields[index];
                if (string.Equals(headerField, "Seconds", StringComparison.OrdinalIgnoreCase))
                {
                    secondsIndex = index;
                }
                else if (headerField.ToUpperInvariant().Contains("HD".ToUpperInvariant()))
                {
                    hardDiskIndices.Add(index);
                }
                else if (headerField.ToUpperInvariant().Contains("Core".ToUpperInvariant()))
                {
                    cpuIndices.Add(index);
                }
            }

            for (var row = 1; row < lines.Length; row++)
            {
                var line = lines[row];
                var fields = GetFields(line);
                var reading = new TempReading
                {
                    HardDiskTemperatures = new decimal[hardDiskIndices.Count],
                    CoreTemperatures = new decimal[cpuIndices.Count]
                };
                var secondsText = fields[secondsIndex];
                reading.TimeStamp = GetTimestamp(secondsText);
                for(int k = 0; k < hardDiskIndices.Count; k++)
                {
                    var hdIndex = hardDiskIndices[k];
                    var hdTempText = fields[hdIndex];
                    var hdTemp = GetTemp(hdTempText);
                    reading.HardDiskTemperatures[k] = hdTemp;
                }
                for (int k = 0; k < cpuIndices.Count; k++)
                {
                    var cpuIndex = cpuIndices[k];
                    var cpuTempText = fields[cpuIndex];
                    var cpuTemp = GetTemp(cpuTempText);
                    reading.CoreTemperatures[k] = cpuTemp;
                }
                readings.Add(reading);
            }
            return readings.ToArray();
        }

        public TempReading GetLatestTemperatureReading()
        {
            if (_readings.Any())
            {
                return _readings.Last();
            }
            return null;
        }

        private DateTime GetTimestamp(string secondsText)
        {
            try
            {
                var seconds = int.Parse(secondsText);
                var timestamp = _date.AddSeconds(seconds);
                return timestamp;
            }
            catch(Exception ex)
            {
                Log.Debug($"Unable to read seconds from secondsText {secondsText}. Error: {ex}");
                throw;
            }
        }

        private DateTime GetDate()
        {
            DateTime date;
            try
            {
                var fileName = Path.GetFileNameWithoutExtension(_filePath);
                var dateText = fileName.Substring(fileName.Length - 8);
                date = DateTime.ParseExact(dateText, "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal).ToUniversalTime();
                return date;
            }
            catch (Exception ex)
            {
                Log.Debug($"Unable to calculate date for file {_filePath}. Error: {ex}");
                throw;
            }
        }

        private decimal GetTemp(string tempText)
        {
            try
            {
                return decimal.Parse(tempText, CultureInfo.InvariantCulture);
            }
            catch(Exception ex)
            {
                Log.Debug($"Unable to read temp value for tempText {tempText}. Error: {ex}");
                throw;
            }
        }

        private static string[] GetFields(string line)
        {
            return line.Split('\t');
        }
    }
}
