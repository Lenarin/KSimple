using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using KSimple.Models;
using KSimple.Models.Entities;
using KSimple.Models.Misc;
using KSimple.Models.Repositories;
using KSimple.Models.Responses;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace KSimple.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class TemplatesController : ControllerBase
    {
        private readonly ApplicationContext _context;
        private readonly string _connectionString;

        public TemplatesController(ApplicationContext context, IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
            _context = context;
        }
        
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Template>>> GetTemplates()
        {
            await using var connection = new SqliteConnection(_connectionString);
            connection.Open();
            return Ok(await new TemplateRepository(connection).GetAllTemplates());
        }
        
        [HttpGet("{id}")]
        public async Task<ActionResult<Template>> GetTemplateById(Guid id)
        {
            await using var connection = new SqliteConnection(_connectionString);
            connection.Open();
            var res = await new TemplateRepository(connection).GetTemplateById(id);
            if (res == null) return NotFound(new ErrorResponse("TemplateNotFound"));
            return Ok(res);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<Template>> PutTemplate(Template template, Guid id)
        {
            if (template.ModelTree != null)
            {
                try
                {
                    template.Validate();
                }
                catch (Exception e)
                {
                    return BadRequest(e);
                }
            }
            
            await using var connection = new SqliteConnection(_connectionString);
            connection.Open();
            var rep = new TemplateRepository(connection);
            var res = await rep.GetTemplateById(id);
            res.Merge(template);

            await rep.UpdateTemplate(res);

            return Ok(res);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<Template>> DeleteTemplate(Guid id)
        {
            await using var connection = new SqliteConnection(_connectionString);
            connection.Open();
            var rep = new TemplateRepository(connection);
            var template = await rep.GetTemplateById(id);
            
            await rep.DeleteTemplateById(id);

            return Ok(template);
        }

        [HttpPost]
        public async Task<ActionResult<Template>> PostNewTemplate(Template template)
        {
            try
            {
                template.Validate();
            }
            catch (Exception e)
            {
                return BadRequest(e);
            }
            
            await using var connection = new SqliteConnection(_connectionString);
            connection.Open();
            template.Id = Guid.NewGuid();
            try
            {
                await new TemplateRepository(connection).AddNewTemplate(template);
            }
            catch (SqliteException err)
            {
                return BadRequest(err.Message);
            }
            return Ok(template);
        }

        [HttpGet("{id}/tree")]
        public async Task<ActionResult<ModelTreeNode>> GetModelTreeOfTemplate(Guid id)
        {
            await using var connection = new SqliteConnection(_connectionString);
            connection.Open();
            
            var tree = await new TemplateRepository(connection).GetModelTree(id);
            if (tree == null) return NotFound();
            return tree;
        }

        [HttpPost("{id}/tree")]
        public async Task<ActionResult> PostModelTreeOfTemplate(Guid id, ModelTreeNode tree)
        {
            if (tree == null) return BadRequest();
            if (tree.Id != "root") return BadRequest("Root node id bust be 'root'");
            try
            {
                tree.Validate();
            }
            catch (Exception e)
            {
                return BadRequest(e);
            }
            
            await using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            await new TemplateRepository(connection).SetModelTree(id, tree);
            return Ok();
        }
        
    }
}