using BlackJack.Server.Services;
using BlackJack.Shared.Models;
using System.Linq;
using Xunit;

namespace BlackJack.Tests
{
    public class GameServiceTests
    {
        [Fact]
        public void CreateGame_ShouldCreateNewGame()
        {
            var service = new GameService();
            var state = service.CreateGame("Player1", "conn1", "MyRoom");

            Assert.NotNull(state);
            Assert.NotNull(state.GameId);
            Assert.Equal("MyRoom", state.RoomName);
            Assert.Single(state.Players);
            Assert.Equal("Player1", state.Players[0].Name);
            Assert.Equal(GameStatus.WaitingForPlayers, state.Status);
        }

        [Fact]
        public void JoinGame_ShouldAddPlayer()
        {
            var service = new GameService();
            var state = service.CreateGame("Player1", "conn1", "MyRoom");
            var joinedState = service.JoinGame(state.GameId, "Player2", "conn2");

            Assert.NotNull(joinedState);
            Assert.Equal(2, joinedState.Players.Count);
            Assert.Equal("Player2", joinedState.Players[1].Name);
        }

        [Fact]
        public void StartGame_ShouldDealCardsAndAddDealer()
        {
            var service = new GameService();
            var state = service.CreateGame("Player1", "conn1", "MyRoom");
            service.JoinGame(state.GameId, "Player2", "conn2");
            
            var startedState = service.StartGame(state.GameId);

            Assert.Equal(GameStatus.InProgress, startedState.Status);
            Assert.Equal(3, startedState.Players.Count); // 2 players + dealer
            Assert.Contains(startedState.Players, p => p.IsDealer);
            
            foreach (var player in startedState.Players)
            {
                Assert.Equal(2, player.Hand.Count);
                Assert.True(player.Score > 0);
            }

            var dealer = startedState.Players.First(p => p.IsDealer);
            Assert.True(dealer.Hand[1].IsHidden);
        }

        [Fact]
        public void CalculateScore_ShouldHandleAces()
        {
            var room = new GameRoom("test", "test");
            var player = new Player();
            
            // Ace + King = 21
            player.Hand.Add(new Card { Rank = Rank.Ace });
            player.Hand.Add(new Card { Rank = Rank.King });
            room.CalculateScore(player);
            Assert.Equal(21, player.Score);

            // Ace + Ace + Nine = 21 (11 + 1 + 9) -> Wait 11+1+9 = 21. Correct.
            player.Hand.Clear();
            player.Hand.Add(new Card { Rank = Rank.Ace });
            player.Hand.Add(new Card { Rank = Rank.Ace });
            player.Hand.Add(new Card { Rank = Rank.Nine });
            room.CalculateScore(player);
            Assert.Equal(21, player.Score);

            // Ace + Ace + Ace = 13 (11 + 1 + 1) or (1 + 1 + 1)? 
            // 11+1+1 = 13. Correct.
            player.Hand.Clear();
            player.Hand.Add(new Card { Rank = Rank.Ace });
            player.Hand.Add(new Card { Rank = Rank.Ace });
            player.Hand.Add(new Card { Rank = Rank.Ace });
            room.CalculateScore(player);
            Assert.Equal(13, player.Score);
            
             // Ace + Ace + Ace + King = 13 (1 + 1 + 1 + 10)
            player.Hand.Clear();
            player.Hand.Add(new Card { Rank = Rank.Ace });
            player.Hand.Add(new Card { Rank = Rank.Ace });
            player.Hand.Add(new Card { Rank = Rank.Ace });
            player.Hand.Add(new Card { Rank = Rank.King });
            room.CalculateScore(player);
            Assert.Equal(13, player.Score);
        }
    }
}
