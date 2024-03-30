using LoginAPI.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace LoginAPI.Services
{
    public class JWTService
    {
        private readonly IConfiguration _config;
        private readonly SymmetricSecurityKey _jwtKey;

        public JWTService( IConfiguration config)
        {
            this._config = config;
            //jwtKey is used for both encripting and decripting the JWT token
            _jwtKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["JWT:Key"]));
        }
        public string CreateJWT(User user)
        {
            //declare Claims
            var userClaims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.GivenName, user.firstName),
                new Claim(ClaimTypes.Surname, user.lastName)
            };

            var credentials = new SigningCredentials(_jwtKey, SecurityAlgorithms.HmacSha256Signature);
            var tokenDesciptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(userClaims),
                Expires = DateTime.UtcNow.AddDays(int.Parse(_config["JWT:ExprireInDays"])),
                SigningCredentials = credentials,
                Issuer = _config["JWT:Issuer"]
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var jwt = tokenHandler.CreateToken(tokenDesciptor);
            return tokenHandler.WriteToken(jwt);

        }
    }
}
