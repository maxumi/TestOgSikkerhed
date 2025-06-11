using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BlazorApp1.Components.Account.Pages;
using BlazorApp1.Components.Account;
using BlazorApp1.Data;
using Bunit.TestDoubles;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Moq;
using Microsoft.Extensions.DependencyInjection;
using BlazorApp1.Components.Account.Shared;
using Microsoft.AspNetCore.Mvc;

namespace BlazorApp1.Test
{
    public class RegisterTest : TestContext
    {
        // Declare mocks for all dependencies
        private readonly Mock<UserManager<ApplicationUser>> _mockUserManager;
        private readonly Mock<IUserStore<ApplicationUser>> _mockUserStore;
        private readonly Mock<SignInManager<ApplicationUser>> _mockSignInManager;
        private readonly Mock<IEmailSender<ApplicationUser>> _mockEmailSender;
        private readonly Mock<ILogger<Register>> _mockLogger;
        private readonly Mock<IdentityRedirectManager> _mockRedirectManager;
        private readonly Mock<RoleManager<IdentityRole>> _mockRoleManager;

        public RegisterTest()
        {

            _mockUserStore = new Mock<IUserStore<ApplicationUser>>();
            _mockUserStore.As<IUserEmailStore<ApplicationUser>>();

            _mockUserManager = new Mock<UserManager<ApplicationUser>>(
                _mockUserStore.Object, null, null, null, null, null, null, null, null);

            _mockUserManager.Setup(m => m.SupportsUserEmail).Returns(true);

            _mockSignInManager = new Mock<SignInManager<ApplicationUser>>(
                _mockUserManager.Object, Mock.Of<Microsoft.AspNetCore.Http.IHttpContextAccessor>(), Mock.Of<IUserClaimsPrincipalFactory<ApplicationUser>>(), null, null, null, null);

            _mockRoleManager = new Mock<RoleManager<IdentityRole>>(
                Mock.Of<IRoleStore<IdentityRole>>(), null, null, null, null);

            _mockEmailSender = new Mock<IEmailSender<ApplicationUser>>();
            _mockLogger = new Mock<ILogger<Register>>();

            var navManager = new FakeNavigationManager(this);
            _mockRedirectManager = new Mock<IdentityRedirectManager>(navManager);

            Services.AddSingleton(_mockUserManager.Object);
            Services.AddSingleton(_mockUserStore.Object);
            Services.AddSingleton(_mockSignInManager.Object);
            Services.AddSingleton(_mockEmailSender.Object);
            Services.AddSingleton(_mockLogger.Object);
            Services.AddSingleton(_mockRedirectManager.Object);
            Services.AddSingleton(_mockRoleManager.Object);
            Services.AddSingleton<NavigationManager>(navManager);
        }
        [Fact]
        public async Task WhenEmailAlreadyExists_ShowError()
        {
            // Arrange — existing user & empty roles
            var emptyRoles = new List<IdentityRole>().AsQueryable();
            _mockRoleManager.Setup(r => r.Roles).Returns(emptyRoles);

            // Act
            var cut = RenderComponent<Register>();

            var existingUser = new ApplicationUser
            {
                UserName = "alice@example.com",
                Email = "alice@example.com"
            };

            // Let your SUT think a user with that e-mail is already in the store
            _mockUserManager.Setup(um => um.FindByEmailAsync(existingUser.Email))
                            .ReturnsAsync(existingUser);

            // duplicate-user error
            _mockUserManager.Setup(um => um.CreateAsync(
                    It.IsAny<ApplicationUser>(),
                    It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Failed(
                    new IdentityError
                    {
                        Code = nameof(IdentityErrorDescriber.DuplicateUserName),
                        Description = "User already exists."
                    }));

            // act
            cut.Find("input[autocomplete=username]")
               .Change(existingUser.Email);
            cut.Find("input[autocomplete=new-password]").Change("Pa$$w0rd");
            cut.FindAll("input[autocomplete=new-password]")[1].Change("Pa$$w0rd");
            cut.Find("form").Submit();

            // Assert: Get div of the error message
            var alert = cut.WaitForElement("div.alert-danger[role='alert']");
            Assert.Contains("Error: User already exists", alert.TextContent.Trim());
        }

        [Fact]
        public async Task WhenPasswordTooShort_ShowError()
        {
            // Arrange
            var emptyRoles = new List<IdentityRole>().AsQueryable();
            _mockRoleManager.Setup(r => r.Roles).Returns(emptyRoles);

            _mockUserManager
                .Setup(um => um.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Success);

            var cut = RenderComponent<Register>();

            // An email and a too short password ("123")
            cut.Find("input[autocomplete=username]").Change("foo@bar.com");
            cut.Find("input[autocomplete=new-password]").Change("123");
            cut.FindAll("input[autocomplete=new-password]")[1].Change("123");

            // Act
            await cut.Find("form").SubmitAsync();

            // Assert: Get div of the error message
            var errorDiv = cut.WaitForElement("div.text-danger");

            Assert.Equal(
                "The Password must be at least 6 and at max 100 characters long.",
                errorDiv.TextContent.Trim()
            );
        }
        [Fact]
        public async Task WhenConfirmPasswordIsWrong_ShowError()
        {
            // Arrange
            var emptyRoles = new List<IdentityRole>().AsQueryable();
            _mockRoleManager.Setup(r => r.Roles).Returns(emptyRoles);

            _mockUserManager
                .Setup(um => um.CreateAsync(
                    It.IsAny<ApplicationUser>(),
                    It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Success);

            // Act
            var cut = RenderComponent<Register>();

            cut.Find("input[autocomplete=username]").Change("foo@bar.com");
            cut.Find("input[autocomplete=new-password]").Change("Pa$$w0rd1");
            cut.FindAll("input[autocomplete=new-password]")[1].Change("Pa$$w0rd2");

            await cut.Find("form").SubmitAsync();

            // Assert: Get div of the error message
            var errorDiv = cut.WaitForElement("div.text-danger");
            Assert.Equal(
                "The password and confirmation password do not match.",
                errorDiv.TextContent.Trim());
        }
    }

}
