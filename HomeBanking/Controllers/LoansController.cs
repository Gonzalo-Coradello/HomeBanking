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
                    return Forbid("Email vacío.");
                }

                Client client = _clientRepository.FindByEmail(email);

                if (client == null)
                {
                    return Forbid("No existe el cliente.");
                }

                var loan = _loanRepository.FindById(application.LoanId);
                if (loan == null)
                {
                    return Forbid("El préstamo no existe");
                }

                if (application.Amount <= 0)
                {
                    return Forbid("El monto debe ser mayor a cero.");
                }

                if (application.Amount > loan.MaxAmount)
                {
                    return Forbid("El monto seleccionado sobrepasa el monto máximo del préstamo.");
                }

                if (application.Payments == string.Empty || application.Payments == "0")
                {
                    return Forbid("Debe seleccionar la cantidad de cuotas.");
                }

                if (!loan.Payments.Split(",").Any(payment => payment == application.Payments))
                {
                    return Forbid("Seleccione un número de cuotas válido.");
                }

                var account = _accountRepository.FindByNumber(application.ToAccountNumber);
                if (account == null)
                {
                    return Forbid("No se encontró la cuenta.");
                }

                if (!client.Accounts.Any(acc => acc.Number == application.ToAccountNumber))
                {
                    return Forbid("La cuenta seleccionada no pertenece al cliente actual.");
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
