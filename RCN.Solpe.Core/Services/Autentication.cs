using Microsoft.Extensions.Configuration;
using RCN.Solpe.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.DirectoryServices.AccountManagement;
using System.Net.Http;

namespace RCN.Solpe.Core.Services
{
  public class Autentication : IAutentication
  {
    private readonly IConfiguration _IConfiguration;
    private string sDefaultOU;
    private string passwordAdmin;
    private string userNameAdmin;
    private string sDomain;
    private string BaseOracleIntegrationUrl;
    public Autentication(IConfiguration iConfiguration)
    {
      _IConfiguration = iConfiguration;
      sDefaultOU = _IConfiguration.GetSection("DomainConfigFS").GetSection("sDefaultOU").Value;
      passwordAdmin = _IConfiguration.GetSection("DomainConfigFS").GetSection("passwordAdmin").Value;
      userNameAdmin = _IConfiguration.GetSection("DomainConfigFS").GetSection("userNameAdmin").Value;
      sDomain = _IConfiguration.GetSection("DomainConfigFS").GetSection("sDomain").Value;
      BaseOracleIntegrationUrl = _IConfiguration.GetSection("OracleIntegration").GetSection("url").Value;
    }

    /// <summary>
    /// Autentication from Active Directory
    /// </summary>
    /// <param name="userName"></param>
    /// <param name="password"></param>
    /// <returns></returns>
    public bool AutenticationUser(string userName, string password, string accesstoken, string platform)
    {
      using (var ctx = new PrincipalContext(ContextType.Domain, sDomain, userName, password))
      {
        var result = ctx.ValidateCredentials(userName, password);//Valida el usuario y pws
        if (result)//Si lo encuentra, llama el servicio http que busca el registro el accesstoken el usario para la plataforma que está utilizando
        {
          GetSolpeAccess(userName, accesstoken, platform);
        }
        else {
          throw new Exception("Nombre de usuario y contraseña no válido");
        }
        return result;

      }
    }

    public List<string> GetAllUsers()
    {
      using (var ctx = new PrincipalContext(ContextType.Domain, sDomain, userNameAdmin, passwordAdmin))

      {
        var myDomainUsers = new List<string>();
        var userPrinciple = new UserPrincipal(ctx);


        using (var search = new PrincipalSearcher(userPrinciple))
        {
          foreach (var domainUser in search.FindAll())
          {
            if (domainUser.SamAccountName != null)
            {
              myDomainUsers.Add(domainUser.SamAccountName);
            }
          }
          return myDomainUsers;
        }
      }
    }

    public async void GetSolpeAccess(string userName, string accessToken, string platform)
    {
      using (HttpClient client = new HttpClient())
      {
        var url = BaseOracleIntegrationUrl + "/solpe/GetAccessUser/" + userName + "/" + accessToken + "/" + platform;
        HttpResponseMessage response = await client.GetAsync(url);
        response.EnsureSuccessStatusCode();
        string responseBody = await response.Content.ReadAsStringAsync();

      }
    }
  }
}
