using System;
using System.Collections.Generic;
using System.Text;

namespace RandomLetterDuel.DTO
{
    public class PlayerDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Score { get; set; }
    }
}
