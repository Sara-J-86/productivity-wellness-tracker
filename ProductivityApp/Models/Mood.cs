using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ProductivityApp.Models
{
    public class Mood
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        [Required]
        [Range(1, 5)]
        public int MoodValue { get; set; }

        [DataType(DataType.Date)]
        public DateTime MoodDate { get; set; }

        // ✅ Static helper to get mood description
        public static string GetMoodDescription(int value)
        {
            switch (value)
            {
                case 1: return "😞 Very Bad";
                case 2: return "😕 Bad";
                case 3: return "😐 Neutral";
                case 4: return "😊 Good";
                case 5: return "😄 Very Good";
                default: return "Unknown";
            }
        }
    }
}
