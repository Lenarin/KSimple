using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Dapper;
using KSimple.Models;
using KSimple.Models.Entities;
using KSimple.Models.Misc;
using KSimple.Models.Responses;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using JsonSerializer = Newtonsoft.Json.JsonSerializer;

// TODO Put/Del storages

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
            return (await _context.Storages.ToListAsync()).ConvertAll(x => new StorageResponse(x));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<StorageResponse>> GetStorageById(Guid id)
        {
            return new StorageResponse(await _context.Storages.FindAsync(id));

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
            
            await _context.Storages.AddAsync(storage);

            var packet = new Packet
            {
                StorageId = storage.Id,
                ServerTimestamp = DateTimeOffset.Now.ToUnixTimeSeconds(),
                Data = data,
            };

            await _context.Packets.AddAsync(packet);

            await _context.SaveChangesAsync();
                
            return Ok(new StorageResponse(storage));
        }

        [HttpGet("{storageId}/packets")]
        public async Task<ActionResult<Packet>> GetLastPacket(Guid storageId)
        {
/*
            var res = await _context.Packets.AsNoTracking()
                .OrderBy(p => p.ServerTimestamp)
                .FirstOrDefaultAsync(p => p.StorageId == storageId);
*/

            await using var connection = new SqliteConnection(_configuration.GetConnectionString("DefaultConnection"));
            connection.Open();
            var res = await connection.QueryFirstOrDefaultAsync<Packet>(@"
                SELECT * FROM Packets
                WHERE StorageId = @StorageId
                ORDER BY ServerTimestamp DESC
                LIMIT 1
                ", new {StorageId = storageId});
            
            if (res == default(Packet))
            {
                return NotFound(new ErrorResponse("Storage not found"));
            }

            return Ok(res);
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
                (storage, lastPacket) = (await connection.QueryAsync<Storage, Packet, (Storage, Packet)>(@"
                    SELECT * FROM Storages
                    JOIN Packets P on Storages.Id = P.StorageId
                    WHERE StorageId = @StorageId
                    ORDER BY ServerTimestamp DESC
                    LIMIT 1
                ", (storage, packet) => (storage, packet),
                    new {StorageId = storageId})).FirstOrDefault();
                    
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

                packet.Id = await connection.QueryFirstAsync<int>(@"
                    INSERT INTO Packets(StorageId, UserTimestamp, ServerTimestamp, Data) 
                    VALUES (@StorageId, @UserTimestamp, @ServerTimestamp, @Data);
                    SELECT last_insert_rowid();
                ", packet);

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