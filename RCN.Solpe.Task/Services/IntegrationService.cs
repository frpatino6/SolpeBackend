using System.Net.Http;

namespace RCN.Solpe.Task.Services
{
  public class IntegrationService
  {
    public string SendNotifications()
    {

      CallGetLiberaSolpe();
      return string.Empty;
    }

    private void CallGetLiberaSolpe()
    {
      using (HttpClient client = new HttpClient())
      {
        try
        {

        }
        catch
        {
        }
      }
    }
  }
}
