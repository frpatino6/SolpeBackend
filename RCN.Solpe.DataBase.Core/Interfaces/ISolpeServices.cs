using RCN.Solpe.DataBase.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace RCN.Solpe.DataBase.Core.Interfaces
{
  public interface ISolpeServices
  {
    Task<List<ZsdLiberaSolpe>> GetLiberaSolpes(string userName);

    Task<int> UpdateSolpe(string number);

    Task<int> SetAccessToken(string userName, string accessToken, string platform);

    Task<Zsd_Solpe_Access_Users> GetAccessUser(string userName, string platform, string accessToken);

    Task<Boolean> SendNotifications();
  }
}
