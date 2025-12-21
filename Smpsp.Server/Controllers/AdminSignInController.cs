using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Smpsp.Server.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Smpsp.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminSignInController(MySettingsService _mss) : ControllerBase
    {
        public const string RoleAdmin = "Admin";

        [HttpPost]
        public ActionResult<AdminSignInReply> SignIn(AdminSignInRequest request)
        {
            if (request.Name.Equals(_mss.Settings.AdminName, StringComparison.InvariantCultureIgnoreCase) && request.Password == _mss.Settings.AdminPassword)
            {
                JwtSecurityTokenHandler tokenHandler = new();
                SymmetricSecurityKey securityKey = new(System.Text.Encoding.UTF8.GetBytes(_mss.Settings.IssuerSigningKey));

                Dictionary<string, object> dic = [];
                dic.Add(ClaimTypes.Name, _mss.Settings.AdminName);
                dic.Add(ClaimTypes.Role, RoleAdmin);

                var dt = DateTime.UtcNow.AddHours(1);

                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Claims = dic,
                    Expires = dt,
                    SigningCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256Signature)
                };

                var st = tokenHandler.CreateToken(tokenDescriptor);
                var token = tokenHandler.WriteToken(st);

                AdminSignInReply reply = new()
                {
                    Token = token,
                    UnixTimestampExpirationDate = ((DateTimeOffset)dt).ToUnixTimeSeconds()
                };

                return Ok(reply);
            }

            return Unauthorized();
        }
    }
}
