using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Contract.Repositories.Entity;
using Core.Base;
using ModelViews.AccountModelViews;
using ModelViews.AccountModelViews.Request;
using ModelViews.AccountModelViews.Response;

namespace Contract.Services.Interface
{
    public interface IAccountService
    {
        Task<Account> CreateAccount(CreateAccountModel model);
        Task CreateWatcher(CreateWatcher model);
        Task<BasePaginatedList<AccountModelResponse>> GetAllAccounts(int pageNumber, int pageSize);
        Task<Account> UpdateAccount(int id, UpdateAccountModel model);
        Task<bool> DeleteAccount(int id);
        Task<AccountModelResponse> GetAccountById(int Id);
    }
}
