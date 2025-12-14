using BlackJack.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BlackJack.Server.Services
{
    public class Deck
    {
        private List<Card> _cards;
        private Random _random = new Random();

        public Deck()
        {
            _cards = new List<Card>();
            foreach (Suit suit in Enum.GetValues(typeof(Suit)))
            {
                foreach (Rank rank in Enum.GetValues(typeof(Rank)))
                {
                    _cards.Add(new Card
                    {
                        Suit = suit,
                        Rank = rank,
                        ImagePath = $"Cards/card{suit}{rank}.png"
                    });
                }
            }
            Shuffle();
        }

        public void Shuffle()
        {
            _cards = _cards.OrderBy(x => _random.Next()).ToList();
        }

        public Card Draw()
        {
            if (_cards.Count == 0) return null;
            var card = _cards[0];
            _cards.RemoveAt(0);
            return card;
        }
    }
}
