using Contract.Repositories.Entity;
using ModelViews.AuthModelViews;


namespace Contract.Services.Interface
{
    public interface IUserService
    {
        Task<Account> CreateAccountAsync(Account account);
        Task<Account> GetUserByEmail(string email);
        Task<bool> AddRoleToAccountAsync(int userId, string roleName);
        Task<bool> AddClaimToUserAsync(int userId, string claimType, string claimValue, string createdBy);
        Task<bool> AddClaimToRoleAsync(int roleId, string claimType, string claimValue, string createdBy);
        Task<IEnumerable<ApplicationUserClaims>> GetUserClaimsAsync(int userId);
        Task<bool> UpdateClaimAsync(int claimId, string claimType, string claimValue, string updatedBy);
        Task<bool> SoftDeleteClaimAsync(int claimId, string deletedBy);
    }
}
