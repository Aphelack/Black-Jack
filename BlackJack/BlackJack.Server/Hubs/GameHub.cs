using BlackJack.Server.Services;
using BlackJack.Shared.Hubs;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace BlackJack.Server.Hubs
{
    public class GameHub : Hub<IGameClient>
    {
        private readonly GameService _gameService;

        public GameHub(GameService gameService)
        {
            _gameService = gameService;
        }

        public async Task CreateGame(string playerName, string roomName)
        {
            var state = _gameService.CreateGame(playerName, Context.ConnectionId, roomName);
            await Groups.AddToGroupAsync(Context.ConnectionId, state.GameId);
            await Clients.Caller.GameCreated(state.GameId);
            await Clients.Group(state.GameId).GameUpdated(state);
        }

        public async Task JoinGame(string gameId, string playerName)
        {
            var state = _gameService.JoinGame(gameId, playerName, Context.ConnectionId);
            if (state != null)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, gameId);
                await Clients.Group(gameId).GameUpdated(state);
            }
            else
            {
                await Clients.Caller.Error("Game not found");
            }
        }

        public async Task LeaveGame(string gameId)
        {
            var state = _gameService.LeaveGame(gameId, Context.ConnectionId);
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, gameId);
            if (state != null)
            {
                await Clients.Group(gameId).GameUpdated(state);
            }
        }

        public async Task RestartGame(string gameId)
        {
            var state = _gameService.RestartGame(gameId);
            if (state != null)
            {
                await Clients.Group(gameId).GameUpdated(state);
            }
        }

        public async Task StartGame(string gameId)
        {
            var state = _gameService.StartGame(gameId);
            if (state != null)
            {
                await Clients.Group(gameId).GameUpdated(state);
            }
        }

        public async Task Hit(string gameId)
        {
            await _gameService.Hit(gameId, Context.ConnectionId, async (state) => 
            {
                await Clients.Group(gameId).GameUpdated(state);
            });
        }

        public async Task Stand(string gameId)
        {
            await _gameService.Stand(gameId, Context.ConnectionId, async (state) => 
            {
                await Clients.Group(gameId).GameUpdated(state);
            });
        }
    }
}
