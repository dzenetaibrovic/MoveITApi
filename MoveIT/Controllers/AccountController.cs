using DataLayer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using MoveIT.Models;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace MoveIT.Controllers
{
    [AllowAnonymous]
    [Route("[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly MoveITModel _context;
        public AccountController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager, MoveITModel context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
        }
        [HttpPost("register")]       
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            Customer customer = new Customer()
            {
                FirstName = model.FirstName,
                LastName = model.LastName,
                Phone = model.Phone,
                Email = model.Email
            };           

            IdentityUser user = new IdentityUser()
            {
                UserName = model.Username,
                Email = model.Email
            };

            IdentityResult userResult = await _userManager.CreateAsync(user, model.Password);

            if (userResult.Succeeded)
            {
                customer.CustomerId = user.Id;
                _context.Add<Customer>(customer);

                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (Exception e)
                {
                    return StatusCode(500, new { Error = e.Message });
                }
                return Ok(user);                
            }

            foreach (var error in userResult.Errors)
                ModelState.AddModelError(error.Code, error.Description);

            return BadRequest(ModelState.Values);
        }
        [HttpPost("signIn")]      
        public async Task<IActionResult> SignIn(SignInViewModel model)
        {
            var signInResult = await _signInManager.PasswordSignInAsync(model.Username, model.Password, false, false);
            if (signInResult.Succeeded)
            {
                var user = await _userManager.FindByNameAsync(model.Username);              

                IdentityOptions identityOptions = new IdentityOptions();

                var claims = new Claim[]
                {
                    new Claim(identityOptions.ClaimsIdentity.UserIdClaimType,user.Id),
                    new Claim(identityOptions.ClaimsIdentity.UserNameClaimType,user.UserName)                   
                };

                var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("this-is-my-secret-key"));
                var signingCredentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);
                var token = new JwtSecurityToken(signingCredentials: signingCredentials, expires: DateTime.Now.AddHours(3), claims: claims);

                var obj = new
                {
                    Token = new JwtSecurityTokenHandler().WriteToken(token),
                    UserId = user.Id,
                    user.UserName
                };

                return Ok(obj);
            }
            return BadRequest(ModelState);
        }
        [HttpPost("signOut")]       
        public new async Task<IActionResult> SignOut()
        {
            await _signInManager.SignOutAsync();
            return NoContent();
        }
    }
}
