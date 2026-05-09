using System;
using System.Collections.Generic;
using System.Text;

namespace RandomLetterDuel.DTO
{
    public class MakeMoveRequestDto
    {
        public Guid GameId { get; set; }
        public Guid PlayerId { get; set; }
        public string Word { get; set; } = string.Empty;
    }
}
