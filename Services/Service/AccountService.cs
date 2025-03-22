using Contract.Repositories.Entity;
using Contract.Repositories.Interface;
using Contract.Services.Interface;
using Core.Base;
using Core.Utils;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ModelViews.AccountModelViews;
using ModelViews.AccountModelViews.Request;
using ModelViews.AccountModelViews.Response;
using ModelViews.PatientInfoModelViews.Response;
using ModelViews.WatcherModel;
using System;
using System.Collections.Generic;
using System.Data;
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

        public async Task<BasePaginatedList<AccountModelResponse>> GetAllAccounts(int pageNumber, int pageSize)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;

            var accountRepo = _unitOfWork.GetRepository<Account>();
            var query = accountRepo.Entities.Where(a => !a.DeletedTime.HasValue).OrderByDescending(a => a.CreatedTime);

            int totalCount = await query.CountAsync();
            var accounts = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();
            List<AccountModelResponse> result = new List<AccountModelResponse>();
            foreach (var account in accounts)
            {
                var Role = await (
                                    from userRole in _unitOfWork.GetRepository<ApplicationUserRoles>().Entities
                                    join roleEntity in _unitOfWork.GetRepository<ApplicationRole>().Entities
                                    on userRole.RoleId equals roleEntity.Id
                                    where userRole.UserId == account.Id
                                    select roleEntity.Name
                                 ).FirstOrDefaultAsync(); // get Role for user
                var Paitient = await _unitOfWork.GetRepository<Watcher>().Entities.Where(n => n.RelativeId == account.Id).ToListAsync();
                var WatcherResponse = new List<WatcherResponse>();
                foreach (var watch in Paitient)
                {
                    WatcherResponse.Add(new WatcherResponse
                    {
                        PatientId = watch.PatientId, // Giả sử Patient có thuộc tính Id
                        PatientName = watch.Patient.FullName // Giả sử Patient có thuộc tính Name
                    });
                }
                var info = account.PatientInformation;
                var PatientInfo = new PatientInfoResponse();
                if (info != null)
                {
                    PatientInfo = new PatientInfoResponse
                {
                    DateOfBirth = info.First().DateOfBirth,
                    Gender = info.First().Gender,

                };
                }
                var AccountModel = new AccountModelResponse
                {
                    Id = account.Id,
                    FullName = account.FullName,
                    Email = account.Email,
                    PhoneNumber = account.PhoneNumber,
                    FCMToken = account.FCMToken,
                    Role = Role,
                    PatientInfo = PatientInfo,
                    WatcherResponses = WatcherResponse
                };
                result.Add(AccountModel);
                Console.WriteLine(AccountModel.Role);
            }

            return new BasePaginatedList<AccountModelResponse>(result, totalCount, pageNumber, pageSize);
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

        public async Task<AccountModelResponse> GetAccountById(int Id)
        {
            var account = _unitOfWork.GetRepository<Account>().Entities.Include(n => n.PatientInformation).FirstOrDefault(n => n.Id == Id);
            var Role = await (
                    from userRole in _unitOfWork.GetRepository<ApplicationUserRoles>().Entities
                    join roleEntity in _unitOfWork.GetRepository<ApplicationRole>().Entities
                    on userRole.RoleId equals roleEntity.Id
                    where userRole.UserId == account.Id
                    select roleEntity.Name
                 ).FirstOrDefaultAsync(); // get Role for user
            var Patient = await _unitOfWork.GetRepository<Watcher>().Entities.Include(n => n.Patient).Where(n => n.RelativeId == Id).ToListAsync();
            var WatcherResponses = new List<WatcherResponse>();
            if (Patient.Any())
            {        
            foreach (var watch in Patient)
            {
                WatcherResponses.Add(new WatcherResponse
                {
                    PatientId = watch.PatientId, // Giả sử Patient có thuộc tính Id
                    PatientName = watch.Patient.FullName // Giả sử Patient có thuộc tính Name
                });
            }
            }
            var info = account.PatientInformation;
            var PatientInfo = new PatientInfoResponse();
            if (info.Count > 0)
            {
            PatientInfo = new PatientInfoResponse
            {
                DateOfBirth = info.First().DateOfBirth,
                Gender = info.First().Gender,

            };
            }
            var AccountModel = new AccountModelResponse
            {
                Id = account.Id,
                FullName = account.FullName,
                Email = account.Email,
                PhoneNumber = account.PhoneNumber,
                FCMToken = account.FCMToken,
                Role = Role,
                PatientInfo = PatientInfo,
                WatcherResponses = WatcherResponses
            };
            return AccountModel;
        }

        public Task CreateWatcher(CreateWatcher model)
        {
            var result = new Watcher
            {
                PatientId = model.PatientId,
                RelativeId = model.RelativeId,
                CreatedTime = DateTimeOffset.Now,
                LastUpdatedTime = DateTimeOffset.Now,
            };
            _unitOfWork.GetRepository<Watcher>().Insert(result);
            _unitOfWork.Save();
            return Task.CompletedTask;
        }
    }
}
