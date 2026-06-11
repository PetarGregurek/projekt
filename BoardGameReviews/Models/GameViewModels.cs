using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BoardGameReviews.Models
{
    public class GameIndexViewModel
    {
        public string? Search { get; set; }
        public string? StatusMessage { get; set; }
        public List<GameListItemViewModel> Games { get; set; } = new();
    }

    public class GameListItemViewModel
    {
        public required Game Game { get; set; }
        public string? GameTypeName { get; set; }
        public string? PublisherName { get; set; }
        public string? CategoryName { get; set; }
        public int ReviewCount { get; set; }
        public double? AverageRating { get; set; }
        public int EventCount { get; set; }
        public bool CanDelete { get; set; }
    }

    public class GameFormInputModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "The name is required.")]
        [StringLength(150)]
        public string Name { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Description { get; set; }

        [Range(1900, 2200)]
        [Display(Name = "Year Published")]
        public int YearPublished { get; set; }

        [Range(1, 100)]
        [Display(Name = "Minimum Players")]
        public int MinPlayers { get; set; }

        [Range(1, 100)]
        [Display(Name = "Maximum Players")]
        public int MaxPlayers { get; set; }

        public Difficulty Difficulty { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Select a game type.")]
        [Display(Name = "Game Type")]
        public int GameTypeId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Select a publisher.")]
        [Display(Name = "Publisher")]
        public int PublisherId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Select a category.")]
        [Display(Name = "Category")]
        public int CategoryId { get; set; }
    }

    public class GameFormViewModel
    {
        public GameFormInputModel Input { get; set; } = new();
        public List<SelectListItem> DifficultyOptions { get; set; } = new();
        public string? CategoryDisplayName { get; set; }
        public string? GameTypeDisplayName { get; set; }
        public string? PublisherDisplayName { get; set; }
        public string? StatusMessage { get; set; }
        public bool IsEdit => Input.Id > 0;
    }

    public class GameDeleteViewModel
    {
        public required Game Game { get; set; }
        public bool CanDelete { get; set; }
        public int ReviewCount { get; set; }
        public int EventCount { get; set; }
    }

    public class LookupItemViewModel
    {
        public int Id { get; set; }
        public string Label { get; set; } = string.Empty;
        public string? Description { get; set; }
    }

    public class AutocompleteDropdownViewModel
    {
        public string InputName { get; set; } = string.Empty;
        public string InputId { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public string LookupUrl { get; set; } = string.Empty;
        public string Placeholder { get; set; } = string.Empty;
        public int SelectedId { get; set; }
        public string? SelectedLabel { get; set; }
        public string? RequiredValidationMessage { get; set; }
        public string? ValidationMessage { get; set; }
    }

    public class ReviewListItemViewModel
    {
        public required Review Review { get; set; }
        public string? GameName { get; set; }
        public string? Username { get; set; }
    }

    public class EventListItemViewModel
    {
        public required Event Event { get; set; }
        public string? GameName { get; set; }
    }

    public class ReviewWithUserViewModel
    {
        public required Review Review { get; set; }
        public User? User { get; set; }
    }

    public class GameDetailsViewModel
    {
        public required Game Game { get; set; }
        public GameType? GameType { get; set; }
        public Publisher? Publisher { get; set; }
        public Category? Category { get; set; }
        public List<ReviewWithUserViewModel> Reviews { get; set; } = new();
        public List<Event> Events { get; set; } = new();
        public List<GameFile> Files { get; set; } = new();
        public double? AverageRating { get; set; }
    }

    public class CategoryDetailsViewModel
    {
        public required Category Category { get; set; }
        public List<Game> Games { get; set; } = new();
    }

    public class GameTypeDetailsViewModel
    {
        public required GameType GameType { get; set; }
        public List<Game> Games { get; set; } = new();
    }

    public class PublisherDetailsViewModel
    {
        public required Publisher Publisher { get; set; }
        public List<Game> Games { get; set; } = new();
    }

    public class UserDetailsViewModel
    {
        public required User User { get; set; }
        public List<ReviewListItemViewModel> Reviews { get; set; } = new();
    }

    public class ReviewDetailsViewModel
    {
        public required Review Review { get; set; }
        public Game? Game { get; set; }
        public User? User { get; set; }
    }

    public class EventDetailsViewModel
    {
        public required Event Event { get; set; }
        public Game? Game { get; set; }
    }

    // ── Category ──────────────────────────────────────────────────────────────────

    public class CategoryIndexViewModel
    {
        public string? Search { get; set; }
        public string? StatusMessage { get; set; }
        public List<Category> Categories { get; set; } = new();
    }

    public class CategoryFormInputModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "The name is required.")]
        [StringLength(120)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        [StringLength(20)]
        [RegularExpression(@"^\d{1,3}\+?$", ErrorMessage = "Age group format: '10+', '12', etc.")]
        public string? AgeGroup { get; set; }

        public Difficulty Difficulty { get; set; }

        [Range(0, 100, ErrorMessage = "Popularity must be between 0 and 100.")]
        public int Popularity { get; set; }
    }

    public class CategoryFormViewModel
    {
        public CategoryFormInputModel Input { get; set; } = new();
        public List<SelectListItem> DifficultyOptions { get; set; } = new();
        public string? StatusMessage { get; set; }
        public bool IsEdit => Input.Id > 0;
    }

    public class CategoryDeleteViewModel
    {
        public required Category Category { get; set; }
        public bool CanDelete { get; set; }
        public int GameCount { get; set; }
    }

    // ── GameType ──────────────────────────────────────────────────────────────────

    public class GameTypeIndexViewModel
    {
        public string? Search { get; set; }
        public string? StatusMessage { get; set; }
        public List<GameType> GameTypes { get; set; } = new();
    }

    public class GameTypeFormInputModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "The name is required.")]
        [StringLength(120)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }
    }

    public class GameTypeFormViewModel
    {
        public GameTypeFormInputModel Input { get; set; } = new();
        public string? StatusMessage { get; set; }
        public bool IsEdit => Input.Id > 0;
    }

    public class GameTypeDeleteViewModel
    {
        public required GameType GameType { get; set; }
        public bool CanDelete { get; set; }
        public int GameCount { get; set; }
    }

    // ── Publisher ─────────────────────────────────────────────────────────────────

    public class PublisherIndexViewModel
    {
        public string? Search { get; set; }
        public string? StatusMessage { get; set; }
        public List<Publisher> Publishers { get; set; } = new();
    }

    public class PublisherFormInputModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "The name is required.")]
        [StringLength(120)]
        public string Name { get; set; } = string.Empty;

        [StringLength(80)]
        [ValidCountry]
        public string? Country { get; set; }
    }

    public class PublisherFormViewModel
    {
        public PublisherFormInputModel Input { get; set; } = new();
        public string? StatusMessage { get; set; }
        public bool IsEdit => Input.Id > 0;
        public List<SelectListItem> CountryOptions => CountryHelper.ToSelectList();
    }

    public class PublisherDeleteViewModel
    {
        public required Publisher Publisher { get; set; }
        public bool CanDelete { get; set; }
        public int GameCount { get; set; }
    }

    // ── User ──────────────────────────────────────────────────────────────────────

    public class UserIndexViewModel
    {
        public string? Search { get; set; }
        public string? StatusMessage { get; set; }
        public List<User> Users { get; set; } = new();
    }

    public class UserFormInputModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "The username is required.")]
        [StringLength(80)]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "The email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email address.")]
        [StringLength(120)]
        public string Email { get; set; } = string.Empty;

        [StringLength(255)]
        public string? Password { get; set; }

        [StringLength(80)]
        [ValidCountry]
        public string? Country { get; set; }

        [Range(5, 120, ErrorMessage = "Age must be between 5 and 120.")]
        public int Age { get; set; }
    }

    public class UserFormViewModel
    {
        public UserFormInputModel Input { get; set; } = new();
        public string? StatusMessage { get; set; }
        public bool IsEdit => Input.Id > 0;
        public List<SelectListItem> CountryOptions => CountryHelper.ToSelectList();
    }

    public class UserDeleteViewModel
    {
        public required User User { get; set; }
        public bool CanDelete { get; set; }
        public int ReviewCount { get; set; }
    }

    // ── Event ─────────────────────────────────────────────────────────────────────

    public class EventIndexViewModel
    {
        public string? Search { get; set; }
        public string? StatusMessage { get; set; }
        public List<EventListItemViewModel> Events { get; set; } = new();
    }

    public class EventFormInputModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "The name is required.")]
        [StringLength(150)]
        public string Name { get; set; } = string.Empty;

        [Range(1, int.MaxValue, ErrorMessage = "Select a game.")]
        [Display(Name = "Game")]
        public int GameId { get; set; }

        [Required(ErrorMessage = "Start date/time is required.")]
        [Display(Name = "Start Date/Time")]
        public DateTime? StartDateTime { get; set; }

        [Required(ErrorMessage = "End date/time is required.")]
        [Display(Name = "End Date/Time")]
        public DateTime? EndDateTime { get; set; }

        [Required(ErrorMessage = "The location is required.")]
        [StringLength(200)]
        public string Location { get; set; } = string.Empty;
    }

    public class EventFormViewModel
    {
        public EventFormInputModel Input { get; set; } = new();
        public string? GameDisplayName { get; set; }
        public string? StatusMessage { get; set; }
        public bool IsEdit => Input.Id > 0;
    }

    public class EventDeleteViewModel
    {
        public required Event Event { get; set; }
        public bool CanDelete { get; set; } = true;
        public string? GameName { get; set; }
    }

    // ── Review ────────────────────────────────────────────────────────────────────

    public class ReviewIndexViewModel
    {
        public string? Search { get; set; }
        public string? StatusMessage { get; set; }
        public List<ReviewListItemViewModel> Reviews { get; set; } = new();
    }

    public class ReviewFormInputModel
    {
        public int Id { get; set; }

        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5.")]
        [Display(Name = "Rating")]
        public int Rating { get; set; } = 1;

        [Required(ErrorMessage = "The title is required.")]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [StringLength(2000)]
        public string? Comment { get; set; }

        [Display(Name = "Recommended")]
        public bool IsRecommended { get; set; }

        [Display(Name = "Created At")]
        public DateTime? CreatedAt { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Select a game.")]
        [Display(Name = "Game")]
        public int GameId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Select a user.")]
        [Display(Name = "User")]
        public int UserId { get; set; }
    }

    public class ReviewFormViewModel
    {
        public ReviewFormInputModel Input { get; set; } = new();
        public string? GameDisplayName { get; set; }
        public string? UserDisplayName { get; set; }
        public string? StatusMessage { get; set; }
        public bool IsEdit => Input.Id > 0;
    }

    public class ReviewDeleteViewModel
    {
        public required Review Review { get; set; }
        public bool CanDelete { get; set; } = true;
        public string? GameName { get; set; }
        public string? Username { get; set; }
    }
}