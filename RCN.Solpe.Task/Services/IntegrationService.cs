using System;
using System.Diagnostics;
using System.Net.Http;

namespace RCN.Solpe.Task.Services
{
    public class IntegrationService
    {
        private static IntegrationService instance;
       


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

        public string SendNotifications(EventLog eventIntegration, string baseUrl)
        {

            CallGetLiberaSolpeRcn(eventIntegration, baseUrl);
            return string.Empty;
        }

        private async void CallGetLiberaSolpeRcn(EventLog eventIntegration, string baseUrl)
        {
            if (string.IsNullOrEmpty(baseUrl))
            {
                throw new ArgumentException("No se ha ingresado valor para el destino de las notificaciones", nameof(baseUrl));
            }
            //Formatea la fecha
            var dateToExecute = DateTime.Now.ToString("yyyy-MM-dd");

            //Construye la url para consultar sí hay usuarios a eliminar
            string url = $"{baseUrl}/solpe/SendNotifications/";

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
                catch (Exception ex)
                {
                    eventIntegration.WriteEntry(ex.Message, EventLogEntryType.Error);
                }
            }
        }
    }
}
