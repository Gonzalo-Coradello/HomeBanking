using HomeBanking.DTO;
using HomeBanking.Models;
using HomeBanking.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;

namespace HomeBanking.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TransactionsController : ControllerBase
    {
        private readonly IAccountRepository _accountRepository;
        private readonly IClientRepository _clientRepository;
        private ITransactionRepository _transactionRepository;

        public TransactionsController(IAccountRepository accountRepository, IClientRepository clientRepository, ITransactionRepository transactionRepository)
        {
            _accountRepository = accountRepository;
            _clientRepository = clientRepository;
            _transactionRepository = transactionRepository;
        }

        [HttpPost]
        [Authorize]
        public IActionResult Post([FromBody] TransferDTO transferDTO)
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

                if (transferDTO.FromAccountNumber == string.Empty || transferDTO.ToAccountNumber == string.Empty)
                {
                    return StatusCode(403, "One of the accounts was not provided");
                }

                if (transferDTO.FromAccountNumber == transferDTO.ToAccountNumber)
                {
                    return StatusCode(403, "You can't transfer to the same account");
                }

                if (transferDTO.Amount == 0 || transferDTO.Description == string.Empty)
                {
                    return StatusCode(403, "Amount or description was not provided");
                }

                Account fromAccount = _accountRepository.FindByNumber(transferDTO.FromAccountNumber);
                if (fromAccount == null)
                {
                    return StatusCode(403, "Origin account does not exist");
                }

                if (fromAccount.Balance < transferDTO.Amount)
                {
                    return StatusCode(403, "Insufficient funds");
                }

                Account toAccount = _accountRepository.FindByNumber(transferDTO.ToAccountNumber);
                if (toAccount == null)
                {
                    return StatusCode(403, "Destiny account does not exist");
                }

                _transactionRepository.Save(new Transaction
                {
                    Type = TransactionType.DEBIT.ToString(),
                    Amount = transferDTO.Amount * -1,
                    Description = transferDTO.Description + " " + toAccount.Number,
                    AccountId = fromAccount.Id,
                    Date = DateTime.Now,
                });

                _transactionRepository.Save(new Transaction
                {
                    Type = TransactionType.CREDIT.ToString(),
                    Amount = transferDTO.Amount,
                    Description = transferDTO.Description + " " + fromAccount.Number,
                    AccountId = toAccount.Id,
                    Date = DateTime.Now,
                });

                fromAccount.Balance = fromAccount.Balance - transferDTO.Amount;
                _accountRepository.Save(fromAccount);

                toAccount.Balance = toAccount.Balance + transferDTO.Amount;
                _accountRepository.Save(toAccount);

                return Created("", fromAccount);

            } catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

        }
    }
}
