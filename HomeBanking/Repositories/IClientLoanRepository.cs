using HomeBanking.Models;
using System.Collections.Generic;

namespace HomeBanking.Repositories
{
    public interface IClientLoanRepository
    {
        void Save(ClientLoan clientLoan);
    }
}
