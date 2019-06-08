using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace RCN.Solpe.Api.Controllers
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
