using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Oracle.ManagedDataAccess.Client;
using RCN.Solpe.DataBase.Core.Interfaces;
using RCN.Solpe.DataBase.Core.ViewModels;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace RCN.Solpe.DataBase.Core.Services
{
    public class SolpeServices : ISolpeServices
    {
        private readonly IConfiguration _IConfiguration;
        private readonly ILogger<SolpeServices> _logger;
        private string Mandante;

        private int countRecords = 0;
        public SolpeServices(IConfiguration configuration, ILogger<SolpeServices> logger)
        {
            _IConfiguration = configuration;
            _logger = logger;

        }


        /// <summary>
        /// Get user from access token table
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="platform"></param>
        /// <returns></returns>
        public async Task<Zsd_Solpe_Access_Users> GetAccessUser(string userName, string platform, string accessToken)
        {
            string conString = GetOracleConnectionParameters();
            int result = 0;
            string sourceToken = accessToken;

            if (!platform.Equals("android"))
            {
                accessToken = GenerateIosAccessToken(accessToken);
            }

            Zsd_Solpe_Access_Users _zsd_Solpe_Access_Users = null;
            using (OracleConnection con = new OracleConnection(conString))
            {
                using (OracleCommand cmd = con.CreateCommand())
                {

                    con.Open();
                    cmd.BindByName = true;

                    OracleParameter pUserName = new OracleParameter
                    {
                        DbType = DbType.String,
                        Value = userName.ToUpper(),
                        ParameterName = "userName"
                    };
                    cmd.Parameters.Add(pUserName);

                    OracleParameter pPlatfor = new OracleParameter
                    {
                        DbType = DbType.String,
                        Value = platform,
                        ParameterName = "platform"
                    };

                    cmd.Parameters.Add(pPlatfor);


                    cmd.CommandText = "select * from ZSD_SOLPE_ACCESS_USERS  where username=:userName and platform = :platform";

                    OracleDataReader reader = cmd.ExecuteReader();


                    while (reader.Read())
                    {
                        _zsd_Solpe_Access_Users = new Zsd_Solpe_Access_Users
                        {
                            Id = reader.GetInt32(0),
                            UserName = reader.GetString(1).ToUpper(),
                            AccessToken = reader.GetString(2),
                            Platform = reader.GetString(3)
                        };

                    }

                    if (_zsd_Solpe_Access_Users != null) //Valida si el usuario tiene el mismo accessToken que el enviado por el dispositivo
                    {
                        result = await SetAccessToken(userName, accessToken, platform); //si no es el mismo, actualiza el accessToken en la base de datos
                        _logger.LogInformation("Actualizando token par envio de notificaciones");
                    }

                    else if (_zsd_Solpe_Access_Users == null)//Si no se encontro el access token del usuario, se procede a registrar el usuario con su accessToken y plataforma 
                    {
                        result = CreateSolpeAccessUser(userName, platform, accessToken, sourceToken);
                        _logger.LogInformation("Generando token ");
                    }


                    reader.Dispose();
                }

            }

            return _zsd_Solpe_Access_Users;
        }


        /// <summary>
        /// Return solpes by user
        /// </summary>
        /// <param name="userName"></param>
        /// <returns></returns>
        public async Task<List<ZsdLiberaSolpe>> GetLiberaSolpes(string userName = "")
        {
            string conString = GetOracleConnectionParameters();
            Mandante = _IConfiguration.GetSection("mandanteCode").Value.ToString();

            _logger.LogInformation("Iniciando GetLiberaSolpes para " + userName.ToUpper());

            DataSet resultDataSet;
            List<ZsdLiberaSolpe> result = new List<ZsdLiberaSolpe>();
            using (OracleConnection con = new OracleConnection(conString))
            {
                using (OracleCommand cmd = con.CreateCommand())
                {

                    con.Open();
                    cmd.BindByName = true;

                    OracleParameter pPlayerNum = new OracleParameter
                    {
                        DbType = DbType.String,
                        Value = userName.ToUpper(),
                        ParameterName = "nombre"
                    };
                    cmd.Parameters.Add(pPlayerNum);


                    OracleParameter estado = new OracleParameter
                    {
                        DbType = DbType.String,
                        Value = "E",
                        ParameterName = "estado"
                    };
                    cmd.Parameters.Add(estado);


                    OracleParameter mandante = new OracleParameter
                    {
                        DbType = DbType.String,
                        Value = Mandante,
                        ParameterName = "mandante"
                    };
                    cmd.Parameters.Add(mandante);

                    if (!string.IsNullOrEmpty(userName))
                    {
                        _logger.LogInformation("Consultando solpes por usuario " + userName.ToUpper());
                        cmd.CommandText = "select * from ZSD_LIBERA_SOLPE where UPPER(usuario)=:nombre and estado='I' and MANDANTE=:mandante";
                    }
                    else
                    {
                        _logger.LogInformation("Consultando solpes para todos en estado I ");
                        cmd.CommandText = $@"select s.usuario,u.access_token 
                                                ,(
                                                 (SELECT count(COUNT(NUMERO)) FROM ZSD_SOLPE_NOTIFICADOS 
                                                 WHERE TIPO_DOC='P' AND  estado='I' and MANDANTE=:mandante
                                                 GROUP BY NUMERO)) as NUMERO_PEDIDOS 
                                                ,(
                                                 (SELECT count(COUNT(NUMERO)) FROM ZSD_SOLPE_NOTIFICADOS 
                                                 WHERE TIPO_DOC='S' AND  estado='I' and MANDANTE=:mandante
                                                 GROUP BY NUMERO)) AS NUMERO_SOLPES
                                              from ZSD_SOLPE_NOTIFICADOS s 
                                              inner join zsd_solpe_access_users u on UPPER(s.usuario)=UPPER(u.username) 
                                              where  estado='I' and MANDANTE=:mandante
                                              group by s.usuario,u.access_token";
                    }

                    //and TRUNC(u.ULTIMA_NOTIFICACION) = TO_DATE('{DateTime.Now}','dd/mon/yyyy')
                    //OracleDataReader reader = cmd.ExecuteReader();

                    OracleDataAdapter adapter = new OracleDataAdapter(cmd);
                    resultDataSet = new DataSet("resultDataSet");
                    adapter.Fill(resultDataSet, "result");

                    _logger.LogInformation("GetLiberaSolpes ejecutado satisfactóriamente ");
                    countRecords = 0;

                    if (!string.IsNullOrEmpty(userName))
                    {
                        foreach (DataTable table in resultDataSet.Tables)
                        {
                            foreach (DataRow dr in table.Rows)
                            {
                                ZsdLiberaSolpe zsdLiberaSolpe = new ZsdLiberaSolpe
                                {
                                    Mandante = dr["MANDANTE"].ToString(),
                                    Tipo_Doc = dr["TIPO_DOC"].ToString(),
                                    Numero = dr["NUMERO"].ToString(),
                                    Posicion = Convert.ToInt32(dr["POSICION"].ToString()),
                                    Texto = dr["TEXTO"].ToString(),
                                    Cantidad = Convert.ToDecimal(dr["CANTIDAD"].ToString()),
                                    Valor = Convert.ToDecimal(dr["VALOR"].ToString()),
                                    Usuario = dr["USUARIO"].ToString(),
                                    Estado = dr["ESTADO"].ToString(),
                                    Destino = dr["DESTINO"].ToString(),
                                    Proveedor = dr["PROVEEDOR"].ToString(),
                                    Moneda = dr["MONEDA"].ToString()
                                };
                                result.Add(zsdLiberaSolpe);
                            }
                        }
                    }
                    else
                    {
                        foreach (DataTable table in resultDataSet.Tables)
                        {
                            foreach (DataRow dr in table.Rows)
                            {
                                ZsdLiberaSolpe zsdLiberaSolpe = new ZsdLiberaSolpe
                                {

                                    Usuario = dr["USUARIO"].ToString(),
                                    Access_Token = dr["ACCESS_TOKEN"].ToString(),
                                    Numero = Convert.ToString(Convert.ToInt32(dr["NUMERO_PEDIDOS"].ToString()) + Convert.ToInt32(dr["NUMERO_SOLPES"].ToString())),
                                };
                                result.Add(zsdLiberaSolpe);
                            }
                        }
                    }
                }
            }
            if (result.Count > 0)
            {
                _logger.LogInformation("Solpes encontradas " + result.Count);
            }
            else
            {
                _logger.LogInformation("No se encontraron registros ");
            }

            return result;
        }

        /// <summary>
        /// add firebase access token by user
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="accessToken"></param>
        /// <param name="platform"></param>
        /// <returns></returns>
        public async Task<int> SetAccessToken(string userName, string accessToken, string platform)
        {
            int rowCount = 0;
            string conString = GetOracleConnectionParameters();
            using (OracleConnection con = new OracleConnection(conString))
            {
                OracleCommand oraUpdate = null;
                try
                {
                    con.Open();
                    string sqlupdate = "update ZSD_SOLPE_ACCESS_USERS set Access_Token=:accesstoken  where UserName=:userName  and platform=:platform";
                    oraUpdate = new OracleCommand(sqlupdate, con);

                    OracleParameter OraParamAccessToken = new OracleParameter(":accesstoken", OracleDbType.Varchar2, 300);
                    OracleParameter OraParamUserName = new OracleParameter(":userName", OracleDbType.Varchar2, 100);
                    OracleParameter OraParamPlatform = new OracleParameter(":platform", OracleDbType.Varchar2, 10);

                    OraParamAccessToken.Value = accessToken;
                    OraParamUserName.Value = userName;
                    OraParamPlatform.Value = platform;


                    oraUpdate.Parameters.Add(OraParamAccessToken);
                    oraUpdate.Parameters.Add(OraParamUserName);
                    oraUpdate.Parameters.Add(OraParamPlatform);

                    rowCount = oraUpdate.ExecuteNonQuery();


                }
                catch (Exception)
                {

                }
                finally
                {
                    oraUpdate.Dispose();

                }

            }
            return rowCount;
        }

        /// <summary>
        /// Crea histórico de los pedidos notificados por usuario
        /// </summary>
        private void SetSolpeNotification(string user, string numero)
        {
        }

        public async Task<int> UpdateSolpe(string number, int posicion)
        {
            int rowCount = 0;
            string conString = GetOracleConnectionParameters();
            using (OracleConnection con = new OracleConnection(conString))
            {
                using (OracleCommand cmd = con.CreateCommand())
                {
                    try
                    {
                        con.Open();

                        cmd.CommandText = "update  ZSD_LIBERA_SOLPE set estado ='N' where numero=:numero and posicion=:posicion ";
                        OracleParameter numero = new OracleParameter
                        {
                            DbType = DbType.String,
                            Value = number,
                            ParameterName = "numero"
                        };
                        cmd.Parameters.Add(numero);

                        OracleParameter pos = new OracleParameter
                        {
                            DbType = DbType.Int32,
                            Value = posicion,
                            ParameterName = "posicion"
                        };
                        cmd.Parameters.Add(pos);
                        // Execute Command (for Delete, Insert,Update).
                        rowCount = cmd.ExecuteNonQuery();
                        _logger.LogInformation("UpdateSolpe correctamente " + rowCount);

                    }
                    catch (Exception ex)
                    {
                        _logger.LogError("Error en servicio ==>UpdateSolpe" + ex.Message);
                    }
                }
            }
            return rowCount;

        }

        public async Task<int> UpdatePedido(string number)
        {
            int rowCount = 0;
            string conString = GetOracleConnectionParameters();
            using (OracleConnection con = new OracleConnection(conString))
            {
                using (OracleCommand cmd = con.CreateCommand())
                {
                    try
                    {
                        con.Open();

                        cmd.CommandText = "update  ZSD_LIBERA_SOLPE set estado ='N' where numero=:numero ";
                        OracleParameter numero = new OracleParameter
                        {
                            DbType = DbType.String,
                            Value = number,
                            ParameterName = "numero"
                        };
                        cmd.Parameters.Add(numero);

                        rowCount = cmd.ExecuteNonQuery();
                        _logger.LogInformation("UpdatePedido correctamente " + rowCount);

                    }
                    catch (Exception ex)
                    {
                        _logger.LogError("Error en servicio ==>UpdatePedido" + ex.Message);
                    }
                }
            }
            return rowCount;

        }

        /// <summary>
        /// Cambia el estado de un pedido a N cuando el evio de la notificación fue correcto
        /// </summary>
        /// <param name="number"></param>
        /// <returns></returns>
        public async Task<int> UpdateSolpeNotificationState(string usuario)
        {
            int rowCount = 0;
            _logger.LogInformation("iniciando  UpdateSolpeNotificationState");

            string conString = GetOracleConnectionParameters();
            using (OracleConnection con = new OracleConnection(conString))
            {
                using (OracleCommand cmd = con.CreateCommand())
                {
                    try
                    {
                        con.Open();

                        cmd.CommandText = "update  ZSD_SOLPE_NOTIFICADOS set estado ='E' where usuario=:usuario ";
                        OracleParameter pPlayerNum = new OracleParameter
                        {
                            DbType = DbType.String,
                            Value = usuario,
                            ParameterName = "usuario"
                        };
                        cmd.Parameters.Add(pPlayerNum);
                        // Execute Command (for Delete, Insert,Update).
                        rowCount = cmd.ExecuteNonQuery();
                        _logger.LogInformation(" ZSD_SOLPE_NOTIFICADOS set estado ='E'");

                    }
                    catch (Exception ex)
                    {
                        _logger.LogError("Error en servicio ==>UpdateSolpeNotificationState" + ex.Message);
                        throw new Exception(ex.Message);
                    }
                }
            }
            return rowCount;

        }

        private int CreateSolpeAccessUser(string userName, string platform, string accessToken, string iosToken = "")
        {
            _logger.LogInformation($"CreateSolpeAccessUser {userName} {platform}");
            int rowCount = 0;
            string conString = GetOracleConnectionParameters();
            using (OracleConnection con = new OracleConnection(conString))
            {
                OracleCommand oraUpdate = null;
                try
                {
                    con.Open();
                    string sqlupdate = "insert into ZSD_SOLPE_ACCESS_USERS (USERNAME,ACCESS_TOKEN,PLATFORM,IOS_TOKEN) values(:userName,:accesstoken,:platform,:ios_token)";
                    oraUpdate = new OracleCommand(sqlupdate, con);

                    OracleParameter OraParamAccessToken = new OracleParameter(":accesstoken", OracleDbType.Varchar2, 500);
                    OracleParameter OraParamUserName = new OracleParameter(":userName", OracleDbType.Varchar2, 100);
                    OracleParameter OraParamPlatform = new OracleParameter(":platform", OracleDbType.Varchar2, 10);
                    OracleParameter OraParamIosToken = new OracleParameter(":ios_token", OracleDbType.Varchar2, 500);

                    OraParamAccessToken.Value = accessToken;
                    OraParamUserName.Value = userName.ToUpper();
                    OraParamPlatform.Value = platform;
                    OraParamIosToken.Value = iosToken;

                    oraUpdate.Parameters.Add(OraParamUserName);
                    oraUpdate.Parameters.Add(OraParamAccessToken);
                    oraUpdate.Parameters.Add(OraParamPlatform);
                    oraUpdate.Parameters.Add(OraParamIosToken);

                    rowCount = oraUpdate.ExecuteNonQuery();
                    _logger.LogInformation("CreateSolpeAccessUser ejecutado con éxito " + rowCount);

                }
                catch (Exception ex)
                {

                    _logger.LogError("Error en servicio ==>CreateSolpeAccessUser" + ex.Message);
                    throw new Exception(ex.Message);
                }
                finally
                {
                    oraUpdate.Dispose();

                }

            }
            _logger.LogInformation($"CreateSolpeAccessUser resultado {rowCount}");
            return rowCount;

        }

        private string GetOracleConnectionParameters()
        {
            string userNameDatabase = _IConfiguration.GetSection("OracleConfig").GetSection("userId").Value;
            string pws = _IConfiguration.GetSection("OracleConfig").GetSection("pws").Value;
            string datasource = _IConfiguration.GetSection("OracleConfig").GetSection("dataSource").Value;

            string conString = $"User Id={userNameDatabase};Password={pws};" +

            //How to connect to an Oracle DB without SQL*Net configuration file
            //  also known as tnsnames.ora.
            $"Data Source={datasource}";
            return conString;
        }

        private string GenerateIosAccessToken(string accessToken)
        {
            string aplicationName = _IConfiguration.GetSection("aplicationName").Value.ToString();
            string authorizationKey = _IConfiguration.GetSection("authorization").Value.ToString();

            TokenRequest requestBody = new TokenRequest
            {
                application = aplicationName,
                sandbox = false,
                apns_tokens = new List<string>()
            };
            requestBody.apns_tokens.Add(accessToken);

            string output = JsonConvert.SerializeObject(requestBody);


            RestClient client = new RestClient("https://iid.googleapis.com/iid/v1:batchImport");

            RestRequest request = new RestRequest(Method.POST);
            request.AddHeader("postman-token", "f57e37d9-1492-e9da-943a-d7773f9b3551");
            request.AddHeader("cache-control", "no-cache");
            request.AddHeader("authorization", authorizationKey);
            request.AddHeader("content-type", "application/json");
            request.AddParameter("application/json", output, ParameterType.RequestBody);
            IRestResponse response = client.Execute(request);
            TokenResponse result = JsonConvert.DeserializeObject<TokenResponse>(response.Content,
                          new JsonSerializerSettings
                          {
                              PreserveReferencesHandling = PreserveReferencesHandling.Objects
                          });
            if (result != null && result.Results.Count > 0)
            {
                _logger.LogInformation("GenerateIosAccessToken ejecutado con éxito " + result.Results[0].registration_token);
                return result.Results[0].registration_token;
            }
            else
            {
                _logger.LogInformation("Error generando el token de accesso");
                throw new Exception("Error generando el token de accesso. Comunicarse con el administrador ");
            }
        }

        public async Task<bool> SendNotifications()
        {
            // llama el servicio que retorna las solpes en estado E
            string authorizationNotification = _IConfiguration.GetSection("authorizationNotification").Value.ToString();

            List<ZsdLiberaSolpe> listresult = await GetLiberaSolpes();
            List<string> notificationResult = new List<string>();
            NotificationsResult res = new NotificationsResult();


            foreach (ZsdLiberaSolpe item in listresult)
            {
                NotificationModel requestBody = new NotificationModel
                {
                    registration_ids = new List<string>()
                };
                requestBody.registration_ids.Add(item.Access_Token);
                requestBody.notification = new Notification
                {
                    badge = Convert.ToInt16(item.Numero),
                    tittle = "Notificación de pedidos SOLPE",
                    text = "Ha sido enviada un solicitud-pedido a su nombre",
                    category = "GENERAL",
                    showWhenInForeground = true
                };
                requestBody.image = "https://firebase.google.com/images/social.png";
                requestBody.sound = "default";


                RestClient client = new RestClient("https://fcm.googleapis.com/fcm/send");
                RestRequest request = new RestRequest(Method.POST);
                request.AddHeader("postman-token", "f57e37d9-1492-e9da-943a-d7773f9b3551");
                request.AddHeader("cache-control", "no-cache");
                request.AddHeader("authorization", authorizationNotification);
                request.AddHeader("content-type", "application/json");

                string output = JsonConvert.SerializeObject(requestBody);

                _logger.LogInformation("Respuesta solicitud http de notificación" + output);

                request.AddParameter("application/json", output, ParameterType.RequestBody);
                IRestResponse response = client.Execute(request);

                if (response.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    _logger.LogError("Error enviando notificaciones " + response.ErrorMessage);
                    throw new Exception("Error enviando notificaciones. Comunicarse con el administrador " + response.ErrorMessage);
                }
                else
                {
                    res = JsonConvert.DeserializeObject<NotificationsResult>(response.Content);
                    _logger.LogInformation("Notificación enviada con éxito " + output);
                    if (res.failure == 0)
                    {
                        await UpdateSolpeNotificationState(item.Usuario);
                    }
                    else
                    {
                        _logger.LogCritical($"Error enviando notificación a: {item.Usuario}" + output);
                    }
                }
            }



            return true;
        }

    }
}
