using LoginAPI.DTOs.Account;
using LoginAPI.Models;
using LoginAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;
using System.Security.Claims;
using System.Threading.Tasks;

namespace LoginAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly JWTService _jwtService;
        private readonly SignInManager<User> _signInManager;
        private readonly UserManager<User> _userManager;

        public AccountController(JWTService jwtService,
            SignInManager<User> signInManager,
            UserManager<User> userManager)
        {
            this._jwtService = jwtService;
            this._signInManager = signInManager;
            this._userManager = userManager;
        }

        [Authorize]
        [HttpGet("refresh-user-token")]
        public async Task<ActionResult<UserDto>> RefreshUserToken()
        {
            var user = await _userManager.FindByNameAsync(User.FindFirst(ClaimTypes.Email)?.Value);
            return CreateApplicationUserDto(user);
        }

        [HttpPost("login")]
        public async Task<ActionResult<UserDto>> Login(LoginDto model)
        {
            var user = await _userManager.FindByNameAsync(model.UserName);
            
            //check if user exists
            if(user == null) 
            {
                return Unauthorized("Invalid username or password");
            }

            //check user's email has been confirmed or not
            if(user.EmailConfirmed == false)
            {
                return Unauthorized("Please confirm your email");
            }
 
            //check user password
            var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, false);
            if (!result.Succeeded)
            {
                return Unauthorized("Invalid username or password");
            }

            return CreateApplicationUserDto(user);
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto model)
        {
             if(await CheckEmailExistsAsync(model.Email))
            {
                return BadRequest($"An existing account is using {model.Email} email address. Please try with another email address");
            }
            var newUser = new User
            {
                firstName = model.FirstName.ToLower(),
                lastName = model.LastName.ToLower(),
                UserName = model.Email.ToLower(),
                Email = model.Email.ToLower(),
                EmailConfirmed = true
            };
            var result = await _userManager.CreateAsync(newUser, model.Password);
            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }
            return Ok(new JsonResult(new {title = "Account Created", message= "Your account has been created, you can login" }));
        }

        #region Private Helper Method
            private UserDto CreateApplicationUserDto(User user)
        {
            return new UserDto 
            {
                FirstName = user.firstName,
                LastName = user.lastName,
                JWT = _jwtService.CreateJWT(user),
            };
        }
        //check if user is using same email address that already been registered
        private async Task<bool> CheckEmailExistsAsync(string email)
        {
            return await _userManager.Users.AnyAsync(x => x.Email == email.ToLower());
        }
        #endregion
    }
}
