using Microsoft.AspNetCore.Mvc.Rendering;

namespace Spring2026_Project3_Jemorgan3.Models
{
    public class MovieActorViewModel
    {
        // The Movie we are adding someone to
        public Movie? Movie { get; set; }

        // A list for the dropdown menu (SelectListItem)
        public SelectList? ActorList { get; set; }

        // The ID of the actor the user selects from the dropdown
        public int SelectedActorId { get; set; }
    }
}