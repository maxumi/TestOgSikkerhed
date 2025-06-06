using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BlazorApp1.Components.Pages.TestComponents;
using Bunit;
using Bunit.TestDoubles;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;

namespace BlazorApp1.Test
{
    public class AuthorizationTest : TestContext
    {
        [Fact]
        public void IsAuthorizedForAdmin_View()
        {
            // Arrange
            var authContext = this.AddTestAuthorization();
            authContext.SetAuthorized("admin@example.com");
            authContext.SetRoles("Admin");

            // Act: Get first AuthorizeAttribute from AdminPage.
            var cut = RenderComponent<AdminPage>();

            // Assert: If @attribute [Authorize(Roles = "Admin")] is set.
            Assert.Contains("Admin PAGE!!!", cut.Markup);
        }
        [Fact]
        public void IsAuthorizedForAdmin_Code()
        {
            // Arrange
            var authContext = this.AddTestAuthorization();
            authContext.SetAuthorized("admin@example.com");
            authContext.SetRoles("Admin");

            var fakeNav = Services.GetRequiredService<NavigationManager>() as FakeNavigationManager;

            // Act: fakeNav.NavigateTo to admin page. and render AdminPage component. 
            fakeNav.NavigateTo("/admin-page");
            _ = RenderComponent<AdminPage>();


            // Assert: since this user is in the "Admin" role, the final URL should end with "/admin-page"
            var relativePath = fakeNav.Uri.Replace(fakeNav.BaseUri, "");
            Assert.Equal("admin-page", relativePath);
        }
        [Fact]
        public void IsNotAuthorizedForAdmin_View()
        {
            // Arrange
            var authContext = this.AddTestAuthorization();
            authContext.SetAuthorized("student@example.com");
            authContext.SetRoles("Student");

            // Act
            var cut = RenderComponent<AdminPage>();

            // Assert: Checks if test is NOT rendered on page
            Assert.DoesNotContain("Admin PAGE!!!", cut.Markup);
        }

        [Fact]
        public void IsNotAuthorizedForAdmin_Code()
        {
            // Arrange: Wrong Role for this page
            var authContext = this.AddTestAuthorization();
            authContext.SetAuthorized("student@example.com");
            authContext.SetRoles("Student");

            // A fake navtion manager to check if user is re-routed
            var fakeNav = Services.GetRequiredService<NavigationManager>() as FakeNavigationManager;

            // Act: AdminPage goes to admin page url but under rendering will go back to base url.
            fakeNav.NavigateTo("/admin-page");
            var _ = RenderComponent<AdminPage>();

            // Assert: Re-routed over to base view for not having access to page
            Assert.Equal(fakeNav.BaseUri, fakeNav.Uri);
        }

        [Fact]
        public void DoesAuthorizedAdminAtrributeExists()
        {
            // Arrange
            var authContext = this.AddTestAuthorization();

            // Act: Get first AuthorizeAttribute from AdminPage.
            var authorizeAttr = typeof(AdminPage)
                .GetCustomAttributes(inherit: true)
                .OfType<AuthorizeAttribute>()
                .FirstOrDefault();

            // Assert: If @attribute [Authorize(Roles = "Admin")] is set.
            Assert.NotNull(authorizeAttr);
            Assert.Equal("Admin", authorizeAttr.Roles);
        }
    }
}
