using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace Spring2026_Project3_Jemorgan3.Models
{
    public class Actor
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string Gender { get; set; }

        [Required]
        public int Age { get; set; }

        [Required]
        [Url]
        public string IMDbUrl { get; set; }

        // Stored as byte[] per Project Requirements
        public byte[]? Photo { get; set; }

        // Navigation property for many-to-many relationship
        [ValidateNever]
        public ICollection<ActorMovie>? ActorMovies { get; set; } = new List<ActorMovie>();
    }
}