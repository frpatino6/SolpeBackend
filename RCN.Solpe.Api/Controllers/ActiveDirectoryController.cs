using Microsoft.AspNetCore.Mvc;
using RCN.Solpe.Core.Interfaces;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace RCN.Solpe.Api.Controllers
{
  [Produces("application/json")]
  public class ActiveDirectoryController : Controller
  {

    private readonly IAutentication _IAutentication;

    public ActiveDirectoryController(IAutentication  autentication)
    {
      _IAutentication = autentication;
    }
    public IActionResult Index()
    {
      return View();
    }

    /// <summary>
    /// Get list of all users
    /// </summary>
    /// <returns></returns>
    [HttpGet("/ActiveDirectory/GetAllUsers/")]
    public IActionResult GetAllUsers()
    {
      try
      {
        var result = _IAutentication.GetAllUsers();
        //var result = _IADIntegrationRepository.DeleteUser("", "");
        return Ok(result);
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