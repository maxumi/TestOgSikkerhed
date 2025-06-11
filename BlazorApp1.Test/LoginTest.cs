using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BlazorApp1.Components.Account.Pages;
using BlazorApp1.Components.Account;
using BlazorApp1.Data;
using Bunit;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Moq;
using Bunit.TestDoubles;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Components;

namespace BlazorApp1.Test
{
    public class LoginTest : TestContext
    {
        private readonly Mock<SignInManager<ApplicationUser>> _mockSignInManager;
        private readonly Mock<ILogger<Register>> _mockLogger;
        private readonly FakeNavigationManager _nav;
        private readonly IdentityRedirectManager _redirectManager;
        public LoginTest()
        {
            _nav = new FakeNavigationManager(this);
            Services.AddSingleton<NavigationManager>(_nav);

            var userStore = new Mock<IUserStore<ApplicationUser>>();
            var userManager = new Mock<UserManager<ApplicationUser>>(
                                   userStore.Object, null, null, null, null,
                                   null, null, null, null);

            _mockSignInManager = new Mock<SignInManager<ApplicationUser>>(
                                   userManager.Object,
                                   Mock.Of<IHttpContextAccessor>(),
                                   Mock.Of<IUserClaimsPrincipalFactory<ApplicationUser>>(),
                                   null, null, null, null);

            Services.AddSingleton(_mockSignInManager.Object);

            _mockLogger = new Mock<ILogger<Register>>();
            Services.AddSingleton(_mockLogger.Object);

            _redirectManager = new IdentityRedirectManager(_nav);
            Services.AddSingleton(_redirectManager);
        }

        [Fact]
        public void CorrectLogin()
        {
            // Arrange
            _mockSignInManager.Setup(m => m.PasswordSignInAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()))
                .ReturnsAsync(SignInResult.TwoFactorRequired);


            // Minimal HttpContext that skips the SignOutAsync branch
            var http = new DefaultHttpContext();
            http.Request.Method = HttpMethods.Post;
            var nav = Services.GetRequiredService<NavigationManager>() as FakeNavigationManager;

            var cut = RenderComponent<Login>(parameters =>
            {
                parameters.AddCascadingValue(http);
            });


            // Act: Redirectmanager is used to redirect to the 2FA page,
            // however it will throw an error when used for unit tests
            var ex = Assert.Throws<InvalidOperationException>(() =>
            {
                cut.Find("input[autocomplete='username']")
                   .Change("testBruger@user.com");

                cut.Find("input[autocomplete='current-password']")
                   .Change("Password123");
                cut.Find("input[name='Input.RememberMe']")
                   .Change(true);
                cut.Find("form").Submit();
            });

            // Assert: The route will now be set to the 2FA page and have rememberMe set to true.
            Assert.Equal("IdentityRedirectManager can only be used during static rendering.", ex.Message);

            Assert.EndsWith("/Account/LoginWith2fa?rememberMe=True", nav.Uri);

        }

        [Fact]
        public void NoLoginInfoGiven_ShowError()
        {
            // Arrange

            // Minimal HttpContext that skips the SignOutAsync branch
            var http = new DefaultHttpContext();
            http.Request.Method = HttpMethods.Post;

            var cut = RenderComponent<Login>(parameters =>
            {
                parameters.AddCascadingValue(http);
            });

            // "Act"

            cut.Find("input[autocomplete='username']")
               .Change("");

            cut.Find("input[autocomplete='current-password']")
               .Change("");

            // Assert
            cut.Find("form").Submit();

            var alertHtml = cut.Find("ul[role='alert']").InnerHtml;

            Assert.Contains("The Email field is required.", alertHtml);
            Assert.Contains("The Password field is required.", alertHtml);

        }
    }
}
