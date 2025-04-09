using Dermatologiya.Server.AllDTOs;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using DotNetEnv;

namespace Dermatologiya.Server.Services
{
    public class JwtService
    {

        //private readonly IConfiguration _configuration;
        //private readonly string userName = "A382F76E8796583A59515AA95FA89";
        //private readonly string password = "zhqG4136o4`_jjcoP@:VU=On9jyt6:f(mm3zACIH4+|L(3I$Ce>6)F#gZihPbE.";

        //public JwtService(IConfiguration configuration)
        //{
        //    _configuration = configuration;
        //}

        //public string GenerateToken(LoginDTO model)
        //{
        //    if (!model.Username.Equals(userName) && !model.Password.Equals(password))
        //    {
        //        throw new Exception("Username yoki parol noto'g'ri");
        //    }
        //    var jwtSettings = _configuration.GetSection("JwtSettings");
        //    var secretKey = jwtSettings["SecretKey"];
        //    var issuer = jwtSettings["Issuer"];
        //    var audience = jwtSettings["Audience"];
        //    var tokenLifetime = int.Parse(jwtSettings["TokenLifetime"]); 

        //    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        //    var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        //    var claims = new[]
        //    {
        //        new Claim(ClaimTypes.Name, model.Username),
        //        new Claim(ClaimTypes.Role, "Admin") // Faqat adminlar uchun
        //    };

        //    var token = new JwtSecurityToken(
        //        issuer: issuer,
        //        audience: audience,
        //        claims: claims,
        //        expires: DateTime.UtcNow.AddSeconds(tokenLifetime),
        //        signingCredentials: credentials
        //    );

        //    return new JwtSecurityTokenHandler().WriteToken(token);
        //}

        //internal object Logout()
        //{
        //    throw new NotImplementedException();
        //}



        private readonly string userName;
        private readonly string password;
        private readonly string secretKey;
        private readonly string issuer;
        private readonly string audience;
        private readonly int tokenLifetime;
        public JwtService(string userName, string password, string secretKey, string issuer, string audience, int tokenLifetime)
        {
            this.userName = userName;
            this.password = password;
            this.secretKey = secretKey;
            this.issuer = issuer;
            this.audience = audience;
            this.tokenLifetime = tokenLifetime;
        }




        public string GenerateToken(LoginDTO model)
        {
            if (!model.Username.Equals(userName) || !model.Password.Equals(password)) // && => || ni o'zgartirdim
            {
                throw new Exception("Username yoki parol noto'g'ri");
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.Name, model.Username),
                new Claim(ClaimTypes.Role, "Admin") // Faqat adminlar uchun
            };

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddSeconds(tokenLifetime),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public object Logout()
        {
            throw new NotImplementedException();
        }
    }
}


//using System;
//using System.IdentityModel.Tokens.Jwt;
//using System.Security.Claims;
//using System.Text;
//using Dermatologiya.Server.AllDTOs;
//using Microsoft.Extensions.Configuration;
//using Microsoft.IdentityModel.Tokens;

//namespace Dermatologiya.Server.Services
//{
//    public class JwtService
//    {
//        private readonly string _secretKey;
//        private readonly string _issuer;
//        private readonly string _audience;
//        private readonly int _tokenLifetime;
//        private readonly string _adminUsername;
//        private readonly string _adminPassword;

//        public JwtService(IConfiguration config)
//        {
//            _secretKey = config["JWTSETTINGS__SECRETKEY"];
//            _issuer = config["JWTSETTINGS__ISSUER"];
//            _audience = config["JWTSETTINGS__AUDIENCE"];
//            _tokenLifetime = int.Parse(config["JWTSETTINGS__TOKENLIFETIME"]);
//            _adminUsername = config["ADMIN__USERNAME"];
//            _adminPassword = config["ADMIN__PASSWORD"];
//        }

//        public string GenerateToken(LoginDTO model)
//        {
//            if (model.Username != _adminUsername || model.Password != _adminPassword)
//                throw new UnauthorizedAccessException("Noto‘g‘ri login yoki parol!");

//            var claims = new[]
//            {
//                new Claim(ClaimTypes.Name, model.Username)
//            };

//            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));
//            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

//            var token = new JwtSecurityToken(
//                issuer: _issuer,
//                audience: _audience,
//                claims: claims,
//                expires: DateTime.UtcNow.AddMinutes(_tokenLifetime),
//                signingCredentials: creds
//            );

//            return new JwtSecurityTokenHandler().WriteToken(token);
//        }

//        public string Logout()
//        {
//            // Frontend tokenni o‘chirishni o‘z zimmasiga oladi.
//            return "Logged out (token must be deleted from client-side).";
//        }
//    }
//}



