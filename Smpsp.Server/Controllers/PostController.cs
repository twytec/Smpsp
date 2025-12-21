using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Smpsp.Server.Backgrounds;
using Smpsp.Server.Data;

namespace Smpsp.Server.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class PostController(
        MySettingsService _mss, PostService _ps, UploadMediaTask _umt, UserAuthStateService _uass, UserService _us) : ControllerBase
    {
        [HttpGet]
        public ActionResult<IEnumerable<Post>> GetAll()
        {
            if (_uass.User is not null)
            {
                return Ok(_ps.GetPosts());
            }
            return BadRequest(_uass.I18n.InvalidOrInactiveUser);
        }

        [HttpGet("{id}")]
        public ActionResult<Post> GetById(string id)
        {
            if (_uass.User is not null)
            {
                if (_ps.GetPostById(id) is Post p)
                {
                    return Ok(p);
                }
                return NotFound(_uass.I18n.NotFound);
            }

            return BadRequest(_uass.I18n.InvalidOrInactiveUser);
        }

        [HttpGet("settings")]
        public ActionResult<PostSettingsReply> GetSettings()
        {
            return Ok(new PostSettingsReply()
            {
                Hashtags = _mss.Settings.Hashtags,
                MaxAllowedImageSize = _mss.Settings.MaxAllowedImageSize,
                MaxAllowedVideoSize = _mss.Settings.MaxAllowedVideoSize,
                SupportedImageExtension = _mss.Settings.SupportedImageExtension,
                SupportedVideoExtension = _mss.Settings.SupportedVideoExtension,
                MaxRequestBodySize = _mss.Settings.MaxRequestBodySize,
                DefaultVotingPeriodInHours = _mss.Settings.DefaultVotingPeriodInHours,
            });
        }

        [HttpPost]
        public async Task<ActionResult<Post>> Add(Post post)
        {
            if (_uass.User is not null)
            {
                if (post.EndOfVoting <= DateTimeOffset.UtcNow.ToUnixTimeSeconds())
                {
                    post.EndOfVoting = DateTimeOffset.UtcNow.AddHours(_mss.Settings.DefaultVotingPeriodInHours).ToUnixTimeSeconds();
                }

                post.Id = Guid.NewGuid().ToString();
                post.UserId = _uass.User.Id;
                await _ps.AddPostAsync(post);
                return Ok(post);
            }

            return BadRequest(_uass.I18n.InvalidOrInactiveUser);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(string id)
        {
            if (_uass.User is User u)
            {
                if (_ps.GetPostById(id) is Post p)
                {
                    if (p.UserId == u.Id || u.VetoLevel > 0)
                    {
                        await _ps.DeletePostAsync(id);
                        return Ok();
                    }

                    return Forbid();
                }

                return NotFound(_uass.I18n.NotFound);
            }

            return BadRequest(_uass.I18n.InvalidOrInactiveUser);
        }

        #region Media

        [HttpPost("media")]
        public async Task<ActionResult<string>> Upload([FromBody] DataMessage req)
        {
            if (_uass.User is not null)
            {
                try
                {
                    await _umt.WriteStreamAsync(req, _uass);
                    return Ok(req.Id);
                }
                catch (Exception ex)
                {
                    return BadRequest(ex.Message);
                }
            }

            return BadRequest(_uass.I18n.InvalidOrInactiveUser);
        }

        #endregion

        #region Voting

        [HttpPatch("voting/{id}")]
        public async Task<IActionResult> Voting(string id, [FromBody] PostVoting voting)
        {
            if (_uass.User is User u)
            {
                if (_ps.GetPostById(id) is Post p)
                {
                    if (p.Status != PostStatus.Voting)
                    {
                        return BadRequest(_uass.I18n.VotingComplete);
                    }

                    voting.UserId = u.Id;

                    var ava = p.Votings.FirstOrDefault(x => x.UserId == voting.UserId);
                    if (ava is not null)
                    {
                        ava.Like = voting.Like;
                        ava.Text = voting.Text;
                    }
                    else
                    {
                        p.Votings.Add(voting);
                    }

                    await _ps.UpdatePostAsync(p);
                    return Ok();
                }

                return NotFound(_uass.I18n.NotFound);
            }

            return BadRequest(_uass.I18n.InvalidOrInactiveUser);
        }

        #endregion

        #region Veto

        [HttpPatch("veto/{id}")]
        public async Task<IActionResult> Veto(string id, [FromBody] PostVeto veto)
        {
            if (_uass.User is User u)
            {
                if (u.VetoLevel == 0)
                {
                    return Forbid();
                }

                if (_ps.GetPostById(id) is Post p)
                {
                    var ava = p.Vetoes.FirstOrDefault(x => x.UserId == u.Id);
                    if (ava is not null)
                    {
                        ava.Text = veto.Text;
                    }
                    else
                    {
                        veto.UserId = u.Id;
                        veto.VetoLevel = u.VetoLevel;

                        p.Vetoes.Add(veto);
                    }

                    await _ps.UpdatePostAsync(p);

                    return Ok();
                }

                return NotFound(_uass.I18n.NotFound);
            }

            return BadRequest(_uass.I18n.InvalidOrInactiveUser);
        }

        [HttpDelete("veto/{id}")]
        public async Task<IActionResult> DeleteVeto(string id)
        {
            if (_uass.User is User u)
            {
                if (id.Contains('+'))
                {
                    var s = id.Split('+');
                    string postId = s[0];
                    string userId = s[1];

                    var post = _ps.GetPostById(postId);
                    if (post is null)
                        return NotFound(_uass.I18n.PostNotFound);

                    var user = _us.GetUserById(userId);
                    if (user is null)
                        return NotFound(_uass.I18n.UserNotFound);

                    var veto = post.Vetoes.FirstOrDefault(x => x.UserId == userId);
                    if (veto is null)
                        return NotFound();

                    if (u.Id != user.Id && u.VetoLevel <= user.VetoLevel)
                    {
                        return Forbid(_uass.I18n.OverrideVeto);
                    }

                    post.Vetoes.Remove(veto);
                    await _ps.UpdatePostAsync(post);
                    return Ok();
                }

                return BadRequest(_uass.I18n.InvalidVetoId);
            }

            return BadRequest(_uass.I18n.InvalidOrInactiveUser);
        }

        #endregion
    }
}
