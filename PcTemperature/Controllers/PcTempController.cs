using PcTemperature.Helpers;
using PcTemperature.Models;
using PcTemperature.Services;
using log4net;
using System;
using System.Configuration;
using System.Threading.Tasks;
using System.Web.Http;

namespace PcTemperature.Controllers
{
    public class PcTempController : ApiController
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(PcTempController));

        [HttpGet]
        [ActionName("reading")]
        public async Task<TempReading> GetLatestReading()
        {
            try
            {
                Log.Debug("Request to get readings");
                var filePath = GetTempLogFilePath();
                var reader = new SpeedFanFileReader(filePath);
                await reader.Init();
                var reading = reader.GetLatestTemperatureReading();
                if (reading == null)
                {
                    reading = TempReadingExtensions.CreateUnsuccessfulReading();
                }
                else if (reading.TimeStamp <= DateTime.UtcNow.Subtract(TimeSpan.FromMinutes(1)))
                {
                    Log.Debug($"Last reading is not recent. Taken at {reading.TimeStamp.ToString("yyyyMMdd HH:mm:ss")}");
                    return reading = TempReadingExtensions.CreateUnsuccessfulReading();
                }
                Log.Debug("Returning reading " + await reading.ToDisplayText());
                return reading;
            }
            catch (AggregateException ex)
            {
                Log.Debug($"Aggregate Error in GetReadings. Error: {ex}");
                return TempReadingExtensions.CreateUnsuccessfulReading();
            }
            catch (Exception ex)
            {
                Log.Debug($"Error in GetReadings. Error: {ex}");
                return TempReadingExtensions.CreateUnsuccessfulReading();
            }
        }

        private string GetTempLogFilePath()
        {
            var filePathTemplate = ConfigurationManager.AppSettings["TEMP_LOG_FILE"];
            var dateFormat = ConfigurationManager.AppSettings["TEMP_LOG_FILE_DATE_FORMAT"];
            var filePath = filePathTemplate.Replace("{{DATE}}", DateTime.Now.ToString(dateFormat))
                .Replace("{{DATEUTC}}", DateTime.UtcNow.ToString(dateFormat));
            return filePath;
        }
    }
}
