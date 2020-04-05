using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace KSimple.Models.Misc
{
    public class AuthOptions
    {
        public const string Issuer = "KSimpleMain";
        public const string Audience = "KSimpleUser";
        private const string Key = "qwertyasdfgh123456789";
        public const int Lifetime = 30;

        public static SymmetricSecurityKey GetSymmetricSecurityKey()
        {
            return new SymmetricSecurityKey(Encoding.ASCII.GetBytes(Key));
        }
    }
}