using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
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
        private readonly ILogger<SolpeController> _logger;
        public SolpeController(ISolpeServices solpeServices, ILogger<SolpeController> logger)
        {
            _ISolpeServices = solpeServices;
            _logger = logger;
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
                //var result = _IADIntegrationRepository.DeleteUser("", "");xz
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
        public async Task<IActionResult> GetAccessUser(string userName, string platform, string accessToken)
        {
            try
            {
                _logger.LogInformation($"Iniciando GetAccessUser");
                var result = await _ISolpeServices.GetAccessUser(userName, platform, accessToken);
                _logger.LogInformation($"GetAccessUser {userName} correctamente {result.AccessToken}");
                //var result = _IADIntegrationRepository.DeleteUser("", "");
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error en GetAccessUser {userName} y {platform}");
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("/solpe/UpdateSolpeState/{number}/{posicion}")]
        public async Task<IActionResult> UpdateSolpeState(string number, int posicion)
        {
            try
            {
                var result = _ISolpeServices.UpdateSolpe(number, posicion);
                //var result = _IADIntegrationRepository.DeleteUser("", "");
                return Ok(result);
            }
            catch (Exception ex)
            {

                return BadRequest(ex.Message);
            }
        }

        [HttpPost("/solpe/UpdatePedidoState/{number}")]
        public async Task<IActionResult> UpdatePedidoState(string number)
        {
            try
            {
                var result = _ISolpeServices.UpdatePedido(number);
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