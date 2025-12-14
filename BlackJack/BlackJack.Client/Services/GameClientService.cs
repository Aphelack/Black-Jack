using BlackJack.Shared.Hubs;
using BlackJack.Shared.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Threading.Tasks;

namespace BlackJack.Client.Services
{
    public class GameClientService : IAsyncDisposable
    {
        private HubConnection _hubConnection;
        private readonly NavigationManager _navigationManager;

        public event Action<GameState> OnGameUpdated;
        public event Action<string> OnGameCreated;
        public event Action<string> OnError;

        public string ConnectionId => _hubConnection?.ConnectionId;

        public GameClientService(NavigationManager navigationManager)
        {
            _navigationManager = navigationManager;
        }

        public async Task InitializeAsync()
        {
            _hubConnection = new HubConnectionBuilder()
                .WithUrl(_navigationManager.ToAbsoluteUri("/gamehub"))
                .Build();

            _hubConnection.On<GameState>(nameof(IGameClient.GameUpdated), (state) =>
            {
                OnGameUpdated?.Invoke(state);
            });

            _hubConnection.On<string>(nameof(IGameClient.GameCreated), (gameId) =>
            {
                OnGameCreated?.Invoke(gameId);
            });

            _hubConnection.On<string>(nameof(IGameClient.Error), (message) =>
            {
                OnError?.Invoke(message);
            });

            await _hubConnection.StartAsync();
        }

        public async Task CreateGame(string playerName, string roomName)
        {
            await _hubConnection.SendAsync("CreateGame", playerName, roomName);
        }

        public async Task JoinGame(string gameId, string playerName)
        {
            await _hubConnection.SendAsync("JoinGame", gameId, playerName);
        }

        public async Task LeaveGame(string gameId)
        {
            await _hubConnection.SendAsync("LeaveGame", gameId);
        }

        public async Task RestartGame(string gameId)
        {
            await _hubConnection.SendAsync("RestartGame", gameId);
        }

        public async Task StartGame(string gameId)
        {
            await _hubConnection.SendAsync("StartGame", gameId);
        }

        public async Task Hit(string gameId)
        {
            await _hubConnection.SendAsync("Hit", gameId);
        }

        public async Task Stand(string gameId)
        {
            await _hubConnection.SendAsync("Stand", gameId);
        }

        public async ValueTask DisposeAsync()
        {
            if (_hubConnection is not null)
            {
                await _hubConnection.DisposeAsync();
            }
        }
    }
}
