using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace Spring2026_Project3_Jemorgan3.Models
{
    public class Movie
    {
        public int Id { get; set; }

        [Required]
        public string Title { get; set; }

        [Required]
        public string Genre { get; set; }

        [Required]
        public int Year { get; set; }

        [Required]
        [Url]
        public string IMDbUrl { get; set; }

        // Added '?' to make it nullable so validation doesn't fail before the file is processed
        public byte[]? Poster { get; set; }

        [ValidateNever]
        public List<ActorMovie>? ActorMovies { get; set; }
    }
}