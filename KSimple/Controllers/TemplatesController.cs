using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using KSimple.Models;
using KSimple.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KSimple.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class TemplatesController : ControllerBase
    {
        private readonly ApplicationContext _context;

        public TemplatesController(ApplicationContext context)
        {
            _context = context;
        }
        
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Template>>> GetTemplates()
        {
            return await _context.Templates.ToListAsync();
        }

        // TODO make normal
        [HttpGet("{id}")]
        public Dictionary<string, dynamic> GetTemplateById(string id, Dictionary<string, JsonElement> data)
        {
            var res = new Dictionary<string, dynamic>();
            foreach (KeyValuePair<string, JsonElement> pair in data)
            {
                try
                {
                    res[pair.Key] = pair.Value.GetArrayLength();
                }
                catch
                {
                    res[pair.Key] = "Not array";
                }
            }

            return res;

        }

        [HttpPost]
        public ActionResult<Template> PostTemplate(Template template)
        {
            return Ok(template);
        }
    }
}