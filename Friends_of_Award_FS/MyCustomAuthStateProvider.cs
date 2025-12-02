using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;

namespace Blazor_WithAuth_ForSWP5
{

	public class MyCustomAuthStateProvider : AuthenticationStateProvider
	{
		private ClaimsPrincipal _anonymous = new(new ClaimsIdentity());
		private ClaimsPrincipal? _currentUser = null;


		public override Task<AuthenticationState> GetAuthenticationStateAsync()
		{
			return Task.FromResult(new AuthenticationState(_currentUser ?? _anonymous));
		}


        /// <summary>
        /// Login mit Benutzername und Rolle
        /// </summary>
        public Task LoginAsync(string username, string role)
        {
            // Claims erstellen (Name + Rolle)
            var claims = new List<Claim>()
            {
                new Claim(ClaimTypes.Name, username),
                new Claim(ClaimTypes.Role, role)   // WICHTIG: Admin/User
            };

            var identity = new ClaimsIdentity(
                claims,
                authenticationType: "MyCustomAuthType"
            );

            _currentUser = new ClaimsPrincipal(identity);

            // Benachrichtige Blazor, dass sich der Auth-State geändert hat
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());

            return Task.CompletedTask;
        }

        /// <summary>
        /// Logout setzt den User auf "anonymous"
        /// </summary>
        public Task LogoutAsync()
        {
            _currentUser = null;
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
            return Task.CompletedTask;
        }
    }
}
