using System.Security.Cryptography;
using System.Text;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers;

public class AccountController(UserManager<AppUser> userManager,ITokenService tokenService,IMapper mapper):BaseApiController
{
    [HttpPost("register")]
    public async Task<ActionResult<UserDto>> Register(RegisterDto registerDto)
    {
        if (await UserExists(registerDto.Username)) return BadRequest("Username is taken");

        using var hmac=new HMACSHA512();
        var user=mapper.Map<AppUser>(registerDto);
        user.UserName=registerDto.Username.ToLower();
        // user.PasswordHash=hmac.ComputeHash(Encoding.UTF8.GetBytes(registerDto.Password));
        // user.PasswordSalt=hmac.Key;
        // context.Users.Add(user);
        // await context.SaveChangesAsync();

        var result=await userManager.CreateAsync(user,registerDto.Password);
        if(!result.Succeeded) return BadRequest(result.Errors);
        
        //return user;
        return new UserDto
        {
            Username=user.UserName,
            Token= await tokenService.CreateToken(user),
            KnownAs=user.KnownAs,
            Gender=user.Gender
        };
    }

    [HttpPost("login")]
    public async Task<ActionResult<UserDto>> Login(LoginDto loginDto)
    {
        var user=await userManager.Users
        .Include(p=>p.photos).FirstOrDefaultAsync(x=>
        x.NormalizedUserName==loginDto.Username.ToUpper());
        if(user==null||user.UserName==null) return Unauthorized("Invalid username");
        var result= await userManager.CheckPasswordAsync(user,loginDto.Password);
        if(!result) return Unauthorized();

        return new UserDto
        {
            Username=user.UserName,
            KnownAs=user.KnownAs,
            Token= await tokenService.CreateToken(user),
            Gender=user.Gender,
            PhotoUrl=user.photos.FirstOrDefault(x=>x.IsMain)?.Url
        };
    }

    private async Task<bool> UserExists(string username)
    {
        return await userManager.Users.AnyAsync(x=>x.NormalizedUserName==username.ToUpper()); 
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
