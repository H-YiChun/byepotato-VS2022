﻿namespace PotatoWebAPI.Models.DTO
{
    public class DailyHealthRecordDTO
    {
        public int? Water { get; set; }

        public int? Steps { get; set; }

        public string? Sleep { get; set; }

        public string? Mood { get; set; }

        public int? Vegetables { get; set; }

        public int? Snacks { get; set; }
    }
}