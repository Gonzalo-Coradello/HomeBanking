using System;
using System.Linq;

namespace HomeBanking.Models
{
    public static class DbInitializer
    {
        public static void Initialize(HomeBankingContext context)
        {
            if(!context.Clients.Any())
            {
                var clients = new Client[]
                {
                    new Client { FirstName = "Gonzalo", LastName = "Coradello", Email = "gonzalocoradello@gmail.com", Password = "123456" },
                    new Client { FirstName = "Eduardo", LastName = "Mendoza", Email = "eduardo@gmail.com", Password = "123456" },
                    new Client { FirstName = "Victor", LastName = "Coronado", Email = "vcoronado@gmail.com", Password = "123456" }
                };

                foreach(Client client in clients)
                {
                    context.Clients.Add(client);
                }

                context.SaveChanges();
            }

            if (!context.Accounts.Any())
            {
                var clientGonzalo = context.Clients.FirstOrDefault(c => c.Email == "gonzalocoradello@gmail.com");
                if (clientGonzalo != null)
                {
                    var accounts = new Account[]
                    {
                        new Account { ClientId = clientGonzalo.Id, CreationDate = DateTime.Now, Number = "VIN002", Balance = 0 }
                    };

                    foreach (Account account in accounts) 
                    {
                        context.Accounts.Add(account);
                    }
                    context.SaveChanges();
                 }

                var clientVictor = context.Clients.FirstOrDefault(c => c.Email == "vcoronado@gmail.com");
                if (clientVictor != null)
                {
                    var accounts = new Account[]
                    {
                        new Account { ClientId = clientVictor.Id, CreationDate = DateTime.Now, Number = "VIN001", Balance = 0 }
                    };

                    foreach (Account account in accounts)
                    {
                        context.Accounts.Add(account);
                    }
                    context.SaveChanges();
                }
            }

            if (!context.Transactions.Any())
            {
                var account1 = context.Accounts.FirstOrDefault(c => c.Number == "VIN001");
                if (account1 != null)
                {
                    var transactions = new Transaction[]
                    {
                        new Transaction { AccountId = account1.Id, Amount = 10000, Date = DateTime.Now.AddHours(-5), Description = "Transferencia recibida", Type = TransactionType.CREDIT.ToString() },
                        new Transaction { AccountId = account1.Id, Amount = -2000, Date = DateTime.Now.AddHours(-6), Description = "Compra en tienda Mercado Libre", Type = TransactionType.DEBIT.ToString() },
                        new Transaction { AccountId = account1.Id, Amount = -3000, Date = DateTime.Now.AddHours(-7), Description = "Compra en tienda xxxx", Type = TransactionType.DEBIT.ToString() },
                    };
                    foreach (Transaction transaction in transactions)
                    {
                        context.Transactions.Add(transaction);
                    }
                    context.SaveChanges();
                }
            }
        }
    }
}
