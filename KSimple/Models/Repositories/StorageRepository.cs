using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using KSimple.Models.Entities;
using Microsoft.Data.Sqlite;

namespace KSimple.Models.Repositories
{
    public class StorageRepository
    {
        private readonly DbConnection _connection;

        public StorageRepository(DbConnection connection)
        {
            _connection = connection;
        }

        public async Task<(Storage, Packet)> GetStorageWithLastPacketByStorageId(Guid storageId)
        {
            return (await _connection.QueryAsync<Storage, Packet, (Storage, Packet)>(@"
                    SELECT * FROM Storages
                    JOIN Packets P on Storages.Id = P.StorageId
                    WHERE StorageId = @StorageId
                    ORDER BY ServerTimestamp DESC
                    LIMIT 1
                ", (storage, packet) => (storage, packet),
                new {StorageId = storageId})).First();
        }

        public async Task<IEnumerable<Storage>> GetAllStorages()
        {
            return await _connection.QueryAsync<Storage>(@"
                SELECT * FROM Storages
            ");
        }

        public async Task<IEnumerable<(Storage, bool)>> GetAllUserStorages(Guid userId)
        {
            return await _connection.QueryAsync<Storage, long, (Storage, bool)>(@"
                SELECT S.Id, S.UserDefinedId, S.Name, S.StorageFields, S.Status, S.TemplateId, Rights_CanModifyStorages CanModifyStorages FROM (
                    SELECT GroupId, Rights_CanModifyStorages FROM UserGroupRights
                    WHERE UserId = @userId AND Rights_CanReadStorages = 1) t1
                JOIN StorageGroups SG ON t1.GroupId = SG.GroupId
                JOIN Storages S on SG.StorageId = S.Id
            ", (storage, b) => (storage, b != 0), new {userId}, splitOn: "CanModifyStorages" );
        }

        public async Task<Storage> GetStorageById(Guid id)
        {
            return await _connection.QueryFirstOrDefaultAsync<Storage>(@"
                SELECT * FROM Storages
                WHERE Id = @id
            ", new {id});
        }

        public async Task<(Storage, bool)> GetUserStorageById(Guid userId, Guid storageId)
        {
            return (await _connection.QueryAsync<Storage, long, (Storage, bool)>(@"
                SELECT S.Id, S.UserDefinedId, S.Name, S.StorageFields, S.Status, S.TemplateId, Rights_CanModifyStorages CanModifyStorages FROM (
                    SELECT GroupId, Rights_CanModifyStorages FROM UserGroupRights
                    WHERE UserId = @userId AND Rights_CanReadStorages = 1) t1
                JOIN StorageGroups SG ON t1.GroupId = SG.GroupId
                JOIN Storages S on SG.StorageId = S.Id
                WHERE StorageId = @storageId
            ", (storage, b) => (storage, b != 0), new {userId}, splitOn: "CanModifyStorages")).ToArray()[0];
        }
        
        public async Task<Guid> InsertStorage(Storage storage)
        {
            return await _connection.QueryFirstAsync<Guid>(@"
                INSERT INTO Storages(Id, UserDefinedId, Name, StorageFields, Status, TemplateId) 
                VALUES (@Id, @UserDefinedId, @Name, @StorageFields, @Status, @TemplateId);
                SELECT Id FROM Storages
                WHERE _ROWID_ = last_insert_rowid();
            ", storage);
        }

        public async Task UpdateStorage(Storage storage)
        {
            await (_connection.QueryAsync(@"
                UPDATE Storages
                SET UserDefinedId = @UserDefinedId
                AND Name = @Name
                AND Status = @Status
                AND StorageFields = @StorageFields
                WHERE Id = @Id
            ", storage));
        }

        public async Task DeleteStorageById(Guid storageId)
        {
            await _connection.QueryAsync(@"
                DELETE FROM Storages
                WHERE Id = @Id;
            ");
        }
    }
}