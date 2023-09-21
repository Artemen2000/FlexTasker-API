using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace FlexTasker.Models
{
	public class AuthOptions : ControllerBase
	{
		public const string ISSUER = "MyAuthServer";
		public const string AUDIENCE = "MyAuthClient";
		const string KEY = "mysupersecret_secretkey!123";
		public const int LIFETIME = 1;
		public static SymmetricSecurityKey GetSymmetricSecurityKey()
		{
			return new SymmetricSecurityKey(Encoding.ASCII.GetBytes(KEY));
		}
	}
}
