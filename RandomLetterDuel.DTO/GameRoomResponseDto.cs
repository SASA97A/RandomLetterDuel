namespace RandomLetterDuel.DTO
{
    public class GameRoomResponseDto
    {
        public Guid Id { get; set; }
        public string RoomCode { get; set; } = string.Empty;
        public GameState State { get; set; } = GameState.WaitingForPlayers;

        //Bokstaven som nästa ord måste börja på
        public char? RequiredLetter { get; set; }
        public List<PlayerDto> Players { get; set; } = new();
        public List<string> UsedWords { get; set; } = new();
        public Guid CurrentTurnPlayerId { get; set; }
        public Guid? WinnerId { get; set; }
    }
}
