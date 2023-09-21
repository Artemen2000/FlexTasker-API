using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using FlexTasker.Models;
using FlexTasker.Database;

namespace FlexTasker.Controllers
{
	public class Account_Controller : Controller
	{
		private readonly ApplicationContext _context;
		public Account_Controller()
		{
			_context = new ApplicationContext();
		}

		// Is wiped every time
		//private List<Models.User> users = new List<Models.User>
		//{
		//	new Models.User { Name="admin@gmail.com", Password="12345" },
		//	new Models.User { Name="qwerty@gmail.com", Password="55555" }
		//};



		[HttpPost("/token")]
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

			return Json(response);
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

		[HttpPost("/register")]
		public IActionResult Register(string username, string password, string confPassword)
		{
			if (password.Equals(confPassword))
			{
				_context.users.Add(new Models.User { Name = username, Password = password });
				_context.SaveChanges();
				return Token(username, password);
			}
			return BadRequest(new { errorText = "Passwords are not equal" });
		}
	}
}
