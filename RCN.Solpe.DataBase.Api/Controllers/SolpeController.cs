using Microsoft.AspNetCore.Mvc;
using RCN.Solpe.DataBase.Core.Interfaces;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace RCN.Solpe.Api.Controllers
{
  [Produces("application/json")]
  public class SolpeController : Controller
  {

    private readonly ISolpeServices _ISolpeServices;

    public SolpeController(ISolpeServices solpeServices)
    {
      _ISolpeServices = solpeServices;
    }
    public IActionResult Index()
    {
      return View();
    }

    /// <summary>
    /// Get list of all users
    /// </summary>
    /// <returns></returns>
    [HttpGet("/solpe/GetLiberaSolpes/{userName=''}")]
    public async Task<IActionResult> GetLiberaSolpes(string userName)
    {
      try
      {
        var result = await _ISolpeServices.GetLiberaSolpes(userName);
        //var result = _IADIntegrationRepository.DeleteUser("", "");
        return Ok(result);
      }
      catch (Exception ex)
      {

        return BadRequest(ex.Message);
      }
    }

    [HttpGet("/solpe/SendNotifications")]
    public async Task<IActionResult> SendNotifications()
    {
      try
      {
        var result = await _ISolpeServices.SendNotifications();
        //var result = _IADIntegrationRepository.DeleteUser("", "");
        return Ok(result);
      }
      catch (Exception ex)
      {

        return BadRequest(ex.Message);
      }
    }

    [HttpGet("/solpe/GetAccessUser/{userName}/{accessToken}/{platform}")]
    public async Task<IActionResult> GetAccessUser(string userName,string platform ,string accessToken)
    {
      try
      {
        var result = await _ISolpeServices.GetAccessUser(userName, platform, accessToken);
        //var result = _IADIntegrationRepository.DeleteUser("", "");
        return Ok(result);
      }
      catch (Exception ex)
      {

        return BadRequest(ex.Message);
      }
    }

    [HttpPost("/solpe/UpdateOrderState/{number}")]
    public async Task<IActionResult> UpdateOrderState(string number)
    {
      try
      {
        var result = _ISolpeServices.UpdateSolpe(number);
        //var result = _IADIntegrationRepository.DeleteUser("", "");
        return Ok(result);
      }
      catch (Exception ex)
      {

        return BadRequest(ex.Message);
      }
    }



  }
}