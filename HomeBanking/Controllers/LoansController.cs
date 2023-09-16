using HomeBanking.DTO;
using HomeBanking.Models;
using HomeBanking.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HomeBanking.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoansController : ControllerBase
    {
        private readonly IClientRepository _clientRepository;
        private readonly IAccountRepository _accountRepository;
        private readonly ILoanRepository _loanRepository;
        private readonly IClientLoanRepository _clientLoanRepository;
        private readonly ITransactionRepository _transactionRepository;

        public LoansController(IClientRepository clientRepository, IAccountRepository accountRepository, ILoanRepository loanRepository, IClientLoanRepository clientLoanRepository, ITransactionRepository transactionRepository)
        {
            _clientRepository = clientRepository;
            _accountRepository = accountRepository;
            _clientLoanRepository = clientLoanRepository;
            _loanRepository = loanRepository;
            _transactionRepository = transactionRepository;
        }

        [HttpGet]
        public IActionResult Get()
        {
            try
            {
                var loans = _loanRepository.GetAll();
                var loansDTO = new List<LoanDTO>();
                foreach (var loan in loans)
                {
                    loansDTO.Add(new LoanDTO
                    {
                        Id = loan.Id,
                        MaxAmount = loan.MaxAmount,
                        Name = loan.Name,
                        Payments = loan.Payments,
                    });
                }

                return Ok(loansDTO);
            } catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost]
        public IActionResult Post([FromBody] LoanApplicationDTO application)
        {
            try
            {
                string email = User.FindFirst("Client") != null ? User.FindFirst("Client").Value : String.Empty;

                if (email == string.Empty)
                {
                    return StatusCode(403, "Email is empty");
                }

                Client client = _clientRepository.FindByEmail(email);

                if (client == null)
                {
                    return StatusCode(403, "Client does not exist");
                }

                var loan = _loanRepository.FindById(application.LoanId);
                if (loan == null)
                {
                    return StatusCode(403, "Loan does not exist");
                }

                if (application.Amount <= 0)
                {
                    return StatusCode(403, "Amount should be greater than 0");
                }

                if (application.Amount > loan.MaxAmount)
                {
                    return StatusCode(403, "Selected amount exceeds loan's maximum amount");
                }

                if (application.Payments == string.Empty || application.Payments == "0")
                {
                    return StatusCode(403, "You must select the number of payments");
                }

                if (!loan.Payments.Split(",").Any(payment => payment == application.Payments))
                {
                    return StatusCode(403, "Select a valid number of payments");
                }

                var account = _accountRepository.FindByNumber(application.ToAccountNumber);
                if (account == null)
                {
                    return StatusCode(403, "Account was not found");
                }

                if (!client.Accounts.Any(acc => acc.Number == application.ToAccountNumber))
                {
                    return StatusCode(403, "Selected account does not belong to the current user");
                }

                var clientLoan = new ClientLoan
                {
                    LoanId = application.LoanId,
                    Payments = application.Payments,
                    Amount = application.Amount + application.Amount * 0.2,
                    ClientId = client.Id,
                };

                _clientLoanRepository.Save(clientLoan);

                var transaction = new Transaction
                {
                    Amount = application.Amount,
                    Type = TransactionType.CREDIT.ToString(),
                    AccountId = account.Id,
                    Date = DateTime.Now,
                    Description = loan.Name + " - " + "loan approved"
                };

                _transactionRepository.Save(transaction);

                account.Balance += application.Amount;
                _accountRepository.Save(account);

                return Created("", clientLoan);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}
