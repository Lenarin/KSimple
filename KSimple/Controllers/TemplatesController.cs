using System;
using System.Threading.Tasks;
using KSimple.Models;
using Microsoft.AspNetCore.Mvc;

namespace KSimple.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class TemplatesController : ControllerBase
    {
        [HttpGet]
        public string GetTemplates()
        {
            return "Hello World!";
        }

        [HttpGet("{id}")]
        public string GetTemplateById(string id)
        {
            return id;
        }

        [HttpPost]
        public ActionResult<Template> PostTemplate(Template template)
        {
            return Ok(template);
        }
    }
}