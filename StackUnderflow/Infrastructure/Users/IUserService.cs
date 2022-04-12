using Model.ViewModel.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StackUnderflow.Infrastructure.Users
{
    public interface IUserService
    {
        Task<ApiResult<string>> Login(string username, string password);
        Task<bool> Register(string userName, string passWord, string email, string firstName, string lastName, DateTime dob);
        Task<bool> Logout();
    }
}
