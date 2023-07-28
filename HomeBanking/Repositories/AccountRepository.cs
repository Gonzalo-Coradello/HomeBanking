using HomeBanking.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace HomeBanking.Repositories
{
    public class AccountRepository : RepositoryBase<Account>, IAccountRepository
    {
        public AccountRepository(HomeBankingContext repositoryContext) : base(repositoryContext) { }

        public IEnumerable<Account> FindAllAccounts()
        {
           return FindAll().Include(account =>  account.Transactions).ToList();
        }

        public Account FindById(long id)
        {
           return FindByCondition(account => account.Id == id).Include(account => account.Transactions).FirstOrDefault();
        }

        public void Save(Account account)
        {
            Create(account);
            SaveChanges();
        }
    }
}
