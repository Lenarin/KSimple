using System;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;

namespace KSimple.Models.Entities
{
    public class RefreshToken
    {
        [Key]
        public string Token { get; set; }
        
        public DateTime ValidTo { get; set; }
        
        public Guid UserId { get; set; }

        public RefreshToken GenerateToken(DateTime valid, int size = 32)
        {
            var randomNumber = new byte[size];
            using var rand = RandomNumberGenerator.Create();
            rand.GetBytes(randomNumber);
            return new RefreshToken()
            {
                Token = Convert.ToBase64String(randomNumber),
                ValidTo = valid
            };
        }
    }
    
}