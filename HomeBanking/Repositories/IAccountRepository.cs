﻿using HomeBanking.Models;
using System.Collections.Generic;

namespace HomeBanking.Repositories
{
    public interface IAccountRepository
    {
        IEnumerable<Account> FindAllAccounts();  
        void Save(Account account);
        Account FindById(long id);
    }
}