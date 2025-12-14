namespace BlackJack.Shared.Models
{
    public enum Suit { Hearts, Diamonds, Clubs, Spades }
    public enum Rank { Two, Three, Four, Five, Six, Seven, Eight, Nine, Ten, Jack, Queen, King, Ace }

    public class Card
    {
        public Suit Suit { get; set; }
        public Rank Rank { get; set; }
        public string ImagePath { get; set; }
        public bool IsHidden { get; set; }

        public int Value
        {
            get
            {
                if (Rank >= Rank.Two && Rank <= Rank.Nine)
                    return (int)Rank + 2;
                if (Rank >= Rank.Ten && Rank <= Rank.King)
                    return 10;
                if (Rank == Rank.Ace)
                    return 11; // Logic for 1 or 11 will be handled in score calculation
                return 0;
            }
        }
    }
}
