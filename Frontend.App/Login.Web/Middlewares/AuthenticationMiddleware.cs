using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Login.Web.Middlewares
{
	public class AuthenticationMiddleware
	{
		private readonly RequestDelegate _next;

		public AuthenticationMiddleware(RequestDelegate next)
		{
			_next = next;
		}

		public async Task Invoke(HttpContext context)
		{
			var user = context.User;
			if (user.Identity != null && user.Identity.IsAuthenticated)
			{
				var expClaim = user.Claims.FirstOrDefault(c => c.Type == "exp");
				if (expClaim != null &&
					long.TryParse(expClaim.Value, out var expSeconds))
				{
					var expDate = DateTimeOffset.FromUnixTimeSeconds(expSeconds).UtcDateTime;
					if (DateTime.UtcNow > expDate)
					{
						// Token expired
						context.Response.Cookies.Delete("access_token"); // Or clear cookie if stored in cookie
						context.Response.Redirect("/Login");
						return;
					}
				}
			}

			await _next(context);
		}
	}
}
