using Contract.Repositories.Entity;
using ModelViews.AuthModelViews;


namespace Contract.Services.Interface
{
    public interface IUserService
    {
        Task<Account> CreateAccountAsync(Account account);
        Task<Account> GetUserByEmail(string email);
    }
}
