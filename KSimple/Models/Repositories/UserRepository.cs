using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading.Tasks;
using Dapper;
using KSimple.Models.Entities;

namespace KSimple.Models.Repositories
{
    public class UserRepository
    {
        private DbConnection _connection;

        public UserRepository(DbConnection connection)
        {
            _connection = connection;
        }

        public async Task<IEnumerable<User>> GetAllUser()
        {
            return await _connection.QueryAsync<User>(@"
                SELECT * FROM Users
            ");
        }

        public async Task<User> GetUserById(Guid Id)
        {
            return await _connection.QueryFirstOrDefaultAsync<User>(@"
                SELECT * FROM Users
                WHERE Id = @Id
            ", new {Id});
        }

        public async Task AddUser(User user)
        {
            await _connection.QueryAsync(@"
                INSERT INTO Users(Id, Name, Email, Password, Role, DefaultGroupId) 
                VALUES (@Id, @Name, @Email, @Password, @Role, @DefaultGroupId)
            ", user);
        }

        public async Task UpdateUser(User user)
        {
            await _connection.QueryAsync(@"
                UPDATE Users
                SET Name = @Name,
                    Email = @Email,
                    Password = @Password,
                    Role = @Role
                WHERE Id = @Id
            ", user);
        }

        public async Task DeleteUserById(Guid Id)
        {
            await _connection.QueryAsync(@"
                DELETE FROM Users
                WHERE Id = @Id
            ", new {Id});
        }

        public async Task<User> GetUserByUsernameAndPass(string username, string password)
        {
            return await _connection.QueryFirstOrDefaultAsync<User>(@"
                SELECT *
                FROM Users
                WHERE Name = @username
                AND Password = @password
            ", new {username, password});
        }
    }
}