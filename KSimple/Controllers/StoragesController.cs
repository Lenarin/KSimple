using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Dapper;
using KSimple.Models;
using KSimple.Models.Entities;
using KSimple.Models.Misc;
using KSimple.Models.Repositories;
using KSimple.Models.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using JsonSerializer = Newtonsoft.Json.JsonSerializer;

namespace KSimple.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class StoragesController : ControllerBase
    {
        private readonly ApplicationContext _context;
        private readonly string _connectionString;

        public StoragesController(ApplicationContext context, IConfiguration configuration)
        {
            _context = context;
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }
    
        [HttpGet("all")]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult<IEnumerable<StorageResponse>>> GetStorages()
        {
            await using var connection = new SqliteConnection(_connectionString);
            connection.Open();
            var res = await new StorageRepository(connection).GetAllStorages();

            return Ok(res.ToList().ConvertAll(s => new StorageResponse(s, true)));
        }

        [HttpGet]
        [Authorize]
        public async Task<ActionResult<IEnumerable<StorageResponse>>> GetUserStorages()
        {
            await using var connection = new SqliteConnection(_connectionString);
            connection.Open();
            
            var userId = Guid.Parse(User.Claims.First(c => c.Type == "userid").Value);

            var res = await new StorageRepository(connection).GetAllUserStorages(userId);

            return Ok(res.ToList().ConvertAll(pair => new StorageResponse(pair.Item1, pair.Item2)));
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<StorageResponse>> GetStorageById(Guid id)
        {
            await using var connection = new SqliteConnection(_connectionString);
            connection.Open();
            
            var userId = Guid.Parse(User.Claims.First(c => c.Type == "userid").Value);

            Storage storage = null;
            bool canModify = false;

            if (User.IsInRole("admin"))
            {
                storage = await new StorageRepository(connection).GetStorageById(id);
                canModify = true;
            }
            else
                (storage, canModify) = await new StorageRepository(connection).GetUserStorageById(userId, id);

            if (storage == null)
                return NotFound();

            return Ok(new StorageResponse(storage, canModify));
        }
        
        
        /// <summary>
        /// Create a new storage and add it to database.
        /// If templateId isn't null try to generate storageFields from that template.
        /// If templateId is null validate provided storageFields
        /// </summary>
        /// <param name="storage">Storage to be added</param>
        /// <returns>Added storage</returns>
        [HttpPost]
        [Authorize]
        public async Task<ActionResult<StorageResponse>> PostStorage(Storage storage)
        {
            await using var connection = new SqliteConnection(_connectionString);
            connection.Open();
            
            var userId = Guid.Parse(User.Claims.First(c => c.Type == "userid").Value);

            var user = await new UserRepository(connection).GetUserById(userId);

            if (user == null)
                return NotFound(new ErrorResponse("User not found"));
            
            var data = new Dictionary<string, JToken>();

            if (storage.TemplateId != null)
            {
                var tree = await new TemplateRepository(connection).GetModelTree(storage.TemplateId.GetValueOrDefault());
                if (tree == null) return BadRequest(new ErrorResponse("Template not found"));
                storage.StorageFields = tree.ToStorageFields();
            }

            foreach (var (name, storageField) in storage.StorageFields)
            {
                if (!StorageField.Check(storageField.DataType, storageField.InitValue)) return BadRequest(
                    new ErrorResponse($"Init value on {name} incorrect"));
                data[name] = storageField.InitValue;
            }

            storage.Id = Guid.NewGuid();

            var transaction = connection.BeginTransaction();

            try
            {

                await new StorageRepository(connection).InsertStorage(storage);

                var packet = new Packet
                {
                    StorageId = storage.Id,
                    ServerTimestamp = DateTimeOffset.Now.ToUnixTimeSeconds(),
                    Data = data,
                };

                await new PacketRepository(connection).InsertNewPacket(packet);

                await new GroupRepository(connection).InsertStoragesToGroupById(user.DefaultGroupId,
                    new List<Guid> {storage.Id});

            }
            catch (SqliteException ex)
            {
                transaction.Rollback();
                return BadRequest(ex.Message);
            }
            
            transaction.Commit();

            return Ok(new StorageResponse(storage));
        }

        [HttpPut("{id}")]
        [Authorize]
        public async Task<ActionResult> PutStorage(Guid id, Storage newStorage)
        {
            if (newStorage.Id != id) return BadRequest(new ErrorResponse("Different id's"));

            await using var connection = new SqliteConnection(_connectionString);
            connection.Open();
            
            var storageRepo = new StorageRepository(connection);

            Storage storage = null;
            var canModifyStorage = false;
            
            if (User.IsInRole("admin"))
            {
                storage = await storageRepo.GetStorageById(id);
                canModifyStorage = true;
            }
            else
            {
                var userId = Guid.Parse(User.Claims.First(c => c.Type == "userid").Value);
                (storage, canModifyStorage) = await storageRepo.GetUserStorageById(userId, id);
            }
            
            if (storage == null)
                return NotFound(new ErrorResponse("Storage not found"));

            if (!canModifyStorage)
                return BadRequest(new ErrorResponse("You can't modify this storage"));
            
            storage.Merge(newStorage);

            await storageRepo.UpdateStorage(storage);

            return Ok(storage);
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<ActionResult<Storage>> DeleteStorage(Guid id)
        {
            await using var connection = new SqliteConnection(_connectionString);
            connection.Open();
            
            var storageRepo = new StorageRepository(connection);

            Storage storage = null;
            var canModifyStorage = false;
            
            if (User.IsInRole("admin"))
            {
                storage = await storageRepo.GetStorageById(id);
                canModifyStorage = true;
            }
            else
            {
                var userId = Guid.Parse(User.Claims.First(c => c.Type == "userid").Value);
                (storage, canModifyStorage) = await storageRepo.GetUserStorageById(userId, id);
            }
            
            if (storage == null)
                return NotFound(new ErrorResponse("Storage not found"));

            if (!canModifyStorage)
                return BadRequest(new ErrorResponse("You can't modify this storage"));

            await storageRepo.DeleteStorageById(id);
            
            return Ok(storage);
        }
        
        // TODO Add packet authorisation 

        [HttpGet("{storageId}/packets")]
        [Consumes("application/x-www-form-urlencoded")]
        public async Task<ActionResult<Packet>> GetLastPacket(Guid storageId, [FromForm] long? start, [FromForm] long? end)
        {
        
            await using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            if (start != null && end != null)
            {
                try
                {
                    var res = await 
                        new PacketRepository(connection).GetPacketsByStorageIdAndTime(storageId, (long) start, (long) end);

                    return Ok(res);
                }
                catch (ArgumentNullException)
                {
                    return NotFound();
                }
            }
            else
            {
                var res = await 
                    new PacketRepository(connection).GetLastPacketByStorageId(storageId);
                
                if (res == null)
                {
                    return NotFound(new ErrorResponse("Storage not found"));
                }

                return Ok(res);
            }
        }

        [HttpPost("{storageId}/packets")]
        public async Task<ActionResult> PostNewPacket(Packet packet, Guid storageId)
        {
            try
            {
                
                await using var connection = new SqliteConnection(_connectionString);
                connection.Open();
                Storage storage;
                Packet lastPacket;
                (storage, lastPacket) = await new StorageRepository(connection).GetStorageWithLastPacketByStorageId(storageId);
                
                packet.Data = storage.ParseData(packet.Data, lastPacket.Data);
                packet.StorageId = storageId;
                packet.ServerTimestamp = DateTimeOffset.Now.ToUnixTimeSeconds();

                packet.Id = await new PacketRepository(connection).InsertNewPacket(packet);
                
                return Ok(packet);
            }
            catch (ArgumentNullException)
            {
                return NotFound();
            }

            
        }

    }
}