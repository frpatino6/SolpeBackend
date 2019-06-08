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
}
