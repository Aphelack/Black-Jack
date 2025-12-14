using BlackJack.Shared.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlackJack.Server.Services
{
    public class GameService
    {
        private ConcurrentDictionary<string, GameRoom> _games = new ConcurrentDictionary<string, GameRoom>();

        public GameState CreateGame(string playerName, string connectionId, string roomName)
        {
            var gameId = Guid.NewGuid().ToString();
            var gameRoom = new GameRoom(gameId, roomName);
            gameRoom.AddPlayer(playerName, connectionId);
            _games.TryAdd(gameId, gameRoom);
            return gameRoom.State;
        }

        public GameState JoinGame(string gameId, string playerName, string connectionId)
        {
            if (_games.TryGetValue(gameId, out var gameRoom))
            {
                gameRoom.AddPlayer(playerName, connectionId);
                return gameRoom.State;
            }
            return null;
        }

        public GameState LeaveGame(string gameId, string connectionId)
        {
            if (_games.TryGetValue(gameId, out var gameRoom))
            {
                gameRoom.RemovePlayer(connectionId);
                if (gameRoom.State.Players.Count == 0)
                {
                    _games.TryRemove(gameId, out _);
                    return null; // Game removed
                }
                return gameRoom.State;
            }
            return null;
        }

        public GameState RestartGame(string gameId)
        {
            if (_games.TryGetValue(gameId, out var gameRoom))
            {
                gameRoom.RestartGame();
                return gameRoom.State;
            }
            return null;
        }

        public GameState StartGame(string gameId)
        {
             if (_games.TryGetValue(gameId, out var gameRoom))
            {
                gameRoom.StartGame();
                return gameRoom.State;
            }
            return null;
        }

        public async Task<GameState> Hit(string gameId, string connectionId, Func<GameState, Task> onStateChanged)
        {
             if (_games.TryGetValue(gameId, out var gameRoom))
            {
                await gameRoom.Hit(connectionId, onStateChanged);
                return gameRoom.State;
            }
            return null;
        }

        public async Task<GameState> Stand(string gameId, string connectionId, Func<GameState, Task> onStateChanged)
        {
             if (_games.TryGetValue(gameId, out var gameRoom))
            {
                await gameRoom.Stand(connectionId, onStateChanged);
                return gameRoom.State;
            }
            return null;
        }
        
        public GameRoom GetGame(string gameId)
        {
            _games.TryGetValue(gameId, out var game);
            return game;
        }
    }

    public class GameRoom
    {
        public GameState State { get; private set; }
        private Deck _deck;

        public GameRoom(string gameId, string roomName)
        {
            State = new GameState { GameId = gameId, RoomName = roomName };
            _deck = new Deck();
        }

        public void AddPlayer(string name, string connectionId)
        {
            if (State.Status == GameStatus.WaitingForPlayers)
            {
                State.Players.Add(new Player { Name = name, ConnectionId = connectionId });
            }
        }

        public void RemovePlayer(string connectionId)
        {
            var player = State.Players.FirstOrDefault(p => p.ConnectionId == connectionId);
            if (player != null)
            {
                State.Players.Remove(player);
                
                // If game is in progress and current player leaves, move to next turn
                if (State.Status == GameStatus.InProgress && State.CurrentTurnConnectionId == connectionId)
                {
                    // This is synchronous, so we can't await. 
                    // But RemovePlayer is called from LeaveGame which is synchronous in GameService (for now).
                    // We might need to make LeaveGame async too if we want to support dealer play on leave.
                    // For now, let's keep it simple and assume dealer play only happens on explicit Stand or Hit bust.
                    // Or we can just fire and forget NextTurn here if it's async, but that's risky.
                    // Let's leave this as is for now, as the user didn't ask to change Leave behavior.
                    // But wait, NextTurn will become async.
                    // We can just call NextTurn(null).
                    _ = NextTurn(null); 
                }
            }
        }

        public void RestartGame()
        {
            State.Status = GameStatus.WaitingForPlayers;
            State.WinnerMessage = null;
            State.CurrentTurnConnectionId = null;
            
            // Remove dealer
            var dealer = State.Players.FirstOrDefault(p => p.IsDealer);
            if (dealer != null)
            {
                State.Players.Remove(dealer);
            }

            // Reset players
            foreach (var player in State.Players)
            {
                player.Hand.Clear();
                player.Score = 0;
                player.IsBusted = false;
                player.IsStanding = false;
            }
        }

        public void StartGame()
        {
            if (State.Players.Count < 1) return; 
            
            var dealer = new Player { Name = "Dealer", IsDealer = true, ConnectionId = "Dealer" };
            State.Players.Add(dealer);

            State.Status = GameStatus.InProgress;
            _deck = new Deck(); 

            foreach (var player in State.Players)
            {
                player.Hand.Add(_deck.Draw());
                player.Hand.Add(_deck.Draw());
                CalculateScore(player);
            }

            var dealerPlayer = State.Players.First(p => p.IsDealer);
            if (dealerPlayer.Hand.Count > 1)
            {
                dealerPlayer.Hand[1].IsHidden = true;
            }

            State.CurrentTurnConnectionId = State.Players.First(p => !p.IsDealer).ConnectionId;
        }

        public async Task Hit(string connectionId, Func<GameState, Task> onStateChanged)
        {
            if (State.Status != GameStatus.InProgress) return;
            if (State.CurrentTurnConnectionId != connectionId) return;

            var player = State.Players.FirstOrDefault(p => p.ConnectionId == connectionId);
            if (player == null) return;

            player.Hand.Add(_deck.Draw());
            CalculateScore(player);

            if (player.Score > 21)
            {
                player.IsBusted = true;
                // Do NOT automatically go to next turn.
                // player.IsStanding = true; 
                // NextTurn();
            }
            
            if (onStateChanged != null) await onStateChanged(State);
        }

        public async Task Stand(string connectionId, Func<GameState, Task> onStateChanged)
        {
            if (State.Status != GameStatus.InProgress) return;
            if (State.CurrentTurnConnectionId != connectionId) return;

            var player = State.Players.FirstOrDefault(p => p.ConnectionId == connectionId);
            if (player == null) return;

            player.IsStanding = true;
            await NextTurn(onStateChanged);
        }

        private async Task NextTurn(Func<GameState, Task> onStateChanged)
        {
            var nextPlayer = State.Players.FirstOrDefault(p => !p.IsDealer && !p.IsStanding);
            if (nextPlayer != null)
            {
                State.CurrentTurnConnectionId = nextPlayer.ConnectionId;
                if (onStateChanged != null) await onStateChanged(State);
            }
            else
            {
                await PlayDealerTurn(onStateChanged);
            }
        }

        private async Task PlayDealerTurn(Func<GameState, Task> onStateChanged)
        {
            var dealer = State.Players.First(p => p.IsDealer);
            foreach (var card in dealer.Hand) card.IsHidden = false;
            
            State.CurrentTurnConnectionId = dealer.ConnectionId;
            if (onStateChanged != null) await onStateChanged(State);
            await Task.Delay(1000);

            while (dealer.Score < 17)
            {
                dealer.Hand.Add(_deck.Draw());
                CalculateScore(dealer);
                if (onStateChanged != null) await onStateChanged(State);
                await Task.Delay(1000);
            }

            if (dealer.Score > 21) dealer.IsBusted = true;
            dealer.IsStanding = true;

            EndGame();
            if (onStateChanged != null) await onStateChanged(State);
        }

        private void EndGame()
        {
            State.Status = GameStatus.Finished;
            State.CurrentTurnConnectionId = null;
            
            var dealer = State.Players.First(p => p.IsDealer);
            var winners = new List<string>();

            foreach (var player in State.Players.Where(p => !p.IsDealer))
            {
                if (player.IsBusted)
                {
                    // Loses
                }
                else if (dealer.IsBusted)
                {
                    winners.Add(player.Name);
                }
                else if (player.Score > dealer.Score)
                {
                    winners.Add(player.Name);
                }
                else if (player.Score == dealer.Score)
                {
                    winners.Add($"{player.Name} (Push)");
                }
            }

            if (winners.Count > 0)
            {
                State.WinnerMessage = "Winners: " + string.Join(", ", winners);
            }
            else
            {
                State.WinnerMessage = "Dealer Wins!";
            }
        }

        public void CalculateScore(Player player)
        {
            int score = 0;
            int aces = 0;

            foreach (var card in player.Hand)
            {
                score += card.Value;
                if (card.Rank == Rank.Ace) aces++;
            }

            while (score > 21 && aces > 0)
            {
                score -= 10;
                aces--;
            }

            player.Score = score;
        }
    }
}
