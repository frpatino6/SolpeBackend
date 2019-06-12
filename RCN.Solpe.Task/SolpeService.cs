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

    System.Timers.Timer timer = new System.Timers.Timer();
    private int eventId = 1;
    double interval = 60;

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
          interval = interval * 1000;
      }

      else
        solpeEventLog.WriteEntry("intervalExecution is not configured", EventLogEntryType.Error);
    }

    protected override void OnStart(string[] args)
    {

      solpeEventLog.WriteEntry("In OnStart");

      // Set up a timer that triggers every minute.

      timer.Interval = interval; // 60 seconds
      timer.Elapsed += new ElapsedEventHandler(this.OnTimer);
      timer.Start();
    }

    public void OnTimer(object sender, ElapsedEventArgs args)
    {

      try
      {
        solpeEventLog.WriteEntry("Monitoring the System", EventLogEntryType.Information, eventId++);

        IntegrationService.Instance.SendNotifications(solpeEventLog);

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
      timer.Elapsed += new ElapsedEventHandler(this.OnTimer);
      timer.Start();
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
