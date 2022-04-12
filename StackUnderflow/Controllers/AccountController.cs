using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Model.Entities;
using StackUnderflow.Infrastructure.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.IdentityModel.Tokens;
using Microsoft.IdentityModel.Logging;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Http;

namespace StackUnderflow.Controllers
{
    public class AccountController : Controller
    {
        private readonly IUserService _userService;    
        private readonly IConfiguration _configuration;
        public AccountController(IUserService userService,
            IConfiguration configuration)
        {
            _userService = userService;
            _configuration = configuration;
        }
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }
       [HttpPost]
        public async Task<IActionResult> Login(string username, string password)
        {
            var result = await _userService.Login(username, password);

            if (result.IsSuccessed)
            {
                var userPrincipal = this.ValidateToken(result.ResultObj);
                var authProperties = new AuthenticationProperties
                {
                    ExpiresUtc = DateTimeOffset.UtcNow.AddHours(10),
                    IsPersistent = false
                };
                
                HttpContext.Session.SetString("Token", result.ResultObj);
                await HttpContext.SignInAsync(
                            CookieAuthenticationDefaults.AuthenticationScheme,
                            userPrincipal,
                            authProperties);

                return RedirectToAction("Index", "Home");
            }
            TempData["ErrorMessage"] = "Đăng nhập không thành công";
            return RedirectToAction("Login", "Account");
        }
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Register(string username, string email, string password, string firstname, string lastname, DateTime dob)
        {
            var user = await _userService.Register(username, password, email, firstname, lastname, dob);

            if (user == true)
            {
                return RedirectToAction("Login", "Account");
            }

            return RedirectToAction("Register", "Account");
        }
        public async Task<IActionResult> Logout()
        {
            await _userService.Logout();
            return RedirectToAction("Index", "Home");
        }
        private ClaimsPrincipal ValidateToken(string jwtToken)
        {
            IdentityModelEventSource.ShowPII = true;

            SecurityToken validatedToken;
            TokenValidationParameters validationParameters = new TokenValidationParameters();

            validationParameters.ValidateLifetime = true;

            validationParameters.ValidAudience = _configuration["Tokens:Issuer"];
            validationParameters.ValidIssuer = _configuration["Tokens:Issuer"];
            validationParameters.IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Tokens:Key"]));

            ClaimsPrincipal principal = new JwtSecurityTokenHandler().ValidateToken(jwtToken, validationParameters, out validatedToken);

            return principal;
        }
    }
}
