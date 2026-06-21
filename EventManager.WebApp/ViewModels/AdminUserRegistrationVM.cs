namespace EventManager.WebApp.ViewModels
{
    public class AdminUserRegistrationVM
    {
        public string Username { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string EventName { get; set; } = string.Empty;

        public string EventTypeName { get; set; } = string.Empty;

        public DateTime RegisteredAt { get; set; }

        public bool IsActive { get; set; }
    }
}