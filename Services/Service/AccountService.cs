using Contract.Repositories.Entity;
using Contract.Repositories.Interface;
using Contract.Services.Interface;
using Core.Base;
using Core.Store;
using Core.Utils;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ModelViews.AccountModelViews;
using ModelViews.AccountModelViews.Request;
using ModelViews.AccountModelViews.Response;
using ModelViews.PatientInfoModelViews.Response;
using ModelViews.WatcherModel;
using Repositories.UOW;
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
                                    where userRole.UserId == account.Id && userRole.DeletedTime == null
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

        public async Task<BaseResponse<string>> UpdateAccount(int id, UpdateAccountModel model)
        {
            var account = await _unitOfWork.GetRepository<Account>()
                .Entities.Include(n => n.PatientInformation)
                .FirstOrDefaultAsync(a => a.Id == id && !a.DeletedTime.HasValue);
            if (account == null)
            {
                return new BaseResponse<string>(StatusCodeHelper.Notfound, "400", "Account not found.");
            }

            if (!string.IsNullOrEmpty(model.Role))
            {
                var role = await _unitOfWork.GetRepository<ApplicationRole>()
                    .Entities.FirstOrDefaultAsync(n => n.Name.Equals(model.Role));

                if (role == null)
                {
                    return new BaseResponse<string>(StatusCodeHelper.Notfound, "400", "Role does not exist.");
                }

                // Lấy role hiện tại của user (đang active)
                var currentUserRole = await _unitOfWork.GetRepository<ApplicationUserRoles>()
    .Entities.FirstOrDefaultAsync(ur => ur.UserId == account.Id && !ur.DeletedTime.HasValue);

                // Lấy role đã bị xóa (soft delete)
                var deletedUserRole = await _unitOfWork.GetRepository<ApplicationUserRoles>()
    .Entities.FirstOrDefaultAsync(ur => ur.UserId == account.Id && ur.DeletedTime.HasValue && ur.RoleId == role.Id);

                if (deletedUserRole != null)
                {
                    // Nếu cập nhật về role cũ đã bị xóa, khôi phục lại
                    deletedUserRole.DeletedTime = null;
                    deletedUserRole.DeletedBy = null;

                    if (currentUserRole != null)
                    {
                        // Xóa role hiện tại (soft delete)
                        currentUserRole.DeletedTime = DateTimeOffset.UtcNow;
                        currentUserRole.DeletedBy = "System";
                    }
                }
                else if (currentUserRole != null && currentUserRole.RoleId != role.Id)
                {
                    // Nếu cập nhật về role mới, xóa role hiện tại và thêm role mới
                    currentUserRole.DeletedTime = DateTimeOffset.UtcNow;
                    currentUserRole.DeletedBy = "System";

                    var newUserRole = new ApplicationUserRoles
                    {
                        UserId = account.Id,
                        RoleId = role.Id,
                        CreatedBy = "System",
                        CreatedTime = DateTimeOffset.UtcNow
                    };
                    await _unitOfWork.GetRepository<ApplicationUserRoles>().InsertAsync(newUserRole);
                }
            }

            if (!string.IsNullOrEmpty(model.FullName))
            {
                account.FullName = model.FullName;
            }

            if (!string.IsNullOrEmpty(model.PhoneNumber))
            {
                account.PhoneNumber = model.PhoneNumber;
            }

            if (!string.IsNullOrEmpty(model.FCMToken))
            {
                account.FCMToken = model.FCMToken;
            }

            if (model.PatientInfo != null)
            {
                var patientInfo = account.PatientInformation.FirstOrDefault();
                if (patientInfo != null)
                {
                    if (!string.IsNullOrEmpty(model.PatientInfo.Gender))
                    {
                        patientInfo.Gender = model.PatientInfo.Gender;
                    }

                    if (model.PatientInfo.DateOfBirth.HasValue)
                    {
                        patientInfo.DateOfBirth = model.PatientInfo.DateOfBirth.Value;
                    }
                }
            }

            account.LastUpdatedBy = "System";
            account.LastUpdatedTime = DateTimeOffset.UtcNow;

            try
            {
                //await _unitOfWork.GetRepository<Account>().UpdateAsync(account);
                await _unitOfWork.SaveAsync();
                return BaseResponse<string>.OkResponse("Account updated successfully.");
            }
            catch (Exception ex)
            {
                return new BaseResponse<string>(StatusCodeHelper.ServerError, StatusCodeHelper.ServerError.Name(), $"Internal server error: {ex.Message}");
            }
        }
        public async Task<BaseResponse<string>> AddPatientInfoAsync(int id, AddPatientInfoModel model)
        {
            var account = await _unitOfWork.GetRepository<Account>().Entities.FirstOrDefaultAsync(a => a.Id == id);

            if (account == null)
            {
                return new BaseResponse<string>(StatusCodeHelper.Notfound, "404", "Account not found.");
            }
            if (account.PatientInformation == null)
            {
                account.PatientInformation = new List<PatientInformation>();
            }
            var patientInfo = new PatientInformation
            {
                Gender = model.Gender,
                DateOfBirth = model.DateOfBirth,                
            };

            account.PatientInformation.Add(patientInfo);

            if (!string.IsNullOrEmpty(model.FCMToken))
            {
                account.FCMToken = model.FCMToken;
            }

            try
            {
                await _unitOfWork.SaveAsync();
                return BaseResponse<string>.OkResponse("Patient information added successfully.");
            }
            catch (Exception ex)
            {
                return new BaseResponse<string>(StatusCodeHelper.ServerError, "500", $"Internal server error: {ex.Message}");
            }
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
                    where userRole.UserId == account.Id && userRole.DeletedTime == null

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
