using Azure.Core;
using Contract.Repositories.Entity;
using Contract.Repositories.Interface;
using Contract.Services.Interface;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ModelViews.AuthModelViews;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Service
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _unitOfWork;
        public UserService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Account> CreateAccountAsync(Account account)
        {   
            await _unitOfWork.GetRepository<Account>().InsertAsync(account);
            await _unitOfWork.SaveAsync();
            return account;
        }

        public async Task<Account> GetUserByEmail(string email)
        {
            var user = await _unitOfWork.GetRepository<Account>().Entities.FirstOrDefaultAsync(x => x.Email.Equals(email));
            return user;
        }
    }
}
