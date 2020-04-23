using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
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
using Microsoft.IdentityModel.Tokens;
// TODO Add get user by username
namespace KSimple.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly ApplicationContext _context;
        private readonly string _connectionString;

        public UsersController(ApplicationContext context, IConfiguration configuration)
        {
            _context = context;
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }
        
        [HttpGet]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult<IEnumerable<User>>> GetUsers()
        {
            await using var connection = new SqliteConnection(_connectionString);
            connection.Open();
            return Ok(await new UserRepository(connection).GetAllUser());
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<User>> GetUserById(Guid id)
        {
            if (User.Claims.First(c => c.Type == "userid").Value != id.ToString())
                return Unauthorized();
            
            await using var connection = new SqliteConnection(_connectionString);
            connection.Open();
            var user = await new UserRepository(connection).GetUserById(id);

            if (user == null)
                return NotFound();

            return Ok(user);
        }

        /// <summary>
        /// Create user with given username and other properties. Also create default group fro user and add link to it
        /// </summary>
        /// <param name="user">User to be created/param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult<User>> PostUser(User user)
        {
            if (user.Name == null)
                return BadRequest(new ErrorResponse("Username must be provided"));

            await using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var transaction = connection.BeginTransaction();
            
            var groupRepo = new GroupRepository(connection);

            var group = new Group()
            {
                Id = Guid.NewGuid(),
                Name = $"{user.Name}_Default_Group"
            };
            
            user.Id = Guid.NewGuid();
            user.DefaultGroupId = group.Id;

            var userGroupRight = new UserGroupRight()
            {
                GroupId = group.Id,
                UserId = user.Id,
                Rights = Right.GetAdminRights()
            };

            try
            {
                await groupRepo.InsertGroup(group);
                await new UserRepository(connection).AddUser(user);
                await groupRepo.InsertUsersToGroup(userGroupRight);
            }
            catch (SqliteException ex)
            {
                transaction.Rollback();
                return BadRequest(new ErrorResponse(ex.Message));

            }

            transaction.Commit();

            return CreatedAtAction("GetUserById", new {id = user.Id}, user);
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<ActionResult<User>> DeleteUserById(Guid id)
        {
            if (!User.IsInRole("admin") && User.Claims.First(c => c.Type == "userid").Value != id.ToString())
                return Unauthorized();
            
            await using var connection = new SqliteConnection(_connectionString);
            connection.Open();
            
            var repo = new UserRepository(connection);

            var user = await repo.GetUserById(id);

            if (user == null)
                return NotFound();

            await repo.DeleteUserById(id);

            return Ok(user);
        }

        [HttpPut("id")]
        [Authorize]
        public async Task<ActionResult> PutUser(User userFields, Guid id)
        {
            if (!User.IsInRole("admin") && User.Claims.First(c => c.Type == "userid").Value != id.ToString())
                return Unauthorized();
            
            await using var connection = new SqliteConnection(_connectionString);
            connection.Open();
            
            var repo = new UserRepository(connection);

            var user = await repo.GetUserById(id);

            if (user == null)
                return NotFound();
            
            user.Merge(userFields);

            await repo.UpdateUser(user);

            return Ok(user);
        }
    }
}