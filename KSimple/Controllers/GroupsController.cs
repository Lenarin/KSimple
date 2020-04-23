using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using KSimple.Models;
using KSimple.Models.Entities;
using KSimple.Models.Misc;
using KSimple.Models.Repositories;
using KSimple.Models.Requests;
using KSimple.Models.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace KSimple.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class GroupsController : ControllerBase
    {
        private readonly ApplicationContext _context;
        private readonly string _connectionString;

        public GroupsController(ApplicationContext context, IConfiguration configuration)
        {
            _context = context;
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }
        
        [HttpGet("all")]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult<IEnumerable<Group>>> GetGroups()
        {
            await using var connection = new SqliteConnection(_connectionString);
            connection.Open();
            
            return Ok(await new GroupRepository(connection).GetAllGroups());
        }

        [HttpGet]
        [Authorize]
        public async Task<ActionResult<IEnumerable<Group>>> GetUserGroups()
        {
            await using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var userId = Guid.Parse(User.Claims.First(c => c.Type == "userid").Value);
            
            return Ok(await new GroupRepository(connection).GetAllUserGroups(userId));
        }
        
        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<Group>> GetGroupById(Guid id)
        {
            await using var connection = new SqliteConnection(_connectionString);
            connection.Open();
            
            var userId = Guid.Parse(User.Claims.First(c => c.Type == "userid").Value);

            Group group = null;

            if (User.IsInRole("admin"))
                group = await new GroupRepository(connection).GetFullGroupById(id);
            else
                group = await new GroupRepository(connection).GetFullUserGroupById(userId, id);
            
            if (group == null)
                return NotFound();
            
            return Ok(GroupResponse.FromGroup(group));
        }

        [HttpPost]
        [Authorize]
        public async Task<ActionResult<Group>> PostGroup(Group group)
        {
            await using var connection = new SqliteConnection(_connectionString);
            connection.Open();
            
            var userId = Guid.Parse(User.Claims.First(c => c.Type == "userid").Value);

            var transaction = connection.BeginTransaction();
            
            var groupRepo = new GroupRepository(connection);
            
            group.Id = Guid.NewGuid();
            
            var userGroupRight = new UserGroupRight
            {
                UserId = userId,
                GroupId = group.Id,
                Rights = Right.GetAdminRights()
            };

            try
            {
                await groupRepo.InsertGroup(group);
                await groupRepo.InsertUsersToGroup(userGroupRight);
            }
            catch (SqliteException ex)
            {
                transaction.Rollback();
                return BadRequest(new ErrorResponse(ex.Message));
            }
            
            transaction.Commit();
            
            return CreatedAtAction("GetGroupById", new {id = group.Id}, group);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<Group>> DeleteGroupById(Guid id)
        {
            await using var connection = new SqliteConnection(_connectionString);
            connection.Open();
            
            var userId = Guid.Parse(User.Claims.First(c => c.Type == "userid").Value);

            Group group = null;
            
            var repo = new GroupRepository(connection);
            if (User.IsInRole("admin"))
                group = await repo.GetGroupById(id);
            else
                group = await repo.GetModerUserGroupById(userId, id);

            if (group == null)
                return NotFound();

            await repo.DeleteGroupById(id);

            return Ok(group);
        }

        [HttpPut("{id}")]
        [Authorize]
        public async Task<ActionResult> PutGroup(PutGroupRequest req, Guid id)
        {
            if (id == Guid.Empty) return BadRequest();

            await using var connection = new SqliteConnection(_connectionString);
            connection.Open();
            var transaction = connection.BeginTransaction();

            var userId = Guid.Parse(User.Claims.First(c => c.Type == "userid").Value);

            Group group = null;
            
            var groupRepo = new GroupRepository(connection);
            
            if (User.IsInRole("admin"))
                group = await groupRepo.GetGroupById(id);
            else
                group = await groupRepo.GetModerUserGroupById(userId, id);

            if (group == null)
                return NotFound();
            
            var res = new Dictionary<string, int>();

            try
            {
                if (!string.IsNullOrEmpty(req.Name))
                {
                    group.Name = req.Name;
                    await groupRepo.UpdateGroup(group);
                }


                if (req.StoragesToInsert != null && req.StoragesToInsert.Count != 0)
                    res["StoragesInserted"] = await groupRepo.InsertStoragesToGroupById(id, req.StoragesToInsert);
                else
                    res["StoragesInserted"] = 0;


                if (req.StoragesToDelete != null && req.StoragesToDelete.Count != 0)
                    res["StoragesDeleted"] = await groupRepo.DeleteStoragesFromGroupById(id, req.StoragesToDelete);
                else
                    res["StoragesDeleted"] = 0;


                if (req.TemplatesToInsert != null && req.TemplatesToInsert.Count != 0)
                    res["TemplatesInserted"] = await groupRepo.InsertTemplatesToGroupById(id, req.TemplatesToInsert);
                else
                    res["TemplatesInserted"] = 0;

                if (req.TemplatesToDelete != null && req.TemplatesToDelete.Count != 0)
                    res["TemplatesDeleted"] = await groupRepo.DeleteTemplatesFromGroupById(id, req.TemplatesToDelete);
                else
                    res["TemplatesDeleted"] = 0;

                if (req.UserRightsToInsert != null && req.UserRightsToInsert.Count != 0)
                    res["UsersInserted"] = await groupRepo.InsertUsersToGroupById(id, req.UserRightsToInsert);
                else
                    res["UsersInserted"] = 0;

                if (req.UsersToDelete != null && req.UsersToDelete.Count != 0)
                    res["UsersDeleted"] = await groupRepo.DeleteUsersFromGroupById(id, req.UsersToDelete);
                else
                    res["UsersDeleted"] = 0;
                
                
                transaction.Commit();

                return Ok(res);
            }
            catch (SqliteException ex)
            {
                transaction.Rollback();
                return BadRequest(new ErrorResponse(ex.Message));
            }

        }
    }
}