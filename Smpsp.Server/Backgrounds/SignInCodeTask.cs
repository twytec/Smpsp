using Microsoft.IdentityModel.Tokens;
using Smpsp.Server.Data;
using System.Collections.Concurrent;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Smpsp.Server.Backgrounds
{
    public class SignInCodeTask(MySettingsService _mss)
    {
        private readonly JwtSecurityTokenHandler _tokenHandler = new();
        private readonly SymmetricSecurityKey _tokenKey = new(Encoding.UTF8.GetBytes(_mss.Settings.IssuerSigningKey));

        private record UserData(User User, string Code, DateTime Expiration);
        private readonly ConcurrentDictionary<string, UserData> _codes = [];
        private readonly Random _random = new();

        public void Cleanup(CancellationToken ct)
        {
            if (_codes.IsEmpty)
                return;

            var items = _codes.Where(x => x.Value.Expiration < DateTime.UtcNow);
            if (items.Any())
            {
                foreach (var item in items)
                {
                    if (ct.IsCancellationRequested)
                        break;

                    _codes.TryRemove(item);
                }
            }
        }

        public string GetCode() => _random.Next(1000, 9999).ToString();

        public string AddUserAndGetAntiforgeryToken(User user, string code)
        {
            string guid = Guid.NewGuid().ToString();
            _codes[guid] = new(user, code, DateTime.UtcNow.AddHours(_mss.Settings.SignInCodeExpirationHours));
            return guid;
        }

        public bool IsAntiforgeryTokenValid(string token) => _codes.ContainsKey(token);
        public SignInCodeReply? GetSignInCodeReply(SignInCodeRequest req)
        {
            if (_codes.TryRemove(req.AntiforgeryToken, out var d) && d.Code == req.Code)
            {
                Dictionary<string, object> dic = [];
                dic.Add(ClaimTypes.Name, d.User.EMail);

                DateTime dt = DateTime.UtcNow.AddDays(_mss.Settings.SignInExpirationDays);
                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Claims = dic,
                    Expires = dt,
                    SigningCredentials = new SigningCredentials(_tokenKey, SecurityAlgorithms.HmacSha256Signature)
                };

                var st = _tokenHandler.CreateToken(tokenDescriptor);
                var token = _tokenHandler.WriteToken(st);

                return new()
                {
                    Token = token,
                    User = d.User,
                    UnixTimestampExpirationDate = ((DateTimeOffset)dt).ToUnixTimeSeconds()
                };
            }

            return null;
        }
    }
}
