using System.Collections.Generic;

namespace RCN.Solpe.DataBase.Core.ViewModels
{

  public class TokenRequest
  {
    public string application { get; set; }
    public bool sandbox { get; set; }
    public List<string> apns_tokens { get; set; }
  }
  public class TokenResponse
  {
    public List<Result> Results { get; set; }
  }
  public class Result
  {
    public string registration_token { get; set; }
    public string apns_token { get; set; }

    public string status { get; set; }
  }

  public class NotificationModel {
    public List<string> registration_ids { get; set; }
    public Notification notification { get; set; }

    public string image { get; set; }
    public string sound { get; set; }

  }

  public class Notification {
    public string tittle { get; set; }
    public string text { get; set; }
    public int badge { get; set; }
    public string category { get; set; }
    public bool showWhenInForeground { get; set; }

  }
}
