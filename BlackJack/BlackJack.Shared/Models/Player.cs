using System.Collections.Generic;

namespace BlackJack.Shared.Models
{
    public class Player
    {
        public string ConnectionId { get; set; }
        public string Name { get; set; }
        public List<Card> Hand { get; set; } = new List<Card>();
        public int Score { get; set; }
        public bool IsDealer { get; set; }
        public bool IsBusted { get; set; }
        public bool IsStanding { get; set; }
    }
}
