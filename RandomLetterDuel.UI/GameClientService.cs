using RandomLetterDuel.DTO;

namespace RandomLetterDuel.UI
{
    public class GameClientService
    {
        private readonly GameApiService _api;
        private readonly PollingService _polling;
        private CancellationTokenSource? _cts;

        // Här sparas läget centralt
        public GameRoomResponseDto? Game { get; private set; }
        public Guid? MyPlayerId { get; private set; }
        public string? ErrorMessage { get; private set; }

        // En händelse som meddelar UI när något har ändrats
        public event Action? OnChange;

        public GameClientService(GameApiService api, PollingService polling)
        {
            _api = api;
            _polling = polling;
        }

        private void NotifyStateChanged() => OnChange?.Invoke();

        public async Task CreateGame(string playerName)
        {
            try
            {
                ErrorMessage = null;
                var result = await _api.CreateGame(new CreateGameRequestDto { PlayerName = playerName });
                if (result != null)
                {
                    Game = result;
                    MyPlayerId = result.Players.First().Id;
                    StartStatusPolling();
                    NotifyStateChanged();
                }
            }
            catch (Exception ex) { ErrorMessage = ex.Message; NotifyStateChanged(); }
        }

        public async Task JoinGame(string playerName, string roomCode)
        {
            try
            {
                ErrorMessage = null;
                var result = await _api.JoinGame(new JoinGameRequestDto { PlayerName = playerName, RoomCode = roomCode });
                if (result != null)
                {
                    Game = result;
                    MyPlayerId = result.Players.First(p => p.Name == playerName).Id;
                    StartStatusPolling();
                    NotifyStateChanged();
                }
            }
            catch (Exception ex) { ErrorMessage = ex.Message; NotifyStateChanged(); }
        }

        public async Task Reconnect(Guid gameId, Guid myPlayerId)
        {
            MyPlayerId = myPlayerId;
            var result = await _api.GetGameState(gameId);
            if (result != null)
            {
                Game = result;
                StartStatusPolling();
                NotifyStateChanged();
            }
        }

        public async Task SubmitWord(string word)
        {
            if (Game == null || MyPlayerId == null) return;
            try
            {
                ErrorMessage = null;
                var result = await _api.SubmitWord(new MakeMoveRequestDto
                {
                    GameId = Game.Id,
                    PlayerId = MyPlayerId.Value,
                    Word = word
                });
                Game = result;
                NotifyStateChanged();
            }
            catch (Exception ex) { ErrorMessage = ex.Message; NotifyStateChanged(); }
        }

        private void StartStatusPolling()
        {
            _cts?.Cancel();
            _cts = new CancellationTokenSource();

            _ = _polling.StartPolling(async () =>
            {
                if (Game == null) return;

                try
                {
                    // Skickar med token härifrån också!
                    var updated = await _api.GetGameState(Game.Id, _cts.Token);

                    if (updated != null)
                    {
                        Game = updated;
                        NotifyStateChanged();
                    }
                }
                catch (Exception)
                {
                    // förösk varje 2 sekunder
                }
            }, 2000, _cts.Token);
        }

        public void StopPolling() => _cts?.Cancel();
    }
}
