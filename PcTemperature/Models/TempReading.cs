using PcTemperature.Helpers;
using System;

namespace PcTemperature.Models
{
    public class TempReading
    {
        public TempReading()
        {
            IsSuccessful = true;
            HardDiskTemperatures = new decimal[0];
            CoreTemperatures = new decimal[0];
        }

        public DateTime TimeStamp { get; set; }

        public decimal[] HardDiskTemperatures { get; set; }

        public decimal HardDiskTempAvg {  get { return HardDiskTemperatures.AverageOrDefault().RoundValue(2); } }

        public decimal HardDiskTempMax { get { return HardDiskTemperatures.MaxOrDefault().RoundValue(2); } }
        
        public decimal[] CoreTemperatures { get; set; }

        public decimal CoreTempAvg { get { return CoreTemperatures.AverageOrDefault().RoundValue(2); } }

        public decimal CoreTempMax {  get { return CoreTemperatures.MaxOrDefault().RoundValue(2); } }

        public bool IsSuccessful { get; set; }
    }
}
