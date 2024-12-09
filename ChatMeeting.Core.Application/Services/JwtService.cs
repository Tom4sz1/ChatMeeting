using ChatMeeting.Core.Domain.Dtos;
using ChatMeeting.Core.Domain.Interfaces.Services;
using ChatMeeting.Core.Domain.Models;
using ChatMeeting.Core.Domain.Options;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace ChatMeeting.Core.Application.Services
{
    public class JwtService : IJwtService
    {
        private readonly JwtSettingsOption _jwtSettingsOption;

        public JwtService(IOptions<JwtSettingsOption> jwtSettingOption)
        {
            _jwtSettingsOption = jwtSettingOption.Value;
        }

        public AuthDto GenerateJwtToken(User user)
        {
            var claims = GetClaims(user);
            var expiryDate = DateTime.Now.AddMinutes(Convert.ToDouble(_jwtSettingsOption.ExpiryInMinutes));
            var creds = GetCredencials();

            var token = new JwtSecurityToken(
                claims: claims,
                expires: expiryDate,
                signingCredentials: creds
                );

            return new AuthDto()
            {
                Token = new JwtSecurityTokenHandler().WriteToken(token),
                ExpiredDate = expiryDate
            };
        }

        private SigningCredentials GetCredencials()
        {
            var byteScretKey = Encoding.ASCII.GetBytes( _jwtSettingsOption.SecretKey );
            var key = new SymmetricSecurityKey(byteScretKey);
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            return creds;
        }

        private Claim[] GetClaims(User user)
        {
            return new Claim[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Username),
                new Claim(JwtRegisteredClaimNames.Jti, user.Id.ToString())
            };
        }
    }
}
