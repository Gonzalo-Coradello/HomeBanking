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
                    new Client { FirstName = "Victor", LastName = "Coronado", Email = "victor@gmail.com", Password = "123456" }
                };

                foreach(Client client in clients)
                {
                    context.Clients.Add(client);
                }

                context.SaveChanges();
            }

            if (!context.Accounts.Any())
            {
                var accountGonzalo = context.Clients.FirstOrDefault(c => c.Email == "gonzalocoradello@gmail.com");
                if (accountGonzalo != null)
                {
                    var accounts = new Account[]
                    {
                        new Account { ClientId = accountGonzalo.Id, CreationDate = DateTime.Now, Number = string.Empty, Balance = 0 }
                    };

                    foreach (Account account in accounts) 
                    {
                        context.Accounts.Add(account);
                    }
                    context.SaveChanges();
                 }
            }
        }
    }
}
