using Microsoft.AspNetCore.Identity;
using Model.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.Extensions.Configuration;
using System.IdentityModel.Tokens.Jwt;
using Model.ViewModel.Common;
using Model.ViewModel;

namespace StackUnderflow.Infrastructure.Users
{
    public class UserService : IUserService
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManger;
        private readonly IConfiguration _config;
        public UserService(UserManager<User> userManager,
            SignInManager<User> signInManger,
            IConfiguration config)
        {
            _userManager = userManager;
            _signInManger = signInManger;
            _config = config;
        }
        public async Task<ApiResult<string>> Login(string username, string password)
        {
            var user = await _userManager.FindByNameAsync(username);
            if (user == null)
            {
                return new ApiErrorResult<string>("Tài khoản không tồn tại");
            }
            var result = await _signInManger.PasswordSignInAsync(user, password, true, false);
            //var roles = await _userManager.GetRolesAsync(user);
            if (!result.Succeeded)
            {
                return new ApiErrorResult<string>("Đăng nhập không đúng");
            }
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier,user.Id.ToString()),
                new Claim(ClaimTypes.Email,user.Email),
                new Claim(ClaimTypes.GivenName,user.FirstName),
                //new Claim(ClaimTypes.Role, string.Join(";",roles))

            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Tokens:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(_config["Tokens:Issuer"],
                _config["Tokens:Issuer"],
                claims,
                expires: DateTime.Now.AddHours(3),
                signingCredentials: creds);
            return new ApiSuccessResult<string>(new JwtSecurityTokenHandler().WriteToken(token));
        }

        public async Task<bool> Logout()
        {
            bool isT = false;
            await _signInManger.SignOutAsync();
            return isT;

        }

        public async Task<bool> Register(string userName, string passWord, string email, string firstName, string lastName, DateTime dob)
        {
            bool isT = false;
            var user = new User()
            {
                UserName = userName,
                Email = email,
                FirstName = firstName,
                LastName = lastName,
                Dob = dob

            };
            var result = await _userManager.CreateAsync(user, passWord);
            if (result.Succeeded)
            {
                return isT = true;
            }
            return isT;
        }
    }
}
