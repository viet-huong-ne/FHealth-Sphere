using Contract.Repositories.Entity;
using Contract.Repositories.Interface;
using Contract.Services.Interface;
using Core.Base;
using Core.Utils;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ModelViews.AccountModelViews;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Services.Service
{
    public class AccountService : IAccountService
    {
        private readonly IUnitOfWork _unitOfWork;

        public AccountService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Account> CreateAccount(CreateAccountModel model)
        {
            if (model == null || string.IsNullOrWhiteSpace(model.UserName))
            {
                throw new ArgumentNullException(nameof(model), "Account data is required and UserName cannot be empty or whitespace.");
            }

            if (model.Password.Length < 6)
            {
                throw new ArgumentException("Password must be at least 6 characters long.", nameof(model.Password));
            }

            var accountRepo = _unitOfWork.GetRepository<Account>();
            var existingAccount = await accountRepo.Entities
                .FirstOrDefaultAsync(a => a.UserName.ToLower() == model.UserName.ToLower() && !a.DeletedTime.HasValue);

            if (existingAccount != null)
            {
                throw new InvalidOperationException("An account with this username already exists.");
            }

            var account = new Account
            {
                UserName = model.UserName.Trim(),
                PhoneNumber = model.PhoneNumber,
                CreatedBy = "System",
                CreatedTime = DateTimeOffset.Now,
                LastUpdatedTime = DateTimeOffset.Now
            };

            var passwordHasher = new PasswordHasher<Account>();
            account.PasswordHash = passwordHasher.HashPassword(account, model.Password);

            await accountRepo.InsertAsync(account);
            await _unitOfWork.SaveAsync();

            return account;
        }

        public async Task<BasePaginatedList<Account>> GetAllAccounts(int pageNumber, int pageSize)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;

            var accountRepo = _unitOfWork.GetRepository<Account>();
            var query = accountRepo.Entities.Where(a => !a.DeletedTime.HasValue).OrderByDescending(a => a.CreatedTime);

            int totalCount = await query.CountAsync();
            var accounts = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();

            return new BasePaginatedList<Account>(accounts, totalCount, pageNumber, pageSize);
        }

        public async Task<Account> UpdateAccount(int id, UpdateAccountModel model)
        {
            if (model == null || string.IsNullOrWhiteSpace(model.UserName))
            {
                throw new ArgumentNullException(nameof(model), "Update data is required and UserName cannot be empty or whitespace.");
            }

            var accountRepo = _unitOfWork.GetRepository<Account>();
            var account = await accountRepo.Entities.FirstOrDefaultAsync(a => a.Id == id && !a.DeletedTime.HasValue);

            if (account == null)
            {
                throw new KeyNotFoundException($"Account with ID {id} not found or already deleted.");
            }

            var existingAccount = await accountRepo.Entities
                .FirstOrDefaultAsync(a => a.UserName.ToLower() == model.UserName.ToLower() && a.Id != id && !a.DeletedTime.HasValue);

            if (existingAccount != null)
            {
                throw new InvalidOperationException("An account with this username already exists.");
            }

            account.UserName = model.UserName.Trim();
            account.PhoneNumber = model.PhoneNumber;
            account.LastUpdatedTime = DateTimeOffset.Now;
            account.LastUpdatedBy = "System";

            if (!string.IsNullOrWhiteSpace(model.Password))
            {
                var passwordHasher = new PasswordHasher<Account>();
                account.PasswordHash = passwordHasher.HashPassword(account, model.Password);
            }

            await accountRepo.UpdateAsync(account);
            await _unitOfWork.SaveAsync();
            return account;
        }

        public async Task<bool> DeleteAccount(int id)
        {
            var accountRepo = _unitOfWork.GetRepository<Account>();
            var account = await accountRepo.Entities.FirstOrDefaultAsync(a => a.Id == id && !a.DeletedTime.HasValue);

            if (account == null)
            {
                return false;
            }

            account.DeletedTime = DateTimeOffset.Now;
            account.DeletedBy = "System";

            await accountRepo.UpdateAsync(account);
            await _unitOfWork.SaveAsync();
            return true;
        }
    }
}
