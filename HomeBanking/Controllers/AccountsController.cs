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
    [Route("api")]
    [ApiController]
    public class AccountsController : ControllerBase
    {
       private readonly IAccountRepository _accountRepository;
       private readonly IClientRepository _clientRepository;

        public AccountsController(IAccountRepository accountRepository, IClientRepository clientRepository)
        {
            _accountRepository = accountRepository;
            _clientRepository = clientRepository;
        }

        [HttpGet("accounts")]
        public IActionResult Get()
        {
            try
            {
                var accounts = _accountRepository.FindAllAccounts();
                var accountsDTO = new List<AccountDTO>();
                foreach (Account account in accounts)
                {
                    accountsDTO.Add(new AccountDTO
                    {
                        Id = account.Id,
                        Number = account.Number,
                        Balance = account.Balance,
                        CreationDate = account.CreationDate,
                        Transactions = account.Transactions.Select(t => new TransactionDTO
                        {
                            Id = t.Id,
                            Amount = t.Amount,
                            Date = t.Date,
                            Description = t.Description,
                            Type = t.Type,
                        }).ToList()
                    });
                }

                return Ok(accountsDTO);
            } catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("accounts/{id}")]
        public IActionResult Get(long id)
        {
            try
            {
                var account = _accountRepository.FindById(id);

                if (account == null)
                {
                    return NotFound();
                }

                var accountDTO = new AccountDTO
                {
                    Id = account.Id,
                    Number = account.Number,
                    Balance = account.Balance,
                    CreationDate = account.CreationDate,
                    Transactions = account.Transactions.Select(t => new TransactionDTO
                    {
                        Id = t.Id,
                        Amount = t.Amount,
                        Date = t.Date,
                        Description = t.Description,
                        Type = t.Type,
                    }).ToList()
                };

                return Ok(accountDTO);
            } catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost("clients/current/accounts")]
        public IActionResult Post()
        {
           try
            {
                string email = User.FindFirst("Client") != null ? User.FindFirst("Client").Value : String.Empty;

                if (email == string.Empty)
                {
                    return StatusCode(StatusCodes.Status403Forbidden);
                }

                Client client  = _clientRepository.FindByEmail(email);

                if (client == null)
                {
                    return StatusCode(StatusCodes.Status403Forbidden);
                }

                if (client.Accounts.Count == 3)
                {
                    return StatusCode(StatusCodes.Status403Forbidden);
                }

                Random random = new();

                var account = new Account
                {
                    Number = "VIN-" + random.Next(0, 99999999).ToString(),
                    Balance = 0,
                    ClientId = client.Id,
                    CreationDate = DateTime.Now,
                };

                _accountRepository.Save(account);

                return Created("", account);

            } catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("clients/current/accounts")]
        public IActionResult GetByClient()
        {
            try
            {
                string email = User.FindFirst("Client") != null ? User.FindFirst("Client").Value : String.Empty;

                if (email == string.Empty)
                {
                    return StatusCode(StatusCodes.Status403Forbidden);
                }

                Client client = _clientRepository.FindByEmail(email);

                if (client == null)
                {
                    return StatusCode(StatusCodes.Status403Forbidden);
                }

                var accounts = _accountRepository.GetAccountsByClient(client.Id);

                return Ok(accounts);
            } catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}
