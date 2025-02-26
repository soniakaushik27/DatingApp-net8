using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using API.Entities;
using API.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

namespace API.Services;

public class TokenService(IConfiguration config,UserManager<AppUser> userManager) : ITokenService
{
    public async Task<string> CreateToken(AppUser user)
    {
        var tokenkey=config["TokenKey"]?? throw new Exception("Cannot access tokenkey from appsetting");
        if(tokenkey.Length<64) throw new Exception("your tokenkey needs to be longer");
        var key=new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenkey));
        if(user.UserName==null) throw new Exception("No username for user");
        var claims=new List<Claim>
        {
            new(ClaimTypes.NameIdentifier,user.Id.ToString()),
            new (ClaimTypes.Name,user.UserName)
        };
        var roles=await userManager.GetRolesAsync(user);
        claims.AddRange(roles.Select(role=>new Claim(ClaimTypes.Role,role)));
        var cred=new SigningCredentials(key,SecurityAlgorithms.HmacSha512Signature);
        var tokenDescriptor= new SecurityTokenDescriptor
        {
            Subject=new ClaimsIdentity(claims),
            Expires=DateTime.UtcNow.AddDays(7),
            SigningCredentials=cred
        };
        var tokenHandler=new JwtSecurityTokenHandler();
        var token=tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}
