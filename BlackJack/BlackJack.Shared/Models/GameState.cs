using System.Collections.Generic;

namespace BlackJack.Shared.Models
{
    public enum GameStatus { WaitingForPlayers, InProgress, Finished }

    public class GameState
    {
        public string GameId { get; set; }
        public string RoomName { get; set; }
        public List<Player> Players { get; set; } = new List<Player>();
        public GameStatus Status { get; set; } = GameStatus.WaitingForPlayers;
        public string CurrentTurnConnectionId { get; set; }
        public string WinnerMessage { get; set; }
    }
}
