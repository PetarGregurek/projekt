using System.Collections.Generic;

namespace BoardGameReviews.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string HashedPassword { get; set; }
        public string Country { get; set; }
        public int Age { get; set; }
    
        public List<Review> Reviews { get; set; }
    }
}
