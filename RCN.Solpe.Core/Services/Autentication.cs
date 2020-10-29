using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
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
        private readonly ILogger<Autentication> _logger;
        private string sDefaultOU;
        private string passwordAdmin;
        private string userNameAdmin;
        private string sDomain;
        private string BaseOracleIntegrationUrl;
        public Autentication(IConfiguration iConfiguration, ILogger<Autentication> logger)
        {
            _IConfiguration = iConfiguration;
            sDefaultOU = _IConfiguration.GetSection("DomainConfigFS").GetSection("sDefaultOU").Value;
            passwordAdmin = _IConfiguration.GetSection("DomainConfigFS").GetSection("passwordAdmin").Value;
            userNameAdmin = _IConfiguration.GetSection("DomainConfigFS").GetSection("userNameAdmin").Value;
            sDomain = _IConfiguration.GetSection("DomainConfigFS").GetSection("sDomain").Value;
            BaseOracleIntegrationUrl = _IConfiguration.GetSection("OracleIntegration").GetSection("url").Value;
            _logger = logger;
        }

        /// <summary>
        /// Autentication from Active Directory
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public bool AutenticationUser(string userName, string password, string accesstoken, string platform)
        {
            _logger.LogInformation($"Autenticando {userName} {accesstoken} {platform}");
            using (PrincipalContext ctx = new PrincipalContext(ContextType.Domain, sDomain, userName, password))
            {
                bool result = ctx.ValidateCredentials(userName, password);//Valida el usuario y pws
                if (result)//Si lo encuentra, llama el servicio http que busca el registro el accesstoken el usario para la plataforma que está utilizando
                {
                    GetSolpeAccess(userName, accesstoken, platform);
                }
                else
                {
                    throw new Exception("Nombre de usuario y contraseña no válidos");
                }
                return result;
            }
        }

        public List<string> GetAllUsers()
        {
            using (PrincipalContext ctx = new PrincipalContext(ContextType.Domain, sDomain, userNameAdmin, passwordAdmin))

            {
                List<string> myDomainUsers = new List<string>();
                UserPrincipal userPrinciple = new UserPrincipal(ctx);


                using (PrincipalSearcher search = new PrincipalSearcher(userPrinciple))
                {
                    foreach (Principal domainUser in search.FindAll())
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
            _logger.LogInformation("GetSolpeAccess");
            using (HttpClient client = new HttpClient())
            {
                string url = BaseOracleIntegrationUrl + "/solpe/GetAccessUser/" + userName + "/" + accessToken + "/" + platform;
                HttpResponseMessage response = await client.GetAsync(url);

                _logger.LogInformation("Ejecutando url ...");
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();
                _logger.LogInformation($"Respuesta  {responseBody}");

            }
        }
    }
}
