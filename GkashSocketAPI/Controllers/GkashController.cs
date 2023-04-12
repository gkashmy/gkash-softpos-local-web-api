using ClientSocketConnection;
using GkashSocketAPI.Core;
using GkashSocketAPI.Dto.LoginDto;
using GkashSocketAPI.Repository;
using Microsoft.AspNetCore.Mvc;
using Serilog.Extensions.Logging;
using Serilog;
using Newtonsoft.Json;

namespace GkashSocketAPI.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class GkashController : ControllerBase
    {
        private readonly IGkashRepository _gkashRepository;
        private readonly IConfiguration _configuration;

        public GkashController(IGkashRepository gkashRepository, IConfiguration configuration)
        {
            _gkashRepository = gkashRepository;
            _configuration = configuration;
        }

        [HttpPost(Name = "Login")]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            Microsoft.Extensions.Logging.ILogger logger = null;
            try
            {
                if (string.IsNullOrWhiteSpace(dto.Username) || string.IsNullOrWhiteSpace(dto.Password))
                {                    
                    return BadRequest("Invalid request, please check your request parameters");
                }

                bool enabledLogging = _configuration.GetValue<bool>("EnabledLogging");
                string loggingPath = _configuration.GetValue<string>("LoggingPath");

                if (enabledLogging)
                {
                    if (string.IsNullOrWhiteSpace(loggingPath))
                    {
                        loggingPath = "C:\\GkashSocketAPILog\\log.txt";
                    }

                    //use serilog
                    var serilogLogger = new LoggerConfiguration()
                     .MinimumLevel.Debug()
                     .WriteTo.File(loggingPath, shared: true, rollingInterval: RollingInterval.Month)
                     .CreateLogger();
                    logger = new SerilogLoggerFactory(serilogLogger).CreateLogger<GkashRepository>();
                }

                await _gkashRepository.LoginAsync(dto, logger);

                ClientSocket client = await _gkashRepository.GetGkashSDKInstanceAsync();

                if(string.IsNullOrWhiteSpace(client.GetIpAddress()))
                {
                    if (enabledLogging)
                    {
                        logger?.LogError("Login failed, please check log");
                        return BadRequest("Login failed, please check log");
                    }
                    else
                    {
                        logger?.LogError("Login failed, please enable logging and check log");
                        return BadRequest("Login failed, please enable logging and check log");
                    }                 
                }                

                return Ok();
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.ToString());
                logger?.LogError("Login Exception: " + ex);

                return StatusCode(StatusCodes.Status500InternalServerError, "An error occured."); // Status Code: 500
            }       
        }

        [HttpPost(Name = "RequestPayment")]
        public async Task<IActionResult> RequestPayment(Dto.PaymentDto.PaymentRequestDto dto)
        {
            Microsoft.Extensions.Logging.ILogger logger = await _gkashRepository.GetLoggerAsync();
            try
            {
                logger?.LogInformation("RequestPayment: " + JsonConvert.SerializeObject(dto));
                ClientSocket client = await _gkashRepository.GetGkashSDKInstanceAsync();
                await _gkashRepository.SetCallbackURL(dto.CallbackURL);
                if (client == null)
                {
                    logger?.LogError("RequestPayment BadRequest: Requesting payment before login");
                    return BadRequest("Please login first before request payment.");
                }

                if (((int)dto.PaymentType) > 13 || ((int)dto.PaymentType) < 0)
                {
                    return BadRequest("Invalid payment type.");
                }

                if (string.IsNullOrWhiteSpace(dto.Amount))
                {
                    logger?.LogError("RequestPayment BadRequest: Amount is empty");
                    return BadRequest("Invalid Amount.");
                }

                try
                {
                    double amt = double.Parse(dto.Amount);
                }
                catch (Exception ex)
                {
                    logger?.LogError("RequestPayment BadRequest: " + ex);
                    return BadRequest("Invalid Amount.");
                }
                
                if (string.IsNullOrWhiteSpace(dto.ReferenceNo))
                {
                    dto.ReferenceNo = "WEBTCP-" + DateTime.Now.ToString("yyyyMMddHHmmss");
                }

                ClientSocketConnection.model.PaymentRequestDto requestDto = new();
                requestDto.PaymentType = dto.PaymentType; 
                requestDto.Amount = dto.Amount;
                requestDto.Email = dto.Email;
                requestDto.ReferenceNo = dto.ReferenceNo;
                requestDto.MobileNo = dto.MobileNo;
                requestDto.PreAuth = dto.PreAuth;                          

                client.RequestPayment(requestDto);

                return Ok(new { dto.ReferenceNo });
            }
            catch(Exception ex)
            {
                logger?.LogError("RequestPayment Exception: " + ex);

                return StatusCode(StatusCodes.Status500InternalServerError, "An error occured."); // Status Code: 500
            }          
        }

        [HttpGet(Name = "QueryStatus")]
        public async Task<IActionResult> QueryStatus(string ReferenceNo)
        {
            Microsoft.Extensions.Logging.ILogger logger = await _gkashRepository.GetLoggerAsync();
            try
            {
                if (string.IsNullOrWhiteSpace(ReferenceNo))
                {
                    return BadRequest("Invalid ReferenceNo");
                }

                ClientSocket client = await _gkashRepository.GetGkashSDKInstanceAsync();
                logger?.LogInformation("QueryStatus: " + ReferenceNo);
                client.QueryStatus(ReferenceNo);

                return Ok();
            }
            catch(Exception ex)
            {
                logger?.LogError("QueryStatus Exception: " + ex);

                return StatusCode(StatusCodes.Status500InternalServerError, "An error occured."); // Status Code: 500
            }
        }
    }
}
