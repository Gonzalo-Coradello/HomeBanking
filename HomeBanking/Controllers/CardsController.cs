using HomeBanking.Models;
using HomeBanking.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;

namespace HomeBanking.Controllers
{
    [Route("api")]
    [ApiController]
    public class CardsController : ControllerBase
    {
        private readonly ICardRepository _cardRepository;
        private readonly IClientRepository _clientRepository;

        public CardsController(ICardRepository cardRepository, IClientRepository clientRepository)
        {
            _cardRepository = cardRepository;
            _clientRepository = clientRepository;
        }

        [HttpPost("clients/current/cards")]
        public IActionResult Post([FromBody] Card card)
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

                if (card.Type != CardType.DEBIT.ToString() && card.Type != CardType.CREDIT.ToString())
                {
                    return StatusCode(403, "Invalid card type");
                }

                if (card.Color != CardColor.SILVER.ToString() && card.Color != CardColor.GOLD.ToString() && card.Color != CardColor.TITANIUM.ToString())
                {
                    return StatusCode(403, "Invalid card color");
                }

                if (client.Cards.Where(c => c.Type == card.Type).Count() >= 3)
                {
                    return StatusCode(403, "You can't have more than three credit or debit cards");
                }

                Random random = new();
                int next()
                {
                    return random.Next(1000, 10000);
                }

                var newCard = new Card
                {
                    ClientId = client.Id,
                    CardHolder = client.FirstName + " " + client.LastName,
                    Color = card.Color,
                    Type = card.Type,
                    Number = $"{next()}-{next()}-{next()}-{next()}",
                    Cvv = random.Next(100, 1000),
                    FromDate = DateTime.Now,
                    ThruDate = DateTime.Now.AddYears(4),
                };

                _cardRepository.Save(newCard);

                return Ok(newCard);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
        [HttpGet("clients/current/cards")]
        public IActionResult Get()
        {
            try
            {
                string email = User.FindFirst("Client") != null ? User.FindFirst("Client").Value : string.Empty;

                if (email == string.Empty)
                {
                    return StatusCode(StatusCodes.Status403Forbidden);
                }

                Client client = _clientRepository.FindByEmail(email);

                if (client == null)
                {
                    return StatusCode(StatusCodes.Status403Forbidden);
                }

                var cards = _cardRepository.GetCardsByClient(client.Id);

                return Ok(cards);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}
