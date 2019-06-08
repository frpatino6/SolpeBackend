using Microsoft.AspNetCore.Mvc;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace RCN.Solpe.DataBase.Api.Controllers
{
  [Route("api/[controller]")]
  [ApiController]
  public class ValuesController : ControllerBase
  {


    [HttpGet("/version/GetAssemblyVersionApi/")]
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
