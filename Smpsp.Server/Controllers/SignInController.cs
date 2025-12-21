using Microsoft.AspNetCore.Mvc;
using Smpsp.Server.Backgrounds;
using Smpsp.Server.Data;

namespace Smpsp.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SignInController(IMailService _ms, UserService _us, SignInCodeTask _sict) : ControllerBase
    {
        [HttpPost]
        public async Task<ActionResult<SignInReply>> SignIn(SignInRequest signIn)
        {
            if (_us.GetUserByEMail(signIn.EMail) is User u && u.Active)
            {
                string code = _sict.GetCode();
                string guid = _sict.AddUserAndGetAntiforgeryToken(u, code);

                if (await _ms.TrySendSignInCodeMessage(u, code))
                {
                    return Ok(new SignInReply() { AntiforgeryToken = guid });
                }
            }

            return NotFound();
        }

        [HttpPost("code")]
        public ActionResult<SignInCodeReply> SignInCode(SignInCodeRequest req)
        {
            if (_sict.IsAntiforgeryTokenValid(req.AntiforgeryToken))
            {
                if (_sict.GetSignInCodeReply(req) is SignInCodeReply rep)
                {
                    return Ok(rep);
                }

                return BadRequest();
            }

            return NotFound();
        }
    }
}
