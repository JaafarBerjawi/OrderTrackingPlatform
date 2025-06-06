﻿using System.ComponentModel.DataAnnotations;

namespace Login.Web.Models;

public class LoginInput
{
	[Required(ErrorMessage = "Username is required.")]
	public string Username { get; set; }

	[Required(ErrorMessage = "Password is required.")]
	[MinLength(8, ErrorMessage = "Password must be at least 8 characters long.")]
	public string Password { get; set; }
}
