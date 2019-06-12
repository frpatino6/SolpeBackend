using System;
using System.Diagnostics;
using System.Net.Http;

namespace RCN.Solpe.Task.Services
{
  public class IntegrationService
  {
    private static IntegrationService instance;
    private string BaseUrl = System.Configuration.ConfigurationSettings.AppSettings["urlADIntegrationServices"].ToString();


    public static IntegrationService Instance
    {
      get
      {
        if (instance == null)
        {
          instance = new IntegrationService();
        }
        return instance;
      }
    }

    public string SendNotifications(EventLog eventIntegration)
    {

      CallGetLiberaSolpe(eventIntegration);
      return string.Empty;
    }

    private async void CallGetLiberaSolpe(EventLog eventIntegration)
    {
      //Formatea la fecha
      var dateToExecute = DateTime.Now.ToString("yyyy-MM-dd");

      //Construye la url para consultar sí hay usuarios a eliminar
      var url = BaseUrl + "/solpe/SendNotifications/";

      eventIntegration.WriteEntry("Calling: " + url);

      using (HttpClient client = new HttpClient())
      {
        try
        {
          //LLama el servicio GetSheduleTaskByExecution
          HttpResponseMessage response = await client.GetAsync(url);
          response.EnsureSuccessStatusCode();
          string responseBody = await response.Content.ReadAsStringAsync();

          eventIntegration.WriteEntry("resultado GetLiberaSolpes" + responseBody, EventLogEntryType.SuccessAudit);
        }
        catch(Exception ex)
        {
          eventIntegration.WriteEntry(ex.Message, EventLogEntryType.Error);
        }
      }
    }
  }
}
