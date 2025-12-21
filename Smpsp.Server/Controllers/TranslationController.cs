using Microsoft.AspNetCore.Mvc;
using Smpsp.Server.Data;

namespace Smpsp.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TranslationController(TranslationService _ts) : ControllerBase
    {
        [HttpGet]
        public ActionResult<IEnumerable<string>> Get()
        {
            var data = _ts.GetSupportedLanguages();
            return Ok(data);
        }

        [HttpGet("{code}")]
        public ActionResult<Translation> Get(string code)
        {
            var data = _ts.GetTranslations(code);
            return Ok(data);
        }
    }
}
