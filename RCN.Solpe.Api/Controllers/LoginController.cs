using Microsoft.AspNetCore.Mvc;
using RCN.Solpe.Core.Interfaces;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace RCN.Solpe.Api.Controllers
{
  [Produces("application/json")]
  public class LoginController : Controller
  {

    private readonly IAutentication _IAutentication;

    public LoginController(IAutentication autentication)
    {
      _IAutentication = autentication;
    }
    public IActionResult Index()
    {
      return View();
    }

    [HttpGet("/login/ValidateUser/{userEmail}/{userPassword}/{accessToken}/{platform}")]
    public IActionResult ValidateUserApiAsync(string userEmail, string userPassword, string accessToken, string platform)
    {
      try
      {
        var result = _IAutentication.AutenticationUser(userEmail, userPassword, accessToken, platform);
        //var result = _IADIntegrationRepository.DeleteUser("", "");

        if (result)
          return Ok();
        else
          return NotFound();
      }
      catch (Exception ex)
      {

        return BadRequest(ex.Message);
      }
    }

    [HttpGet("/login/GetAssemblyVersionApi/")]
    public async Task<IActionResult> GetAssemblyVersionApi()
    {
      try
      {
        var result = Assembly.GetEntryAssembly().GetCustomAttribute<AssemblyFileVersionAttribute>().Version;
        //var result = _IADIntegrationRepository.DeleteUser("", "");
        return Ok(result.ToString());
      }
      catch (Exception ex)
      {

        return BadRequest(ex.Message);
      }
    }

  }
}