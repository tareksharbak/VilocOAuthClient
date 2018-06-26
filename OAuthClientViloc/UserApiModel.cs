using System;
using System.Collections.Generic;
using System.Text;

namespace OAuthClientViloc
{
	public class UserApiModel
	{
		public string Token { get; set; }
		public string UserName { get; set; }
		public string Password { get; set; }
		public string ConfirmPassword { get; set; }
	}
}
