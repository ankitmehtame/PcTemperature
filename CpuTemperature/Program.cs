using log4net;
using System;
using System.ServiceProcess;

namespace CpuTemperature
{
    static class Program
    {
        static readonly ILog Log = LogManager.GetLogger(typeof(Program));

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            log4net.Config.XmlConfigurator.Configure();
            try
            {
                if (!Environment.UserInteractive)
                {
                    Log.Info("Starting service");
                    ServiceBase[] ServicesToRun;
                    ServicesToRun = new ServiceBase[]
                    {
                        new CpuTempReaderService()
                    };
                    ServiceBase.Run(ServicesToRun);
                }
                else
                {
                    Log.Info("Starting as interactive console app. Press enter to exit.");
                    var service = new CpuTempReaderService();
                    service.StartService(args);
                    Console.ReadLine();
                    service.StopService();
                }
            }
            catch (Exception ex)
            {
                Log.ErrorFormat("Unhandled exception. {0}\r\n{1}", ex.Message, ex.StackTrace);
            }
            finally
            {
                Log.InfoFormat("Stopped");
            }
        }
    }
}
