using System.ComponentModel.DataAnnotations;

namespace EventManager.WebAPI.Dtos
{
    public class PromoteUserDto
    {
        [Required(ErrorMessage = "User Id is required!")]
        public int UserId { get; set; }
    }
}
