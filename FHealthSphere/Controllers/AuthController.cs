using Contract.Services.Interface;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Contract.Repositories.Entity;
using Microsoft.AspNetCore.Authorization;
using Services.Service;
using Google.Apis.Auth;
using ModelViews.AuthModelViews;

namespace FHealthSphere.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly TokenService _tokenService;
        public AuthController(IUserService userService, TokenService tokenService)
        {
            _userService = userService;
            _tokenService = tokenService;
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> GoogleSignIn([FromBody] string request)
        {
            Account acc = new Account();
            try
            {
                var payload = await GoogleJsonWebSignature.ValidateAsync(request);

                if (payload == null)
                    return BadRequest("Invalid Google token");
                Console.WriteLine(payload.Email);
                var user = await _userService.GetUserByEmail(payload.Email);
                //check user don't exist create user
                if (user == null)
                {

                    user = new Account
                    {
                        UserName = payload.Email.Split("@")[0],
                        Email = payload.Email,
                        FullName = payload.Name,
                        CreatedBy = payload.Name,
                        CreatedTime = DateTime.UtcNow,
                        LastUpdatedBy = payload.Name,
                        LastUpdatedTime = DateTime.UtcNow

                    };

                    user = await _userService.CreateAccountAsync(user);
                }
                Console.WriteLine("User ID" + user.Id.ToString());
                var jwtToken = _tokenService.GenerateJwtTokenAsync(user);
                return Ok(new { token = jwtToken, user });
            }
            catch
            {
                return BadRequest("Google authentication failed");
            }
        }
        private string GenerateJwtToken(Account user)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("hoqee0yaZ7jg1kBiii75VNA4fAuWPTA0A9pVY5W+XKV8IAf+99yvEMjIWLGAWYOU2iFrGL+Ct7FupbTX2LYzXQ=="));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.UserName)
            };

            var token = new JwtSecurityToken(
                issuer: "yourdomain.com",
                audience: "yourdomain.com",
                claims: claims,
                expires: DateTime.UtcNow.AddHours(2),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        //[AllowAnonymous]
        //[HttpGet("login")]
        //public async Task<IActionResult> Login(string token)
        //{
        //    Account acc = new Account();
        //    try
        //    {
        //        var payload = await GoogleJsonWebSignature.ValidateAsync(token);

        //        if (payload == null)
        //            return BadRequest("Invalid Google token");
        //        Console.WriteLine(payload.Email);
        //        var user = await _userService.GetUserByEmail(payload.Email);

        //        //check user don't exist create user
        //        if (user == null)
        //        {
        //            return BadRequest("account is not existed");
        //        }
        //        Console.WriteLine("User ID" + user.Id.ToString());
        //        var jwtToken = _tokenService.GenerateJwtTokenAsync(user);
        //        return Ok(new { token = jwtToken, user });
        //    }
        //    catch
        //    {
        //        return BadRequest("Google authentication failed");
        //    }
        //}
        [Authorize] // Bắt buộc có JWT Token
        [HttpGet("validate")]
        public IActionResult ValidateToken()
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            if (identity != null && identity.IsAuthenticated)
            {
                var claims = identity.Claims.Select(c => new { c.Type, c.Value }).ToList();
                return Ok(new
                {
                    IsValid = true,
                    User = claims
                });
            }

            return Unauthorized(new { IsValid = false, Message = "Invalid token" });
        }
    }
}
