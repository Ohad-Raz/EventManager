namespace EventManager.WebAPI.Dtos
{
    public class RegistrationDetailsDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public int EventId { get; set; }
        public string EventName { get; set; } = null!;
        public string Username { get; set; } = null!;
        public bool IsActive { get; set; }
        public DateTime RegisteredAt { get; set; }
    }
}