using System.ServiceProcess;

namespace RCN.Solpe.Task
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
            //if (Environment.UserInteractive)
            //{
            //  SolpeService service1 = new SolpeService();
            //  service1.startConsole();
            //  Console.ReadLine();

            //}

            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[]
            {
                new SolpeService()
            };
            ServiceBase.Run(ServicesToRun);
        }
    }
}
