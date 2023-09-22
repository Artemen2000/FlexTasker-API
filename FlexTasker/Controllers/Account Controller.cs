using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using FlexTasker.Models;
using FlexTasker.Database;
using Microsoft.Identity.Client;
using Microsoft.AspNetCore.Authorization;

namespace FlexTasker.Controllers
{
	public class JsonCreator : Controller { }

	[Route("api/[controller]")]
	[ApiController]
	public class AccountController : ControllerBase
	{
		private readonly ApplicationContext _context;
		public AccountController()
		{
			_context = new ApplicationContext();
		}

		[HttpPost("/api/signin")]
		public IActionResult Token(string username, string password)
		{
			var identity = GetIdentity(username, password);
			if (identity == null)
			{
				return BadRequest(new { errorText = "Invalid username or password" });
			}

			var now = DateTime.UtcNow;
			var jwt = new JwtSecurityToken(
				issuer: AuthOptions.ISSUER,
				audience: AuthOptions.AUDIENCE,
				notBefore: now,
				claims: identity.Claims,
			expires: now.Add(TimeSpan.FromMinutes(AuthOptions.LIFETIME)),
				signingCredentials: new Microsoft.IdentityModel.Tokens.SigningCredentials(
					AuthOptions.GetSymmetricSecurityKey(), SecurityAlgorithms.HmacSha256));
			var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);

			var response = new
			{
				access_token = encodedJwt,
				username = identity.Name
			};

			JsonCreator jc = new JsonCreator();
			return jc.Json(response);
		}

		private ClaimsIdentity GetIdentity(string username, string password)
		{
			Models.User user = _context.users.FirstOrDefault(x => x.Name == username && x.Password == password);
			if (user != null)
			{
				var claims = new List<Claim>
				{
					new Claim(ClaimsIdentity.DefaultNameClaimType, user.Name)
				};
				ClaimsIdentity claimsIdentity = new ClaimsIdentity(claims, "Token", ClaimsIdentity.DefaultNameClaimType, ClaimsIdentity.DefaultRoleClaimType);
				return claimsIdentity;
			}
			return null;
		}

		[HttpPost("/api/signup")]
		public IActionResult Register(string username, string password, string confPassword)
		{
			if (username.IsNullOrEmpty() || password.IsNullOrEmpty() || confPassword.IsNullOrEmpty())
				return BadRequest(new { errorText = "All field must be filled" });
			if (!password.Equals(confPassword))
				return BadRequest(new { errorText = "Passwords are not equal" });
			User user = _context.users.FirstOrDefault(x => x.Name == username);
			if (user != null)
				return BadRequest(new { errorText = "This user is already registered" });
			
			_context.users.Add(new Models.User { Name = username, Password = password });
			_context.SaveChanges();
			long userid = _context.users.SingleOrDefault(user => user.Name == username).Id;
			_context.todoLists.Add(new TodoList { Name = "Default List", UserId = userid, ListType = Models.Type.DEFAULT });
			_context.SaveChanges();
			return Token(username, password);
		}

		[Authorize]
		[HttpPost("/api/logout")]
		public IActionResult Logout()
		{
			return Ok();
		}
	}
}
