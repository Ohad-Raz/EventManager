using EventManager.DAL.Models;
using EventManager.DAL.Repositories;
using EventManager.WebAPI.Dtos;
using EventManager.WebAPI.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventManager.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        // Access to appsettings.json, needed for JWT secure key
        private readonly IConfiguration _configuration;

        private readonly IUserRepository _userRepository;

        public UserController(IConfiguration configuration, IUserRepository userRepository)
        {
            _configuration = configuration;
            _userRepository = userRepository;
        }

        /// <summary>
        /// Registers a new user in the database.
        /// Password is never stored directly, only hash + salt.
        /// Default role is User (RoleId = 2).
        /// </summary>
        [HttpPost("[action]")]
        public ActionResult<UserDto> Register(UserDto userDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                // Remove accidental spaces around username
                string trimmedUsername = userDto.Username.Trim();

                // Prevent duplicate usernames
                if (_userRepository.UsernameExists(trimmedUsername))
                    return BadRequest($"Username {trimmedUsername} already exists");

                // Prevent duplicate e-mails
                if (_userRepository.EmailExists(userDto.Email))
                    return BadRequest($"Email {userDto.Email} already exists");

                // Create salt and hash for the entered password
                string b64salt = PasswordHashProvider.GetSalt();
                string b64hash = PasswordHashProvider.GetHash(userDto.Password, b64salt);

                // Create database User entity from DTO
                User user = new User
                {
                    Username = trimmedUsername,
                    PwdHash = b64hash,
                    PwdSalt = b64salt,
                    FirstName = userDto.FirstName,
                    LastName = userDto.LastName,
                    Email = userDto.Email,
                    Phone = userDto.Phone,
                    RoleId = 2 // default role = User
                };

                // Save new user to database
                _userRepository.AddUser(user);
                _userRepository.SaveChanges();

                // Return generated Id to the client
                userDto.Id = user.Id;

                // Never send password back in response
                userDto.Password = string.Empty;

                return Ok(userDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        /// <summary>
        /// Logs in an existing user.
        /// Checks username + password hash, then returns JWT token.
        /// </summary>
        [HttpPost("[action]")]
        public ActionResult Login(UserLoginDto userDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                string genericLoginFail = "Incorrect username or password";

                // Find the user by username and also load Role,
                // because we want to store role name inside JWT
                User? existingUser = _userRepository.GetUserWithRoleByUsername(userDto.Username);

                // User not found
                if (existingUser == null)
                    return BadRequest(genericLoginFail);

                // Hash entered password using the user's stored salt
                string b64hash = PasswordHashProvider.GetHash(userDto.Password, existingUser.PwdSalt);

                // Compare calculated hash with stored hash
                if (b64hash != existingUser.PwdHash)
                    return BadRequest(genericLoginFail);

                // Read secure key from configuration
                string? secureKey = _configuration["JWT:SecureKey"];

                if (string.IsNullOrEmpty(secureKey))
                    return StatusCode(500, "JWT secure key is missing.");

                // Read role name from related Role entity
                string roleName = existingUser.Role.Name;

                // Create token with username + role claim
                string serializedToken = JwtTokenProvider.CreateToken(
                    secureKey,
                    120,
                    existingUser.Username,
                    roleName
                );

                return Ok(serializedToken);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [Authorize]
        [HttpPost("[action]")]
        public ActionResult ChangePassword(ChangePasswordDto changePassDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                string genericLoginFail = "Incorrect username or password";

                // Get username from JWT
                string? username = HttpContext.User.Identity?.Name;

                // Find logged-in user
                User? existingUser = _userRepository.GetUserByUsername(username);

                if (existingUser == null)
                    return BadRequest(genericLoginFail);

                // Check new password rules
                if (!changePassDto.NewPasswordsMatch())
                    return BadRequest("Passwords do not match.");

                if (!changePassDto.IsDifferentFromCurrent())
                    return BadRequest("New password must be different from current password.");

                // Verify CURRENT password using existing salt
                string currentHash = PasswordHashProvider.GetHash(changePassDto.CurrentPassword, existingUser.PwdSalt);

                if (currentHash != existingUser.PwdHash)
                    return BadRequest(genericLoginFail);

                // Generate new salt and hash for NEW password
                string newSalt = PasswordHashProvider.GetSalt();
                string newHash = PasswordHashProvider.GetHash(changePassDto.NewPassword, newSalt);

                existingUser.PwdSalt = newSalt;
                existingUser.PwdHash = newHash;

                _userRepository.SaveChanges();

                return Ok("Password changed successfully");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("[action]")]
        public ActionResult PromoteUser(PromoteUserDto promoteUDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                // 1. find target user
                User? existingUser = _userRepository.GetUserById(promoteUDto.UserId);
                // 2. if not found, return NotFound
                // User not found
                if (existingUser == null)
                    return NotFound("User Not Found");
                // 3. if already admin, maybe return BadRequest or Ok
                if (existingUser.RoleId == 1)
                    return BadRequest("User is already an admin.");
                // 4. set RoleId = 1
                existingUser.RoleId = 1;//ref tracking 
                // 5. save changes
                _userRepository.SaveChanges();
                return Ok("User promoted successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}