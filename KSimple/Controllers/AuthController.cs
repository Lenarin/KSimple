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
using Newtonsoft.Json;

namespace KSimple.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationContext _context;
        private readonly string _connectionString;

        public AuthController(ApplicationContext context, IConfiguration configuration)
        {
            _context = context;
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        [HttpPost("token/refresh")]
        public async Task<ActionResult> RefreshToken(RefreshRequest req)
        {
            await using var connection = new SqliteConnection(_connectionString);
            connection.Open();
            
            var repo = new RefreshTokenRepository(connection);

            var jwtHandler = new JwtSecurityTokenHandler();

            var (refreshToken, user) = await repo.FindTokenWithUser(req.RefreshToken);

            if (refreshToken == null)
                return BadRequest(new ErrorResponse("Token not found"));

            var (accessJwt, newRefreshToken) = CreateTokens(user);

            var encodedAccessJwt = jwtHandler.WriteToken(accessJwt);

            var transaction = connection.BeginTransaction();

            await repo.DeleteToken(req.RefreshToken);
            await repo.InsertToken(newRefreshToken, user.Id);
            
            transaction.Commit();
            
            return Ok(new
            {
                access_token = encodedAccessJwt, 
                access_token_valid_to = ((DateTimeOffset) accessJwt.ValidTo).ToUnixTimeMilliseconds(),
                refresh_token = newRefreshToken.Token,
                refresh_token_valid_to = ((DateTimeOffset) newRefreshToken.ValidTo).ToUnixTimeMilliseconds()
            });
        }

        [HttpPost("token/login")]
        public async Task<ActionResult> CreateToken(LoginRequest auth)
        {
            await using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var user = await new UserRepository(connection).GetUserByUsernameAndPass(auth.Login, auth.Password);
            if (user == null) return BadRequest(new ErrorResponse("User not found"));

            var (accessJwt, refreshToken) = CreateTokens(user);
            
            var jwtHandler = new JwtSecurityTokenHandler();
            
            var encodedAccessJwt = jwtHandler.WriteToken(accessJwt);

            await new RefreshTokenRepository(connection).InsertToken(refreshToken, user.Id);
            
            return Ok(new
            {
                access_token = encodedAccessJwt, 
                access_token_valid_to = ((DateTimeOffset) accessJwt.ValidTo).ToUnixTimeMilliseconds(),
                refresh_token = refreshToken.Token,
                refresh_token_valid_to = ((DateTimeOffset) refreshToken.ValidTo).ToUnixTimeMilliseconds()
            });
        }

        private static (JwtSecurityToken, RefreshToken) CreateTokens(User user)
        {
            var accessIdentity = GetIdentity(user, "accessToken");
            var refreshIdentity = GetIdentity(user, "refreshToken");

            var now = DateTime.UtcNow;
            
            var accessJwt = new JwtSecurityToken(
                issuer: AuthOptions.Issuer, 
                audience: AuthOptions.Audience,
                notBefore: now,
                claims: accessIdentity.Claims,
                expires: now.Add(TimeSpan.FromMinutes(AuthOptions.AccessTokenLifetime)),
                signingCredentials: new SigningCredentials(
                    AuthOptions.GetSymmetricSecurityKey(), 
                    SecurityAlgorithms.HmacSha256)
            );
            
            var refreshToken = new RefreshToken().GenerateToken(now.Add(TimeSpan.FromMinutes(AuthOptions.RefreshTokenLifetime)));

            return (accessJwt, refreshToken);
        }

        private static ClaimsIdentity GetIdentity(User user, string tokenType)
        {
            var claims = new List<Claim>
            {            
                new Claim(ClaimsIdentity.DefaultRoleClaimType, user.Role),
                new Claim("userid", user.Id.ToString())
            };
            
            var claimsIdentity = new ClaimsIdentity(claims, "Token", tokenType,
                user.Role);

            return claimsIdentity;
        }
    }
}