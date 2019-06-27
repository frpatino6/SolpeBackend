using System.Collections.Generic;

namespace RCN.Solpe.DataBase.Core.ViewModels
{
  public class ResultList
  {
    public string message_id { get; set; }
  }
  public class NotificationsResult
  {
    public long multicast_id { get; set; }
    public int success { get; set; }
    public int failure { get; set; }
    public int canonical_ids { get; set; }
    public List<ResultList> results { get; set; }
  }
}
