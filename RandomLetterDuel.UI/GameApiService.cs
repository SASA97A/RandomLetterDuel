using RandomLetterDuel.DTO;

namespace RandomLetterDuel.UI
{
    public class GameApiService
    {
        private readonly HttpClient _http;

        public GameApiService(HttpClient http)
        {
            _http = http;
        }

        // Skapa spel
        public async Task<GameRoomResponseDto?> CreateGame(CreateGameRequestDto dto)
        {
            var res = await _http.PostAsJsonAsync("api/game/create", dto);
            return await HandleResponse<GameRoomResponseDto>(res);
        }

        // Gå med i spel
        public async Task<GameRoomResponseDto?> JoinGame(JoinGameRequestDto dto)
        {
            var res = await _http.PostAsJsonAsync("api/game/join", dto);
            return await HandleResponse<GameRoomResponseDto>(res);
        }

        // Skicka ord
        public async Task<GameRoomResponseDto?> SubmitWord(MakeMoveRequestDto dto)
        {
            var res = await _http.PostAsJsonAsync("api/game/submit-word", dto);
            return await HandleResponse<GameRoomResponseDto>(res);
        }

        // Hämta status 
        public async Task<GameRoomResponseDto?> GetGameState(Guid gameId, CancellationToken token = default)
        {         
            try
            {
                // Skicka med token i anropet
                return await _http.GetFromJsonAsync<GameRoomResponseDto>($"api/game/{gameId}", token);
            }
            catch (OperationCanceledException)
            {
                // Om vi avbryter medvetet, returnera bara null istället för att krascha
                return null;
            }
        }

        // Hjälpmetod för att slippa skriva samma felhantering överallt
        private async Task<T?> HandleResponse<T>(HttpResponseMessage res)
        {
            if (res.IsSuccessStatusCode)
                return await res.Content.ReadFromJsonAsync<T>();

            var body = await res.Content.ReadAsStringAsync();
            var errorMessage = string.IsNullOrWhiteSpace(body) ? "Ett oväntat fel uppstod." : body.Trim().Trim('"');
            throw new HttpRequestException(errorMessage);
        }
    }
}
