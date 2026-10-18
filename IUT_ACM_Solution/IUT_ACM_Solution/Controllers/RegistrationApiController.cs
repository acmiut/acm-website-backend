using IUT_ACM_Solution.DTOs;
using IUT_ACM_Solution.Interfaces.Services;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace IUT_ACM_Solution.Controllers
{
    /// <summary>
    /// JSON endpoints used by the React sign-up pages.
    /// Error bodies look like  { "message": "...", "errors": { "email": ["..."] }, "retryAfterSeconds": 42 }
    /// (automatic model-validation errors use the standard ValidationProblemDetails shape instead).
    /// </summary>
    [ApiController]
    [Route("api/auth")]
    [EnableCors("Frontend")]
    public class RegistrationApiController : ControllerBase
    {
        private readonly IRegistrationService registrationService;

        public RegistrationApiController(IRegistrationService registrationService)
        {
            this.registrationService = registrationService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDto dto)
        {
            var outcome = await registrationService.StartRegistrationAsync(dto, HttpContext.RequestAborted);
            return ToActionResult(outcome);
        }

        [HttpPost("resend-confirmation")]
        public async Task<IActionResult> ResendConfirmation([FromBody] ResendConfirmationRequestDto dto)
        {
            var outcome = await registrationService.ResendConfirmationAsync(dto.Email, HttpContext.RequestAborted);
            return ToActionResult(outcome);
        }

        [HttpPost("confirm-email")]
        public async Task<IActionResult> ConfirmEmail([FromBody] ConfirmEmailRequestDto dto)
        {
            var outcome = await registrationService.ConfirmEmailAsync(dto.Token, HttpContext.RequestAborted);
            return ToActionResult(outcome);
        }

        private IActionResult ToActionResult(RegistrationOutcome outcome)
        {
            switch (outcome.Status)
            {
                case RegistrationStatus.Success:
                    return Ok(new { message = "OK", resendAfterSeconds = outcome.WaitSeconds ?? 0 });

                case RegistrationStatus.EmailAlreadyRegistered:
                    return Conflict(Error("این ایمیل قبلاً ثبت‌نام شده است.", "email"));

                case RegistrationStatus.WeakPassword:
                    return BadRequest(Error(
                        "رمز عبور باید حداقل ۸ کاراکتر و شامل حرف بزرگ، حرف کوچک، عدد و نماد باشد.", "password"));

                case RegistrationStatus.Cooldown:
                {
                    var seconds = outcome.WaitSeconds ?? 60;
                    Response.Headers["Retry-After"] = seconds.ToString();
                    return StatusCode(StatusCodes.Status429TooManyRequests, new
                    {
                        message = $"ایمیل تأیید به‌تازگی ارسال شده است. لطفاً {seconds} ثانیه دیگر دوباره تلاش کنید.",
                        retryAfterSeconds = seconds
                    });
                }

                case RegistrationStatus.EmailSendFailed:
                    return StatusCode(StatusCodes.Status503ServiceUnavailable,
                        Error("ارسال ایمیل با خطا مواجه شد. چند دقیقه دیگر دوباره تلاش کنید."));

                case RegistrationStatus.InvalidOrExpiredToken:
                    return BadRequest(Error("لینک تأیید نامعتبر است یا منقضی شده است."));

                case RegistrationStatus.NotFound:
                    return NotFound(Error("ثبت‌نام در انتظار تأییدی برای این ایمیل پیدا نشد. دوباره ثبت‌نام کنید."));

                default:
                    return StatusCode(StatusCodes.Status500InternalServerError,
                        Error("خطای غیرمنتظره‌ای رخ داد. لطفاً بعداً دوباره تلاش کنید."));
            }
        }

        private static object Error(string message, string? fieldName = null)
        {
            Dictionary<string, string[]>? errors = null;
            if (fieldName != null)
            {
                errors = new Dictionary<string, string[]>();
                errors[fieldName] = new[] { message };
            }

            return new { message, errors };
        }
    }
}
