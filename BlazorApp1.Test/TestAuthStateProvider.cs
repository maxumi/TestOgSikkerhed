using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.Authorization;

namespace BlazorApp1.Test
{

    public class TestAuthStateProvider : AuthenticationStateProvider
    {
        private readonly ClaimsPrincipal _user;

        public TestAuthStateProvider(string userName)
        {
            var identity = new ClaimsIdentity(
                new[] { new Claim(ClaimTypes.Name, userName) },
                "test");
            _user = new ClaimsPrincipal(identity);
        }

        public override Task<AuthenticationState> GetAuthenticationStateAsync()
            => Task.FromResult(new AuthenticationState(_user));
    }

}
