using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace RCN.Solpe.Core.Interfaces
{
  public interface IAutentication
  {
    Boolean AutenticationUser(string userName, string password, string accesstoken, string platfor);
    List<string> GetAllUsers();

    void GetSolpeAccess(string userName, string accessToken, string platfor);

  }
}
