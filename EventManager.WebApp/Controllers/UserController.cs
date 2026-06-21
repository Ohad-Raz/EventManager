using AutoMapper;
using EventManager.DAL.Models;
using EventManager.DAL.Repositories;
using EventManager.WebAPI.Security;
using EventManager.WebApp.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EventManager.WebApp.Controllers
{
    public class UserController : Controller
    {
        private readonly IUserRepository _userRepository;
        private readonly IRegistrationRepository _registrationRepository;
        private readonly IMapper _mapper;

        // Repository for user database operations, injected by DI.
        public UserController(
            IUserRepository userRepository,
            IRegistrationRepository registrationRepository,
            IMapper mapper)
        {
            _userRepository = userRepository;
            _registrationRepository = registrationRepository;
            _mapper = mapper;
        }

        // GET: User/Register
        public IActionResult Register()
        {
            return View();
        }

        // POST: User/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Register(RegisterVM model)
        {
            try
            {
                // 1. validate form input
                if (!ModelState.IsValid)
                    return View(model);

                // 2. remove accidental spaces around username
                string trimmedUsername = model.Username.Trim();

                // 3. prevent duplicate usernames
                if (_userRepository.UsernameExists(trimmedUsername))
                {
                    ModelState.AddModelError(nameof(model.Username), $"Username {trimmedUsername} already exists.");
                    return View(model);
                }

                // 4. prevent duplicate emails
                if (_userRepository.EmailExists(model.Email))
                {
                    ModelState.AddModelError(nameof(model.Email), $"Email {model.Email} already exists.");
                    return View(model);
                }

                // 5. create salt and hash for the entered password
                string b64salt = PasswordHashProvider.GetSalt();
                string b64hash = PasswordHashProvider.GetHash(model.Password, b64salt);

                // 6. create database User entity from ViewModel
                User user = new User
                {
                    Username = trimmedUsername,
                    PwdHash = b64hash,
                    PwdSalt = b64salt,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Email = model.Email,
                    Phone = model.Phone,
                    RoleId = 2 // default role = User
                };

                // 7. save new user to database
                _userRepository.AddUser(user);
                _userRepository.SaveChanges();
                TempData["RegisterMessage"] = "Registration successful. Please log in.";

                return RedirectToAction(nameof(Login));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(model);
            }
        }

        // GET: User/Login
        public IActionResult Login(string? returnUrl)
        {
            // 1. preserve return URL so login can redirect back after success
            LoginVM model = new LoginVM
            {
                ReturnUrl = returnUrl
            };

            return View(model);
        }

        // POST: User/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginVM model)
        {
            try
            {
                // 1. validate form input
                if (!ModelState.IsValid)
                    return View(model);

                const string genericLoginFail = "Incorrect username or password";

                // 2. find user by username and also load Role for cookie role claim
                User? existingUser = _userRepository.GetUserWithRoleByUsername(model.Username);

                if (existingUser == null)
                {
                    ModelState.AddModelError("", genericLoginFail);
                    return View(model);
                }

                // 3. hash entered password using the user's stored salt
                string b64hash = PasswordHashProvider.GetHash(model.Password, existingUser.PwdSalt);

                // 4. compare calculated hash with stored hash
                if (b64hash != existingUser.PwdHash)
                {
                    ModelState.AddModelError("", genericLoginFail);
                    return View(model);
                }

                // 5. read role name from related Role entity
                string roleName = existingUser.Role.Name;

                // 6. create cookie claims with username and role
                List<Claim> claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, existingUser.Username),
                    new Claim(ClaimTypes.Role, roleName)
                };

                ClaimsIdentity claimsIdentity = new ClaimsIdentity(
                    claims,
                    CookieAuthenticationDefaults.AuthenticationScheme);

                ClaimsPrincipal claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

                // 7. sign in user using authentication cookie
                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    claimsPrincipal);

                // 8. redirect safely after successful login, using LocalRedirect to prevent redirect to external sites!!!
                if (!string.IsNullOrWhiteSpace(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
                    return LocalRedirect(model.ReturnUrl);

                TempData["LoginMessage"] = "You have been logged in successfully.";

                if (roleName == "Admin")
                {
                    return RedirectToAction("Search", "Event");
                }

                if (roleName == "User")
                {
                    return RedirectToAction("Index", "Home");
                }

                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(model);
            }
        }

        // GET: User/Logout
        public async Task<IActionResult> Logout()
        {
            // 1. remove authentication cookie
            await HttpContext
                  .SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            TempData["LogoutMessage"] = "You have been logged out.";

            return RedirectToAction("Index", "Home");
        }

        // GET: User/Forbidden
        public IActionResult Forbidden()
        {
            return View();
        }

        [Authorize]
        public IActionResult ProfileDetails()
        {
            var username = HttpContext.User.Identity.Name;

            var userDb = _userRepository.GetUserWithRoleByUsername(username);
            ProfileVM userVm = _mapper.Map<ProfileVM>(userDb);

            return View(userVm);
        }
        [Authorize]
        public IActionResult ProfileEdit(int id)
        {
            var username = HttpContext.User.Identity.Name;
            var userDb = _userRepository.GetUserWithRoleByUsername(username);

            // Event Manager: only allow editing own profile
            if (userDb == null || userDb.Id != id)
                return Forbid();

            ProfileVM userVm = _mapper.Map<ProfileVM>(userDb);

            return View(userVm);
        }

        [Authorize]
        [HttpPost]
        public IActionResult ProfileEdit(int id, UpdateProfileVM userVm)
        {
            var username = HttpContext.User.Identity.Name;//is this also redundent?
            var userDb = _userRepository.GetUserWithRoleByUsername(username);

            if (userDb == null || userDb.Id != id)
                return Forbid();

            _mapper.Map(userVm, userDb);

            _userRepository.SaveChanges();

            return RedirectToAction("ProfileDetails");
        }
        [Authorize]
        public JsonResult GetProfileData(int id)
        {
            var username = HttpContext.User.Identity.Name;
            var userDb = _userRepository.GetUserWithRoleByUsername(username);

            if (userDb == null || userDb.Id != id)
                return Json(new { error = "Forbidden" });

            return Json(_mapper.Map<UpdateProfileVM>(userDb));
        }

        [Authorize]
        [HttpPut]
        public ActionResult SetProfileData(int id, [FromBody] UpdateProfileVM userVm)
        {
            var username = HttpContext.User.Identity.Name;
            var userDb = _userRepository.GetUserWithRoleByUsername(username);

            if (userDb == null || userDb.Id != id)
                return Forbid();

            if (_userRepository.EmailExists(userVm.Email, userDb.Id))
                return BadRequest("Email is already in use.");

            _mapper.Map(userVm, userDb);

            _userRepository.SaveChanges();

            return Ok();
        }

        [Authorize(Roles = "Admin")]
        public IActionResult RegistrationsOverview()
        {
            List<Registration> registrations = _registrationRepository.GetAllRegistrationsWithDetails();
            List<AdminUserRegistrationVM> model = _mapper.Map<List<AdminUserRegistrationVM>>(registrations);

            return View(model);
        }
    }
}