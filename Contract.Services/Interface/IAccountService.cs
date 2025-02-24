using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Contract.Repositories.Entity;
using Core.Base;
using ModelViews.AccountModelViews;

namespace Contract.Services.Interface
{
    public interface IAccountService
    {
        Task<Account> CreateAccount(CreateAccountModel model);
        Task<BasePaginatedList<Account>> GetAllAccounts(int pageNumber, int pageSize);
        Task<Account> UpdateAccount(int id, UpdateAccountModel model);
        Task<bool> DeleteAccount(int id);
    }
}
