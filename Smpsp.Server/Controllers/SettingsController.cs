using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Smpsp.Server.Data;

namespace Smpsp.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SettingsController(MySettingsService _mss, IMailService _ms) : ControllerBase
    {
        [Authorize(Roles = AdminSignInController.RoleAdmin)]
        [HttpGet]
        public ActionResult<MySettings> Get()
        {
            return Ok(_mss.Settings);
        }

        [Authorize(Roles = AdminSignInController.RoleAdmin)]
        [HttpPut]
        public ActionResult Put(MySettings settings)
        {
            try
            {
                _mss.Settings.MergeFrom(settings);
                _mss.SaveSettings();
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize(Roles = AdminSignInController.RoleAdmin)]
        [HttpGet("testmail")]
        public async Task<ActionResult> TestMail()
        {
            try
            {
                if (await _ms.TrySendTestMessage(_mss.Settings.SmtpEmail))
                {
                    return Ok();
                }

                return BadRequest();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
