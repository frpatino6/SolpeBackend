using RCN.Solpe.Task.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace RCN.Solpe.Task
{
    public partial class SolpeService : ServiceBase
    {
        [Obsolete]
        private string BaseUrlRcn = System.Configuration.ConfigurationSettings.AppSettings["urlADIntegrationServicesRcn"].ToString();
        [Obsolete]
        private string BaseUrlWin = System.Configuration.ConfigurationSettings.AppSettings["urlADIntegrationServicesWin"].ToString();

        Timer timerRcn = new System.Timers.Timer();
        Timer timerWin = new System.Timers.Timer();

        private int eventId = 1;
        readonly double interval = 60;

        [Obsolete]
        public SolpeService()
        {
            InitializeComponent();
            solpeEventLog = new EventLog();

            if (!EventLog.SourceExists("SolpeSource"))
            {
                EventLog.CreateEventSource("SolpeSource", "SolpeLog");
            }
            solpeEventLog.Source = "SolpeSource";
            solpeEventLog.Log = "SolpeLog";


            if (System.Configuration.ConfigurationSettings.AppSettings["intervalExecution"] != null)
            {
                string value = System.Configuration.ConfigurationSettings.AppSettings["intervalExecution"].ToString();
                bool success = Double.TryParse(value, out interval);

                if (!success)
                    solpeEventLog.WriteEntry("intervalExecution it does not have a correct format", EventLogEntryType.Error);
                else
                    interval *= 1000;
            }

            else
                solpeEventLog.WriteEntry("intervalExecution is not configured", EventLogEntryType.Error);
        }

        protected override void OnStart(string[] args)
        {

            solpeEventLog.WriteEntry("In OnStart");

            // Set up a timer that triggers every minute.

            timerRcn.Interval = interval; // 60 seconds
            timerRcn.Elapsed += new ElapsedEventHandler(this.OnTimerRcn);
            timerRcn.Start();

            timerWin.Interval = interval; // 60 seconds
            timerWin.Elapsed += new ElapsedEventHandler(this.OnTimerWin);
            timerWin.Start();
        }

        public void OnTimerRcn(object sender, ElapsedEventArgs args)
        {

            try
            {
                solpeEventLog.WriteEntry("Monitoring the System", EventLogEntryType.Information, eventId++);

                IntegrationService.Instance.SendNotifications(solpeEventLog, BaseUrlRcn);

            }
            catch (Exception ex)
            {
                if (ex.InnerException != null)
                    solpeEventLog.WriteEntry("!!!ERROR!!!" + ex.InnerException, EventLogEntryType.Error);
                else
                    solpeEventLog.WriteEntry("!!!ERROR!!!" + ex.Message, EventLogEntryType.Error);

            }


        }

        public void OnTimerWin(object sender, ElapsedEventArgs args)
        {

            try
            {
                solpeEventLog.WriteEntry("Monitoring the System", EventLogEntryType.Information, eventId++);

                IntegrationService.Instance.SendNotifications(solpeEventLog,BaseUrlWin);

            }
            catch (Exception ex)
            {
                if (ex.InnerException != null)
                    solpeEventLog.WriteEntry("!!!ERROR!!!" + ex.InnerException, EventLogEntryType.Error);
                else
                    solpeEventLog.WriteEntry("!!!ERROR!!!" + ex.Message, EventLogEntryType.Error);

            }


        }

        protected override void OnStop()
        {
            solpeEventLog.WriteEntry("In OnStop");
        }

        protected override void OnContinue()
        {
            solpeEventLog.WriteEntry("In OnContinue.");
        }

        public void startConsole()
        {
            solpeEventLog.WriteEntry("In OnStart");

            // Set up a timer that triggers every minute.
            System.Timers.Timer timer = new System.Timers.Timer();
            timer.Interval = interval; // 60 seconds
            timer.Elapsed += new ElapsedEventHandler(this.OnTimerRcn);
            timer.Start();

            System.Timers.Timer timerWin = new System.Timers.Timer();
            timerWin.Interval = interval; // 60 seconds
            timerWin.Elapsed += new ElapsedEventHandler(this.OnTimerWin);
            timerWin.Start();
        }
        public void RunAsConsole(string[] args)
        {
            OnStart(args);
            Console.WriteLine("Press any key to exit...");
            Console.ReadLine();
            OnStop();
        }
    }
}
