using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using KSimple.Models.Entities;

namespace KSimple.Models.Repositories
{
    public class RefreshTokenRepository
    {
        private readonly DbConnection _connection;

        public RefreshTokenRepository(DbConnection connection)
        {
            _connection = connection;
        }
        
        //public async Task InsertToken()

        public async Task<RefreshToken> FindToken(string token)
        {
            return await _connection.QueryFirstOrDefaultAsync<RefreshToken>(@"
                SELECT * FROM RefreshTokens
                WHERE Token = @token
            ", token);
        }

        public async Task<(RefreshToken, User)> FindTokenWithUser(string token)
        {
            return (await _connection.QueryAsync<RefreshToken, User, (RefreshToken, User)>(@"
                SELECT * FROM (
                    SELECT * FROM RefreshTokens
                    WHERE Token = @token
                    ) tok
                JOIN Users ON tok.UserId = Users.Id
            ", (refreshToken, user) => (refreshToken, user), new {token})).First();
        }

        public async Task<IEnumerable<RefreshToken>> GetAllTokensByUserId(Guid id)
        {
            return await _connection.QueryAsync<RefreshToken>(@"
                SELECT * FROM RefreshTokens
                WHERE UserId = @id
            ", new {id});
        }

        public async Task DeleteToken(string token)
        {
            await _connection.QueryAsync(@"
                DELETE FROM RefreshTokens
                WHERe Token = @token
            ", new {token});
        }

        public async Task DeleteAllTokensByUserId(Guid id)
        {
            await _connection.QueryAsync(@"
                DELETE FROM RefreshTokens
                WHERe UserId = @id
            ", new {id});
        }

        public async Task InsertToken(RefreshToken token, Guid userId)
        {
            await _connection.QueryAsync(@"
                INSERT INTO RefreshTokens(Token, ValidTo, UserId) 
                VALUES (@token, @validTo, @userId)
            ", new {token = token.Token, validTo = token.ValidTo, userId});
        }
    }
}