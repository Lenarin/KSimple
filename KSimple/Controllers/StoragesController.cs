using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Dapper;
using KSimple.Models;
using KSimple.Models.Entities;
using KSimple.Models.Misc;
using KSimple.Models.Repositories;
using KSimple.Models.Responses;
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
        private readonly IConfiguration _configuration;

        public StoragesController(ApplicationContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }
    
        [HttpGet]
        public async Task<ActionResult<IEnumerable<StorageResponse>>> GetStorages()
        {
            await using var connection = new SqliteConnection(_configuration.GetConnectionString("DefaultConnection"));
            connection.Open();
            var res = await new StorageRepository(connection).GetAllStorages();

            return Ok(res.ToList().ConvertAll(s => new StorageResponse(s)));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<StorageResponse>> GetStorageById(Guid id)
        {
            await using var connection = new SqliteConnection(_configuration.GetConnectionString("DefaultConnection"));
            connection.Open();
            var res = await new StorageRepository(connection).GetStorageById(id);

            return Ok(new StorageResponse(res));
        }

        [HttpPost]
        public async Task<ActionResult<StorageResponse>> PostStorage(Storage storage)
        {
            var data = new Dictionary<string, JToken>();
            
            foreach (var field in storage.StorageFields)
            {
                // data[field.Key] = StorageField.Parse(field.Value.DataType, field.Value.InitValue);
                if (!StorageField.Check(field.Value.DataType, field.Value.InitValue)) return BadRequest($"Init value on {field.Key} incorrect");
                data[field.Key] = field.Value.InitValue;
            }
            
            storage.Id = new Guid();
            
            await using var connection = new SqliteConnection(_configuration.GetConnectionString("DefaultConnection"));
            connection.Open();

            await new StorageRepository(connection).InsertStorage(storage);

            var packet = new Packet
            {
                StorageId = storage.Id,
                ServerTimestamp = DateTimeOffset.Now.ToUnixTimeSeconds(),
                Data = data,
            };

            await new PacketRepository(connection).InsertNewPacket(packet);
            
            return Ok(new StorageResponse(storage));
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> PutStorage(Guid id, Storage storage)
        {
            if (storage.Id != id) return BadRequest(new ErrorResponse("Different id's"));
            
            await using var connection = new SqliteConnection(_configuration.GetConnectionString("DefaultConnection"));
            connection.Open();

            new StorageRepository(connection).UpdateStorage(storage);

            return Ok();
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<Storage>> DeleteStorage(Guid id)
        {
            await using var connection = new SqliteConnection(_configuration.GetConnectionString("DefaultConnection"));
            connection.Open();

            return await new StorageRepository(connection).DeleteStorageById(id);
        }

        [HttpGet("{storageId}/packets")]
        [Consumes("application/x-www-form-urlencoded")]
        public async Task<ActionResult<Packet>> GetLastPacket(Guid storageId, [FromForm] long? start, [FromForm] long? end)
        {
/*
            var res = await _context.Packets.AsNoTracking()
                .OrderBy(p => p.ServerTimestamp)
                .FirstOrDefaultAsync(p => p.StorageId == storageId);
*/          
            await using var connection = new SqliteConnection(_configuration.GetConnectionString("DefaultConnection"));
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
                
                await using var connection = new SqliteConnection(_configuration.GetConnectionString("DefaultConnection"));
                connection.Open();
                Storage storage;
                Packet lastPacket;
                (storage, lastPacket) = await new StorageRepository(connection).GetStorageWithLastPacketByStorageId(storageId);
                    
                /*
                var storage = await _context.Storages.FindAsync(storageId);

                var lastPacket = await _context.Entry(storage)
                    .Collection(s => s.Packets)
                    .Query()
                    .OrderBy(p => p.ServerTimestamp)
                    .LastAsync();
                    */

                packet.Data = storage.ParseData(packet.Data, lastPacket.Data);
                packet.StorageId = storageId;
                packet.ServerTimestamp = DateTimeOffset.Now.ToUnixTimeSeconds();

                packet.Id = await new PacketRepository(connection).InsertNewPacket(packet);

                /*
                await _context.Packets.AddAsync(packet);

                await _context.SaveChangesAsync();
                */

                return Ok(packet);
            }
            catch (ArgumentNullException)
            {
                return NotFound();
            }

            
        }

    }
}