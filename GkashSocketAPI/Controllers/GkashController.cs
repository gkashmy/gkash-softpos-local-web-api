using ClientSocketConnection;
using GkashSocketAPI.Core;
using GkashSocketAPI.Dto.LoginDto;
using Microsoft.AspNetCore.Mvc;
using ClientSocketConnection.model;
using System.Text.Json;

namespace GkashSocketAPI.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class GkashController : ControllerBase
    {
        private readonly IGkashService _gkashService;
        private readonly ILogger _logger;
        private readonly string HANDLER = nameof(GkashController);

        public GkashController(IGkashService gkashService, ILogger<GkashController> logger)
        {
            _gkashService = gkashService;
            _logger = logger;
        }

        [HttpPost(Name = "Login")]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            string tag = $"{HANDLER}.{nameof(Login)}";
            try
            {
                _logger?.LogInformation($"{tag} request: {dto.Username}");

                if (string.IsNullOrWhiteSpace(dto.Username) || string.IsNullOrWhiteSpace(dto.Password))
                {
                    _logger?.LogError($"{tag} Username or Password is empty");
                    return BadRequest("Invalid request, please check your request parameters");
                }

                string status = await _gkashService.LoginAsync(dto);
                _logger?.LogInformation($"{tag} response: {status}");
                return Ok(new {Message = status});
            }
            catch(Exception ex)
            {
                _logger?.LogError($"{tag} Exception: {ex}");

                return StatusCode(StatusCodes.Status500InternalServerError, "An error occured."); // Status Code: 500
            }       
        }

        [HttpPost(Name = "RequestPayment")]
        public IActionResult RequestPayment(PaymentRequestDto dto)
        {
            string tag = $"{HANDLER}.{nameof(RequestPayment)}";
            try
            {                            
                _logger?.LogInformation($"{tag}: {JsonSerializer.Serialize(dto)}");

                if (((int)dto.PaymentType) > 13 || ((int)dto.PaymentType) < 0)
                {
                    return BadRequest("Invalid payment type.");
                }

                if (string.IsNullOrWhiteSpace(dto.Amount))
                {
                    _logger?.LogError($"{tag} RequestPayment BadRequest: Amount is empty");
                    return BadRequest("Invalid Amount.");
                }

                try
                {
                    double amt = double.Parse(dto.Amount);
                }
                catch (Exception ex)
                {
                    _logger?.LogError($"{tag} BadRequest: {ex}");
                    return BadRequest("Invalid Amount.");
                }
                
                if (string.IsNullOrWhiteSpace(dto.ReferenceNo))
                {
                    dto.ReferenceNo = "WEBTCP-" + DateTime.Now.ToString("yyyyMMddHHmmss");
                }

                _gkashService.RequestPayment(dto);

                return Ok(new { dto.ReferenceNo });
            }
            catch(Exception ex)
            {
                _logger?.LogError($"{tag} Exception: {ex}");

                return StatusCode(StatusCodes.Status500InternalServerError, "An error occured."); // Status Code: 500
            }          
        }

        [HttpGet(Name = "QueryStatus")]
        public async Task<IActionResult> QueryStatus(string ReferenceNo)
        {
            string tag = $"{HANDLER}.{nameof(QueryStatus)}";
            try
            {
                if (string.IsNullOrWhiteSpace(ReferenceNo))
                {
                    _logger?.LogInformation($"{tag} ReferenceNo is empty");
                    return BadRequest("Invalid ReferenceNo");
                }

                _logger?.LogInformation($"{tag} : " + ReferenceNo);
                TransResult.TransactionStatus status = await _gkashService.QueryTransactionStatusAsync(ReferenceNo);

                if (status == null)
                {
                    return BadRequest();
                }

                return Ok(status);
            }
            catch (Exception ex)
            {
                _logger?.LogError($"{tag} Exception: {ex}");

                return StatusCode(StatusCodes.Status500InternalServerError, "An error occured."); // Status Code: 500
            }
        }

        [HttpGet(Name = "QueryCardAndDuitNowStatus")]
        public async Task<IActionResult> QueryCardAndDuitNowStatus(string ReferenceNo)
        {
            string tag = $"{HANDLER}.{nameof(QueryCardAndDuitNowStatus)}";
            try
            {
                if (string.IsNullOrWhiteSpace(ReferenceNo))
                {
                    _logger?.LogInformation($"{tag} ReferenceNo is empty");
                    return BadRequest("Invalid ReferenceNo");
                }
                _logger?.LogInformation($"{tag} : " + ReferenceNo);
                List<TransResult.TransactionStatus> status = await _gkashService.QueryCardAndDuitNowStatusAsync(ReferenceNo);              

                if (status == null)
                {
                    return BadRequest();
                }

                return Ok(status);
            }
            catch (Exception ex)
            {
                _logger?.LogError($"{tag} Exception: {ex}");

                return StatusCode(StatusCodes.Status500InternalServerError, "An error occured."); // Status Code: 500
            }
        }

        [HttpGet(Name = "CancelTransaction")]
        public IActionResult CancelTransaction(string TerminalId)
        {
            string tag = $"{HANDLER}.{nameof(CancelTransaction)}";
            try
            {
                if (string.IsNullOrWhiteSpace(TerminalId))
                {
                    return BadRequest($"{tag} Invalid TerminalId");
                }

                _gkashService.CancelTransaction(TerminalId);
                _logger?.LogInformation($"{tag} CancelTransaction: {TerminalId}");

                return Ok();
            }
            catch (Exception ex)
            {
                _logger?.LogError($"{tag} Exception: {ex}");

                return StatusCode(StatusCodes.Status500InternalServerError, "An error occured."); // Status Code: 500
            }
        }
    }
}
