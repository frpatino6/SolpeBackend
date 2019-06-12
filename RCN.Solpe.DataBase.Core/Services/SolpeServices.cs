﻿using Microsoft.Extensions.Configuration;
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

    public SolpeServices(IConfiguration configuration)
    {
      _IConfiguration = configuration;
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
        accessToken = GenerateIosAccessToken(accessToken);


      Zsd_Solpe_Access_Users _zsd_Solpe_Access_Users = null;
      using (OracleConnection con = new OracleConnection(conString))
      {
        using (OracleCommand cmd = con.CreateCommand())
        {

          con.Open();
          cmd.BindByName = true;

          OracleParameter pUserName = new OracleParameter();
          pUserName.DbType = DbType.String;
          pUserName.Value = userName;
          pUserName.ParameterName = "userName";
          cmd.Parameters.Add(pUserName);

          OracleParameter pPlatfor = new OracleParameter();
          pPlatfor.DbType = DbType.String;
          pPlatfor.Value = platform;
          pPlatfor.ParameterName = "platform";

          cmd.Parameters.Add(pPlatfor);


          cmd.CommandText = "select * from ZSD_SOLPE_ACCESS_USERS  where username=:userName and platform = :platform";

          OracleDataReader reader = cmd.ExecuteReader();


          while (reader.Read())
          {
            _zsd_Solpe_Access_Users = new Zsd_Solpe_Access_Users();
            _zsd_Solpe_Access_Users.Id = reader.GetInt32(0);
            _zsd_Solpe_Access_Users.UserName = reader.GetString(1);
            _zsd_Solpe_Access_Users.AccessToken = reader.GetString(2);
            _zsd_Solpe_Access_Users.Platform = reader.GetString(3);

          }

          if (_zsd_Solpe_Access_Users != null && accessToken != _zsd_Solpe_Access_Users.AccessToken) //Valida si el usuario tiene el mismo accessToken que el enviado por el dispositivo
            result = await SetAccessToken(userName, accessToken, platform); //si no es el mismo, actualiza el accessToken en la base de datos
          else if (_zsd_Solpe_Access_Users == null)//Si no se encontro el access token del usuario, se procede a registrar el usuario con su accessToken y plataforma 
            result = CreateSolpeAccessUser(userName, platform, accessToken, sourceToken);

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


      List<ZsdLiberaSolpe> result = new List<ZsdLiberaSolpe>();
      using (OracleConnection con = new OracleConnection(conString))
      {
        using (OracleCommand cmd = con.CreateCommand())
        {

          con.Open();
          cmd.BindByName = true;

          OracleParameter pPlayerNum = new OracleParameter();
          pPlayerNum.DbType = DbType.String;
          pPlayerNum.Value = userName;
          pPlayerNum.ParameterName = "nombre";
          cmd.Parameters.Add(pPlayerNum);

          if (!string.IsNullOrEmpty(userName))
            cmd.CommandText = "select * from ZSD_LIBERA_SOLPE  where usuario=:nombre and estado!='E'";
          else
            cmd.CommandText = @"select s.usuario,u.access_token  from zsd_libera_solpe s
                                inner join zsd_solpe_access_users u on s.usuario=u.username
                                where   s.estado IS NULL
                                group by s.usuario,u.access_token";

          OracleDataReader reader = cmd.ExecuteReader();

          if (!string.IsNullOrEmpty(userName))
            while (reader.Read())
            {
              ZsdLiberaSolpe zsdLiberaSolpe = new ZsdLiberaSolpe();
              zsdLiberaSolpe.Mandante = reader.GetString(0);
              zsdLiberaSolpe.Tipo_Doc = reader.GetString(1);
              zsdLiberaSolpe.Numero = reader.GetString(2);
              zsdLiberaSolpe.Posicion = reader.GetInt32(3);
              zsdLiberaSolpe.Texto = reader.GetString(4);
              zsdLiberaSolpe.Cantidad = reader.GetDecimal(5);
              zsdLiberaSolpe.Valor = reader.GetDecimal(6);
              zsdLiberaSolpe.Usuario = reader.GetString(7);
              zsdLiberaSolpe.Estado = reader.GetString(8).ToString();
              result.Add(zsdLiberaSolpe);
            }
          else
            while (reader.Read())
            {
              ZsdLiberaSolpe zsdLiberaSolpe = new ZsdLiberaSolpe();

              zsdLiberaSolpe.Usuario = reader.GetString(0);
              zsdLiberaSolpe.Access_Token = reader.GetString(1).ToString();
              result.Add(zsdLiberaSolpe);
            }
          reader.Dispose();

        }
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


    public async Task<int> UpdateSolpe(int number)
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

            cmd.CommandText = "update  ZSD_LIBERA_SOLPE set estado ='E' where numero=:numero ";
            OracleParameter pPlayerNum = new OracleParameter();
            pPlayerNum.DbType = DbType.String;
            pPlayerNum.Value = number;
            pPlayerNum.ParameterName = "numero";
            cmd.Parameters.Add(pPlayerNum);
            // Execute Command (for Delete, Insert,Update).
            rowCount = cmd.ExecuteNonQuery();

          }
          catch (Exception)
          {

          }
        }
      }
      return rowCount;

    }

    private int CreateSolpeAccessUser(string userName, string platform, string accessToken, string iosToken = "")
    {
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
          OracleParameter OraParamUserName = new OracleParameter(":userName", OracleDbType.Varchar2, 100);  //Ajout
          OracleParameter OraParamPlatform = new OracleParameter(":platform", OracleDbType.Varchar2, 10);
          OracleParameter OraParamIosToken = new OracleParameter(":ios_token", OracleDbType.Varchar2, 500);

          OraParamAccessToken.Value = accessToken;
          OraParamUserName.Value = userName;
          OraParamPlatform.Value = platform;
          OraParamIosToken.Value = iosToken;

          oraUpdate.Parameters.Add(OraParamUserName);
          oraUpdate.Parameters.Add(OraParamAccessToken);
          oraUpdate.Parameters.Add(OraParamPlatform);
          oraUpdate.Parameters.Add(OraParamIosToken);

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

    private string GetOracleConnectionParameters()
    {
      var userNameDatabase = _IConfiguration.GetSection("OracleConfig").GetSection("userId").Value;
      var pws = _IConfiguration.GetSection("OracleConfig").GetSection("pws").Value;
      var datasource = _IConfiguration.GetSection("OracleConfig").GetSection("dataSource").Value;

      string conString = $"User Id={userNameDatabase};Password={pws};" +

      //How to connect to an Oracle DB without SQL*Net configuration file
      //  also known as tnsnames.ora.
      $"Data Source={datasource}";
      return conString;
    }

    private string GenerateIosAccessToken(string accessToken)
    {
      var requestBody = new TokenRequest();
      requestBody.application = "org.nativescript.solpercn";
      requestBody.sandbox = false;
      requestBody.apns_tokens = new List<string>();
      requestBody.apns_tokens.Add(accessToken);

      string output = JsonConvert.SerializeObject(requestBody);


      var client = new RestClient("https://iid.googleapis.com/iid/v1:batchImport");
      var request = new RestRequest(Method.POST);
      request.AddHeader("postman-token", "f57e37d9-1492-e9da-943a-d7773f9b3551");
      request.AddHeader("cache-control", "no-cache");
      request.AddHeader("authorization", "key=AAAA56nMBb0:APA91bH7bLu1IS7ryEnGLq2jmREJTtStG3zbIuimiHtV28ujaxaBeOPhVNfrMT0xZXwQ1I0iUl-FtBISAwkeAaGsbmOkRBHKYnczyubq5vl_b5ARaIt2uiNdjy8-a51Dc-xXG6daQ4mS");
      request.AddHeader("content-type", "application/json");
      request.AddParameter("application/json", output, ParameterType.RequestBody);
      IRestResponse response = client.Execute(request);
      var result = JsonConvert.DeserializeObject<TokenResponse>(response.Content,
                    new JsonSerializerSettings
                    {
                      PreserveReferencesHandling = PreserveReferencesHandling.Objects
                    });
      if (result != null && result.Results.Count > 0)
        return result.Results[0].registration_token;
      else
      {
        throw new Exception("Error generando el token de accesso. Comunicarse con el administrador ");
      }
    }

    public async Task<bool> SendNotifications()
    {
      // llama el servicio que retorna las solpes en estado E
      List<ZsdLiberaSolpe> listresult = await GetLiberaSolpes();
      List<string> notificationResult = new List<string>();
      var requestBody = new NotificationModel();
      requestBody.registration_ids = new List<string>();

      foreach (var item in listresult)
      {
        requestBody.registration_ids.Add(item.Access_Token);

      }
      requestBody.notification = new Notification();
      requestBody.notification.badge = 1;
      requestBody.notification.tittle = "Notificación de pedidos.SOLPE";
      requestBody.notification.category = "GENERAL";
      requestBody.notification.showWhenInForeground = true;
      requestBody.image = "https://firebase.google.com/images/social.png";
      requestBody.sound = "default";


      var client = new RestClient("https://fcm.googleapis.com/fcm/send");
      var request = new RestRequest(Method.POST);
      request.AddHeader("postman-token", "f57e37d9-1492-e9da-943a-d7773f9b3551");
      request.AddHeader("cache-control", "no-cache");
      request.AddHeader("authorization", "key=AIzaSyC7UkSrfU-qgMT5O3K14jdj-kz5Gzbzfv4");
      request.AddHeader("content-type", "application/json");

      string output = JsonConvert.SerializeObject(requestBody);

      request.AddParameter("application/json", output, ParameterType.RequestBody);
      IRestResponse response = client.Execute(request);

      if (response.StatusCode != System.Net.HttpStatusCode.OK)
      {
        throw new Exception("Error generando el token de accesso. Comunicarse con el administrador ");
      }

      return true;
    }

  }
}
