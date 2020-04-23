using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace KSimple.Models.Misc
{
    public class AuthOptions
    {
        public const string Issuer = "KSimpleMain";
        public const string Audience = "KSimpleUser";
        private const string Key = "qwertyasdfgh123456789";
        public const int AccessTokenLifetime = 180; // 3 hours
        public const int RefreshTokenLifetime = 20160; // 2 weeks

        public static SymmetricSecurityKey GetSymmetricSecurityKey()
        {
            return new SymmetricSecurityKey(Encoding.ASCII.GetBytes(Key));
        }
    }
}