﻿using HomeBanking.DTO;
using HomeBanking.Models;
using HomeBanking.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace HomeBanking.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClientsController : ControllerBase
    {
        private readonly IClientRepository _clientRepository;
        private readonly IAccountRepository _accountRepository;

        public ClientsController(IClientRepository clientRepository, IAccountRepository accountRepository)
        {
            _clientRepository = clientRepository;
            _accountRepository = accountRepository;
        }

        [HttpGet]
        public IActionResult Get()
        {
            try
            {
                var clients = _clientRepository.GetAllClients();

                var clientsDTO = new List<ClientDTO>();

                foreach (Client client in clients)
                {
                    var newClientDTO = new ClientDTO
                    {
                        Id = client.Id,
                        Email = client.Email,
                        FirstName = client.FirstName,
                        LastName = client.LastName,
                        Accounts = client.Accounts.Select(ac => new AccountDTO
                        {
                            Id = ac.Id,
                            Balance = ac.Balance,
                            CreationDate = ac.CreationDate,
                            Number = ac.Number
                        }).ToList(),
                        Credits = client.ClientLoans.Select(cl => new ClientLoanDTO
                        {
                            Id=cl.Id,
                            LoanId = cl.LoanId,
                            Name = cl.Loan.Name,
                            Amount = cl.Amount,
                            Payments = int.Parse(cl.Payments)
                        }).ToList(),
                        Cards = client.Cards.Select(c => new CardDTO
                        {
                            Id = c.Id,
                            CardHolder = c.CardHolder,
                            Color = c.Color,
                            Cvv = c.Cvv,
                            Number = c.Number,
                            FromDate = c.FromDate,
                            ThruDate = c.ThruDate,
                            Type = c.Type,
                        }).ToList()
                    };
                    clientsDTO.Add(newClientDTO);
                }

                return Ok(clientsDTO);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("{id}")]
        public IActionResult Get(long id) 
        { 
            try
            {
                var client = _clientRepository.FindById(id);
                
                if(client == null)
                {
                    return NotFound();
                }

                var clientDTO = new ClientDTO
                {
                    Id = client.Id,
                    Email = client.Email,
                    FirstName = client.FirstName,
                    LastName = client.LastName,
                    Accounts = client.Accounts.Select(ac => new AccountDTO
                    {
                        Id = ac.Id,
                        Balance = ac.Balance,
                        CreationDate = ac.CreationDate,
                        Number = ac.Number
                    }).ToList(),
                    Credits = client.ClientLoans.Select(cl => new ClientLoanDTO
                    {
                        Id = cl.Id,
                        LoanId = cl.LoanId,
                        Name = cl.Loan.Name,
                        Amount = cl.Amount,
                        Payments = int.Parse(cl.Payments)
                    }).ToList(),
                    Cards = client.Cards.Select(c => new CardDTO
                    {
                        Id = c.Id,
                        CardHolder = c.CardHolder,
                        Color = c.Color,
                        Number = c.Number,
                        Cvv = c.Cvv,
                        FromDate = c.FromDate,
                        ThruDate = c.ThruDate,
                        Type = c.Type,
                    }).ToList()
                };

                return Ok(clientDTO);
            }
            catch(Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        
        }

        [HttpGet("current")]
        public IActionResult GetCurrent()
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

                var clientDTO = new ClientDTO
                {
                    Id = client.Id,
                    Email = client.Email,
                    FirstName = client.FirstName,
                    LastName = client.LastName,
                    Accounts = client.Accounts.Select(ac => new AccountDTO
                    {
                        Id = ac.Id,
                        Balance = ac.Balance,
                        CreationDate = ac.CreationDate,
                        Number = ac.Number,
                    }).ToList(),
                    Credits = client.ClientLoans.Select(cl => new ClientLoanDTO
                    {
                        Id = cl.Id,
                        LoanId = cl.LoanId,
                        Name = cl.Loan.Name,
                        Amount = cl.Amount,
                        Payments = int.Parse(cl.Payments)
                    }).ToList(),
                    Cards = client.Cards.Select(c => new CardDTO
                    {
                        Id = c.Id,
                        CardHolder = c.CardHolder,
                        Color = c.Color,
                        Cvv = c.Cvv,
                        FromDate = c.FromDate,
                        Number = c.Number,
                        ThruDate = c.ThruDate,
                        Type = c.Type
                    }).ToList()
                };

                return Ok(clientDTO);

            } catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost]
        public IActionResult Post([FromBody] Client client)
        {
            string namePattern = @"^[a-zA-Z\s]{3,}$";
            string emailPattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
            string passwordPattern = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)[a-zA-Z\d]{8,}$";

            try
            {
                if (String.IsNullOrEmpty(client.Email) ||
                    String.IsNullOrEmpty(client.Password) ||
                    String.IsNullOrEmpty(client.FirstName) ||
                    String.IsNullOrEmpty(client.LastName))
                    return StatusCode(403, "Fields cannot be empty");

                if(!Regex.IsMatch(client.FirstName, namePattern, RegexOptions.IgnoreCase))
                {
                    return StatusCode(403, "Invalid first name format");
                }

                if (!Regex.IsMatch(client.LastName, namePattern, RegexOptions.IgnoreCase))
                {
                    return StatusCode(403, "Invalid last name format");
                }

                if (!Regex.IsMatch(client.Email, emailPattern, RegexOptions.IgnoreCase))
                {
                    return StatusCode(403, "Invalid email");
                }

                if (!Regex.IsMatch(client.Password, passwordPattern))
                {
                    return StatusCode(403, "Invalid password");
                }

                Client user = _clientRepository.FindByEmail(client.Email);

                if (user != null)
                {
                    return StatusCode(403, "Email is already in use");
                }

                Client newClient = new Client
                {
                    Email = client.Email,
                    Password = client.Password,
                    FirstName = client.FirstName,
                    LastName = client.LastName,
                };

                _clientRepository.Save(newClient);

                var createdClient = _clientRepository.FindByEmail(newClient.Email);

                Random random = new();

                var account = new Account
                {
                    Number = "VIN-" + random.Next(0, 99999999).ToString(),
                    Balance = 0,
                    ClientId = createdClient.Id,
                    CreationDate = DateTime.Now,
                };

                _accountRepository.Save(account);

                return Created("", newClient);

            } catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}
