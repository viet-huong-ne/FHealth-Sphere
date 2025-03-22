using AutoMapper;
using Contract.Repositories.Entity;
using ModelViews.AccountModelViews.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.Mapper
{
    public class AccountMapping : Profile
    {
        public AccountMapping() 
        { 
            CreateMap<AccountModelResponse, Account>();
        }
    }
}
