using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Smpsp.Server.Data;

namespace Smpsp.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController(UserService _us, UserAuthStateService _uass) : ControllerBase
    {
        [Authorize]
        [HttpGet]
        public ActionResult<IEnumerable<User>> Get()
        {
            if (_uass.User is not null)
            {
                return Ok(_us.GetAllUsers());
            }
            return BadRequest(_uass.I18n.InvalidOrInactiveUser);
        }

        [Authorize]
        [HttpGet("{id}")]
        public ActionResult<User> GetById(string id)
        {
            if (_uass.User is not null)
            {
                if (_us.GetUserById(id) is User u)
                {
                    return Ok(u);
                }
                return NotFound();
            }
            return BadRequest(_uass.I18n.InvalidOrInactiveUser);
        }

        [Authorize(Roles = AdminSignInController.RoleAdmin)]
        [HttpPost]
        public async Task<ActionResult> Post(User user)
        {
            try
            {
                await _us.AddUserAsync(user);
                return Ok(user);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize]
        [HttpPut]
        public async Task<ActionResult> Put(User user)
        {
            if (HttpContext.User.IsInRole(AdminSignInController.RoleAdmin) == false)
            {
                if (_uass.User is User u && user.Id == u.Id)
                {
                    user.Id = u.Id;
                    user.Active = u.Active;
                    user.EMail = u.EMail;
                    user.VetoLevel = u.VetoLevel;
                }
                else
                {
                    return Forbid();
                }
            }

            try
            {
                await _us.UpdateUserAsync(user);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize(Roles = AdminSignInController.RoleAdmin)]
        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(string id)
        {
            try
            {
                await _us.DeleteUserAsync(id);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
