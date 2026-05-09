using Microsoft.AspNetCore.Mvc;
using RandomLetterDuel.BLL;
using RandomLetterDuel.DTO;

namespace RandomLetterDuel.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GameController : ControllerBase
    {
        private readonly GameService _gameService;

        public GameController(GameService gameService)
        {
            _gameService = gameService;
        }

        [HttpPost("create")]
        public async Task<ActionResult<GameRoomResponseDto>> CreateGame([FromBody] CreateGameRequestDto request)
        {
            try
            {
                var result = await _gameService.CreateGame(request);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("join")]
        public async Task<ActionResult<GameRoomResponseDto>> JoinGame([FromBody] JoinGameRequestDto request)
        {
            var result = await _gameService.JoinGame(request);

            if (result == null)
            {
                return NotFound("Kunde inte hitta rummet eller så är det redan fullt.");
            }

            return Ok(result);
        }

        [HttpPost("submit-word")]
        public async Task<ActionResult<GameRoomResponseDto>> SubmitWord([FromBody] MakeMoveRequestDto request)
        {
            try
            {
                var result = await _gameService.SubmitWordAsync(request);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                // Felaktiga ord, för korta ord, etc.
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                // Fel tur, spelet inte igång, etc.
                return Conflict(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Ett oväntat fel uppstod.");
            }
        }
    }
}