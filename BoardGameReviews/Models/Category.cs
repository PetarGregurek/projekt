using System.Collections.Generic;

namespace BoardGameReviews.Models
{
    public class Category
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string AgeGroup { get; set; }
        public Difficulty Difficulty { get; set; }
        public int Popularity { get; set; }
        
        public List<Game> Games { get; set; }
    }
}
