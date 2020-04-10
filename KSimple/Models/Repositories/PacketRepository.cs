using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dapper;
using KSimple.Models.Entities;
using Microsoft.Data.Sqlite;

namespace KSimple.Models.Repositories
{
    public class PacketRepository
    {
        private SqliteConnection _connection;

        public PacketRepository(SqliteConnection connection)
        {
            _connection = connection;
        }

        public async Task<Packet> GetLastPacketByStorageId(Guid storageId)
        {
            return await _connection.QueryFirstOrDefaultAsync<Packet>(@"
                    SELECT * FROM Packets
                    WHERE StorageId = @storageId
                    ORDER BY ServerTimestamp DESC
                    LIMIT 1
                ", new {storageId});
        }
        
        public async Task<IEnumerable<Packet>> GetPacketsByStorageIdAndTime(Guid storageId, long start, long end)
        {
            return await _connection.QueryAsync<Packet>(@"
                        SELECT * FROM Packets
                        WHERE StorageId = @storageId 
                        AND ServerTimestamp BETWEEN @start and @end
                    ", new {storageId, start, end});
        }

        public async Task<int> InsertNewPacket(Packet packet)
        {
            return await _connection.QueryFirstAsync<int>(@"
                    INSERT INTO Packets(StorageId, UserTimestamp, ServerTimestamp, Data) 
                    VALUES (@StorageId, @UserTimestamp, @ServerTimestamp, @Data);
                    SELECT last_insert_rowid();
                ", packet);
        }
    }
}