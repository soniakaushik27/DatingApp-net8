using System.Security.Cryptography;
using System.Text;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers;

public class AccountController(DataContext context,ITokenService tokenService,IMapper mapper):BaseApiController
{
    [HttpPost("register")]
    public async Task<ActionResult<UserDto>> Register(RegisterDto registerDto)
    {
        if (await UserExists(registerDto.Username)) return BadRequest("Username is taken");

        using var hmac=new HMACSHA512();
        var user=mapper.Map<AppUser>(registerDto);
        user.UserName=registerDto.Username.ToLower();
        user.PasswordHash=hmac.ComputeHash(Encoding.UTF8.GetBytes(registerDto.Password));
        user.PasswordSalt=hmac.Key;
        context.Users.Add(user);
        await context.SaveChangesAsync();
        
        //return user;
        return new UserDto
        {
            Username=user.UserName,
            Token=tokenService.CreateToken(user),
            KnownAs=user.KnownAs
        };
    }

    [HttpPost("login")]
    public async Task<ActionResult<UserDto>> Login(LoginDto loginDto)
    {
        var user=await context.Users
        .Include(p=>p.photos).FirstOrDefaultAsync(x=>
        x.UserName==loginDto.Username.ToLower());
        if(user==null) return Unauthorized("Invalid username");
        using var hmac= new HMACSHA512(user.PasswordSalt);
        var computedHash=hmac.ComputeHash(Encoding.UTF8.GetBytes(loginDto.Password));
        for (int i = 0; i < computedHash.Length; i++)
        {
            if(computedHash[i]!=user.PasswordHash[i]) return Unauthorized("Invalid password");
        }
        //return user;
        return new UserDto
        {
            Username=user.UserName,
            KnownAs=user.KnownAs,
            Token=tokenService.CreateToken(user),
            PhotoUrl=user.photos.FirstOrDefault(x=>x.IsMain)?.Url
        };
    }

    private async Task<bool> UserExists(string username)
    {
        return await context.Users.AnyAsync(x=>x.UserName.ToLower()==username.ToLower()); 
    }
    // public async Task<ActionResult<AppUser>> Register(string username,string password)
    // {
    //     using var hmac=new HMACSHA512();
    //     var user=new AppUser
    //     {
    //         UserName=username,
    //         PasswordHash=hmac.ComputeHash(Encoding.UTF8.GetBytes(password)),
    //         PasswordSalt=hmac.Key
    //     };
    //     context.Users.Add(user);
    //     await context.SaveChangesAsync();
    //     return user;
    // }

}
