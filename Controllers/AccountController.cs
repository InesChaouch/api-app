using System;
using System.Security.Cryptography;
using System.Text;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    public class AccountController(DataContext context, ITokenService tokenService) : BaseApiController
    {
    [HttpPost("register")]
    public async Task<ActionResult<UserDto>> Register(RegisterDto registerdto)
    { 
        if (await UserExists(registerdto.Username)) return BadRequest("Username is taken");
        return Ok();
    //     using var hmac = new HMACSHA512(); // put using otherwise garbage collector will delete this

    //     var user = new AppUser
    //     {
    //         UserName = registerdto.Username.ToLower(),
    //         PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(registerdto.Password)),
    //         PasswordSalt = hmac.Key
    //     };

    //     context.Users.Add(user);
    //     await context.SaveChangesAsync();

    //     return new UserDto
    //    {
    //     Username = user.UserName,
    //     Token = tokenService.CreateToken(user),
    //    };
    }
    public async Task<bool> UserExists(string username){
        return await context.Users.AnyAsync(x => x.UserName.ToLower() == username.ToLower());
    }
    [HttpPost("login")]
    public async Task<ActionResult<UserDto>> Login(LoginDto logindto)
    {
       var user = await context.Users.FirstOrDefaultAsync(x => x.UserName == logindto.UserName.ToLower());
       
       if (user == null) return Unauthorized("invalid username or password");

       using var hmac = new HMACSHA512(user.PasswordSalt);

       var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(logindto.Password));

       for (int i = 0; i<computedHash.Length; i++)
       {
        if (computedHash[i] != user.PasswordHash[i]) return Unauthorized("invalid Password");
       }

       return new UserDto
       {
        Username = user.UserName,
        Token = tokenService.CreateToken(user),
       };
    }


    }
}