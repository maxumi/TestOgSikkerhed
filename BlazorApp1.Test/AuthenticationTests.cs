using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BlazorApp1.Components.Pages.TestComponents;
using Bunit;
using Bunit.TestDoubles;

namespace BlazorApp1.Test
{
    public class AuthenticationTests : TestContext
    {
        [Fact]
        public void IfNotAuthenticated_View()
        {
            // Arrange
            var authContext = this.AddTestAuthorization();
            authContext.SetNotAuthorized();

            // Act
            var cut = RenderComponent<AuthenticationTest>();

            // Assert
            Assert.Contains("Sorry but you are not Authenticate", cut.Markup);
        }
        [Fact]
        public void IfNotAuthenticated_Code()
        {
            // Arrange
            var authContext = this.AddTestAuthorization();
            authContext.SetNotAuthorized();

            // Act
            var cut = RenderComponent<AuthenticationTest>();
            var result = cut.Instance.GetUserName();


            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void IfAuthenticated_View()
        {
            // Arrange
            var authContext = this.AddTestAuthorization();
            authContext.SetAuthorized("user@example.com");

            // Act
            var cut = RenderComponent<AuthenticationTest>();

            // Assert
            Assert.Contains("Welcome, You are logged in", cut.Markup);
        }

        [Fact]
        public void IfAuthenticated_Code()
        {
            // Arrange
            var authContext = this.AddTestAuthorization();
            authContext.SetAuthorized("user@example.com");

            // Act
            var cut = RenderComponent<AuthenticationTest>();
            var result = cut.Instance.GetUserName();

            // Assert
            Assert.Equal("user@example.com", result);
        }
    }
}
