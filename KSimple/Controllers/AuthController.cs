using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.Tasks;
using KSimple.Models;
using KSimple.Models.Misc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace KSimple.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationContext _context;

        public AuthController(ApplicationContext context)
        {
            _context = context;
        }

        [HttpPost("token")]
        public async Task<ActionResult> CreateToken(Dictionary<string, string> auth)
        {
            var identity = await GetIdentity(auth["username"], auth["password"]);
            if (identity == null) return BadRequest(new {error = "Invalid username or password"});

            var now = DateTime.UtcNow;
            
            var jwt = new JwtSecurityToken(
                issuer: AuthOptions.Issuer, 
                audience: AuthOptions.Audience,
                notBefore: now,
                claims: identity.Claims,
                expires: now.Add(TimeSpan.FromMinutes(AuthOptions.Lifetime)),
                signingCredentials: new SigningCredentials(
                    AuthOptions.GetSymmetricSecurityKey(), 
                    SecurityAlgorithms.HmacSha256)
            );
            
            var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);

            return Ok(new {access_token = encodedJwt, username = identity.Name});
        }

        private async Task<ClaimsIdentity> GetIdentity(string username, string password)
        {
            var user = await _context.Users.FirstOrDefaultAsync(user =>
                user.Name == username && user.Password == password);

            if (user == null) return null;
            
            var claims = new List<Claim>
            {
                new Claim(ClaimsIdentity.DefaultNameClaimType, user.Name),
                new Claim(ClaimsIdentity.DefaultRoleClaimType, user.Role),
                new Claim("userid", user.Id.ToString()),
            };
            
            ClaimsIdentity claimsIdentity = new ClaimsIdentity(claims, "Token", ClaimsIdentity.DefaultNameClaimType,
                ClaimsIdentity.DefaultRoleClaimType);

            return claimsIdentity;
        }
    }
}