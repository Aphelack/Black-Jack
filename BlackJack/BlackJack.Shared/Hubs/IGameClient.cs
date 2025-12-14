using BlackJack.Shared.Models;
using System.Threading.Tasks;

namespace BlackJack.Shared.Hubs
{
    public interface IGameClient
    {
        Task GameUpdated(GameState state);
        Task GameCreated(string gameId);
        Task PlayerJoined(string playerName);
        Task Error(string message);
    }
}
