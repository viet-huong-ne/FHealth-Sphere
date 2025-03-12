using Azure.Core;
using Contract.Repositories.Entity;
using Contract.Repositories.Interface;
using Contract.Services.Interface;
using Core.Utils;
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

        public async Task<bool> AddRoleToAccountAsync(int userId, string roleName)
        {
            // Tìm tài khoản người dùng đã tồn tại
            var user = await _unitOfWork.GetRepository<Account>().GetByIdAsync(userId);

            if (user == null)
            {
                throw new Exception("User not found");
            }

            // Kiểm tra vai trò có tồn tại không
            var role = await _unitOfWork.GetRepository<ApplicationRole>().Entities.FirstOrDefaultAsync(r => r.Name == roleName);

            if (role == null)
            {
                throw new Exception("Role does not exist");
            }

            // Kiểm tra nếu người dùng đã có vai trò này
            var userRoleRepository = _unitOfWork.GetRepository<ApplicationUserRoles>();
            var existingUserRole = await userRoleRepository.Entities
                .AsNoTracking()  // Không theo dõi thực thể này
                .FirstOrDefaultAsync(ur => ur.UserId == userId && ur.RoleId == role.Id);

            if (existingUserRole != null)
            {
                throw new Exception("User already has this role");
            }

            // Nếu không tồn tại, thêm vai trò cho người dùng
            var applicationUserRole = new ApplicationUserRoles
            {
                UserId = user.Id,
                RoleId = role.Id,
                CreatedBy = user.UserName,  // Ghi lại ai đã thêm vai trò này
                CreatedTime = CoreHelper.SystemTimeNow,
                LastUpdatedBy = user.UserName,
                LastUpdatedTime = CoreHelper.SystemTimeNow
            };

            // Lưu thông tin vào ApplicationUserRoles
            await userRoleRepository.InsertAsync(applicationUserRole);
            await _unitOfWork.SaveAsync();

            return true;

        }

        public Task<bool> AddClaimToUserAsync(int userId, string claimType, string claimValue, string createdBy)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<ApplicationUserClaims>> GetUserClaimsAsync(int userId)
        {
            throw new NotImplementedException();
        }

        public Task<bool> UpdateClaimAsync(int claimId, string claimType, string claimValue, string updatedBy)
        {
            throw new NotImplementedException();
        }

        public Task<bool> SoftDeleteClaimAsync(int claimId, string deletedBy)
        {
            throw new NotImplementedException();
        }

        public Task<bool> AddClaimToRoleAsync(int roleId, string claimType, string claimValue, string createdBy)
        {
            throw new NotImplementedException();
        }
    }
}
