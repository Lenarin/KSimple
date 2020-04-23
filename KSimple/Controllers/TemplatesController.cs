using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using KSimple.Models;
using KSimple.Models.Entities;
using KSimple.Models.Misc;
using KSimple.Models.Repositories;
using KSimple.Models.Responses;
using Microsoft.AspNetCore.Authorization;
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
        
        [HttpGet("all")]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult<IEnumerable<TemplateResponse>>> GetTemplates()
        {
            await using var connection = new SqliteConnection(_connectionString);
            connection.Open();
            return Ok((await new TemplateRepository(connection).GetAllTemplates())
                .ToList().ConvertAll(template => new TemplateResponse(template, true)));
        }

        [HttpGet]
        [Authorize]
        public async Task<ActionResult<IEnumerable<TemplateResponse>>> GetUserTemplates()
        {
            await using var connection = new SqliteConnection(_connectionString);
            connection.Open();
            
            var userId = Guid.Parse(User.Claims.First(c => c.Type == "userid").Value);

            return Ok((await new TemplateRepository(connection).GetAllUserTemplates(userId))
                .ToList().ConvertAll(pair => new TemplateResponse(pair.Item1, pair.Item2)));
        }
        
        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<TemplateResponse>> GetTemplateById(Guid id)
        {
            await using var connection = new SqliteConnection(_connectionString);
            connection.Open();
            
            Template template = null;
            var canModify = false;

            if (User.IsInRole("admin"))
            {
                template = await new TemplateRepository(connection).GetTemplateById(id);
                canModify = true;
            }
            else
            {
                var userId = Guid.Parse(User.Claims.First(c => c.Type == "userid").Value);
                (template, canModify) = await new TemplateRepository(connection).GetUserTemplateById(userId, id);
            }   
            
            if (template == null) return NotFound(new ErrorResponse("Template Not Found"));
            return Ok(new TemplateResponse(template, canModify));
        }

        [HttpPut("{id}")]
        [Authorize]
        public async Task<ActionResult<TemplateResponse>> PutTemplate(Template newTemplate, Guid id)
        {
            if (newTemplate.ModelTree != null)
            {
                try
                {
                    newTemplate.Validate();
                }
                catch (Exception e)
                {
                    return BadRequest(e.Message);
                }
            }
            
            await using var connection = new SqliteConnection(_connectionString);
            connection.Open();
            
            var repo = new TemplateRepository(connection);

            Template template = null;
            var canModify = false;

            if (User.IsInRole("admin"))
            {
                template = await new TemplateRepository(connection).GetTemplateById(id);
                canModify = true;
            }
            else
            {
                var userId = Guid.Parse(User.Claims.First(c => c.Type == "userid").Value);
                (template, canModify) = await new TemplateRepository(connection).GetUserTemplateById(userId, id);
            }

            if (template == null)
                return NotFound(new ErrorResponse("Template not found"));

            if (!canModify)
                return BadRequest(new ErrorResponse("You can't modify this template"));
            
            template.Merge(newTemplate);

            await repo.UpdateTemplate(template);

            return Ok(new TemplateResponse(template, canModify));
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<ActionResult<Template>> DeleteTemplate(Guid id)
        {
            await using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var repo = new TemplateRepository(connection);

            Template template = null;
            var canModify = false;

            if (User.IsInRole("admin"))
            {
                template = await new TemplateRepository(connection).GetTemplateById(id);
                canModify = true;
            }
            else
            {
                var userId = Guid.Parse(User.Claims.First(c => c.Type == "userid").Value);
                (template, canModify) = await new TemplateRepository(connection).GetUserTemplateById(userId, id);
            }

            if (template == null)
                return NotFound(new ErrorResponse("Template not found"));

            if (!canModify)
                return BadRequest(new ErrorResponse("You can't modify this template"));
            
            await repo.DeleteTemplateById(id);

            return Ok(template);
        }

        [HttpPost]
        [Authorize]
        public async Task<ActionResult<Template>> PostNewTemplate(Template template)
        {
            try
            {
                template.Validate();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
            
            await using var connection = new SqliteConnection(_connectionString);
            connection.Open();
            
            var userId = Guid.Parse(User.Claims.First(c => c.Type == "userid").Value);

            var transaction = await connection.BeginTransactionAsync();

            var user = await new UserRepository(connection).GetUserById(userId);
            
            template.Id = Guid.NewGuid();
            try
            {
                await new TemplateRepository(connection).AddNewTemplate(template);
                await new GroupRepository(connection).InsertTemplatesToGroupById(user.DefaultGroupId,
                    new List<Guid> {template.Id});
            }
            catch (SqliteException err)
            {
                transaction.Rollback();
                return BadRequest(err.Message);
            }
            
            transaction.Commit();
            
            return Ok(template);
        }

        [HttpGet("{id}/tree")]
        [Authorize]
        public async Task<ActionResult<(ModelTreeNode, bool)>> GetModelTreeOfTemplate(Guid id)
        {
            await using var connection = new SqliteConnection(_connectionString);
            connection.Open();
            
            var repo = new TemplateRepository(connection);

            ModelTreeNode node = null;
            var canModify = false;

            if (User.IsInRole("admin"))
            {
                node = await repo.GetModelTree(id);
                canModify = true;
            }
            else
            {
                var userId = Guid.Parse(User.Claims.First(c => c.Type == "userid").Value);
                (node, canModify) = await repo.GetUserModelTree(userId, id);
            }

            if (node == null)
                return NotFound(new ErrorResponse("Template not found"));
            
            return (node, canModify);
        }

        [HttpPut("{id}/tree")]
        [Authorize]
        public async Task<ActionResult> PostModelTreeOfTemplate(Guid id, ModelTreeNode tree)
        {
            if (tree == null) return BadRequest();
            if (tree.Id != "root") return BadRequest("Root node id must be 'root'");
            try
            {
                tree.Validate();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
            
            await using var connection = new SqliteConnection(_connectionString);
            connection.Open();
            
            var repo = new TemplateRepository(connection);

            ModelTreeNode node = null;
            var canModify = false;

            if (User.IsInRole("admin"))
            {
                node = await repo.GetModelTree(id);
                canModify = true;
            }
            else
            {
                var userId = Guid.Parse(User.Claims.First(c => c.Type == "userid").Value);
                (node, canModify) = await repo.GetUserModelTree(userId, id);
            }

            if (node == null)
                return NotFound(new ErrorResponse("Template not found"));

            if (!canModify)
                return BadRequest(new ErrorResponse("You can't modify this template"));

            await repo.SetModelTree(id, tree);
            return Ok();
        }
        
    }
}