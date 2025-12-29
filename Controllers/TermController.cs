using AppNew.Models;
using Microsoft.AspNetCore.Mvc;

namespace AppNew.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TermController : ControllerBase
    {
        [HttpGet("{term}")]
        public IActionResult GetDefinition(string term)
        {
            var definition = TermDictionary.GetDefinition(term);
            if (definition == null)
            {
                return NotFound(new { message = "Термин не найден" });
            }
            return Ok(definition);
        }
    }
}
