using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
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
        private readonly ILogger<LoginController> _logger;
        private readonly HttpContext _context;
        public LoginController(IAutentication autentication, ILogger<LoginController> logger)
        {
            _IAutentication = autentication;
            _logger = logger;
            //this._context = context;
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
                if (userPassword.Equals("empty"))
                {
                    userPassword = Request.Headers["SolpePassword"];
                }

                bool result = _IAutentication.AutenticationUser(userEmail, userPassword, accessToken, platform);

                if (result)
                {
                    _logger.LogInformation($"El usuario {userEmail} logeado satisfactóriamente");
                    return Ok();
                }
                else
                {
                    _logger.LogError($"usuario y contraseña no válido");
                    return NotFound();
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("/login/GetAssemblyVersionApi/")]
        public async Task<IActionResult> GetAssemblyVersionApi()
        {
            try
            {
                string result = Assembly.GetEntryAssembly().GetCustomAttribute<AssemblyFileVersionAttribute>().Version;
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