using System;
using System.Linq;
using System.Threading.Tasks;
using BlazorApp1.Components.Account.Pages;
using BlazorApp1.Data.Context;
using BlazorApp1.Data.Models;
using Bunit;
using Bunit.TestDoubles;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;
using BlazorApp1.Components.Pages;

namespace BlazorApp1.Test
{
    public class RegisterCprComponentTests : TestContext
    {
        // Helper function for data being seeded with cprNr
        private TodoDbContext CreateDbContext(params CprNr[] seed)
        {
            // use in memory
            var options = new DbContextOptionsBuilder<TodoDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            var db = new TodoDbContext(options);
            if (seed?.Any() == true)
            {
                db.CprNrList.AddRange(seed);
                db.SaveChanges();
            }
            return db;
        }

        // A helper method that setups Auth and database context with common services
        private void RegisterCommonServices(string userName, TodoDbContext dbContext)
        {
            var authContext = this.AddTestAuthorization();
            authContext.SetAuthorized(userName);

            Services.AddScoped<TodoDbContext>(_ => dbContext);
            Services.AddScoped<IHashingHandler, HashingHandler>();
        }

        [Fact]
        public void RedirectsWhenCprAlreadyRegistered()
        {
            // Arrange
            var db = CreateDbContext(new CprNr { User = "alice", CprNum = "hashed" });
            RegisterCommonServices("alice", db);
            var nav = Services.GetRequiredService<FakeNavigationManager>();

            // Act
            RenderComponent<CprRegister>();

            // Assert – component navigates immediately on Init
            Assert.EndsWith("/Todo", nav.Uri);
        }

        [Fact]
        public void AuthorizedUser_CanAccessPage()
        {
            // Arrange
            var db = CreateDbContext();
            RegisterCommonServices("eve", db);

            // Act
            var cut = RenderComponent<CprRegister>();

            // Assert: if user is authorized, the page should render without meessage below
            Assert.Contains("Register CPR NR.", cut.Markup, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("You are not authorized to view this page.", cut.Markup);
        }

        [Fact]
        public void WhenCprFieldLeftEmpty()
        {
            // Arrange
            var db = CreateDbContext();
            RegisterCommonServices("bob", db);
            var cut = RenderComponent<CprRegister>();

            // Act
            cut.Find("form").Submit();

            // Assert
            Assert.Contains("CPR number is required.", cut.Markup);
        }

        [Fact]
        public void WhenCprFormatWrongOrInvalid()
        {
            // Arrange
            var db = CreateDbContext();
            RegisterCommonServices("bob", db);
            var cut = RenderComponent<CprRegister>();
            // Act
            cut.Find("input#cpr").Change("notacpr");
            cut.Find("form").Submit();

            // Assert
            Assert.Contains("CPR is wrong.", cut.Markup);
        }

        [Fact]
        public void WhenValidCprSubmitted()
        {
            // Arrange
            var db = CreateDbContext();
            RegisterCommonServices("carol", db);
            var nav = Services.GetRequiredService<FakeNavigationManager>();
            var hashing = Services.GetRequiredService<IHashingHandler>();
            var cut = RenderComponent<CprRegister>();

            // Act
            const string plainCpr = "010101-1234";
            cut.Find("input#cpr").Change(plainCpr);
            cut.Find("form").Submit();

            var record = db.CprNrList.Single(r => r.User == "carol");

            // Assert
            // The value is not normal cpr text
            Assert.NotEqual(plainCpr, record.CprNum);
            Assert.True(hashing.VerifyBCrypt2(plainCpr, record.CprNum));
            Assert.EndsWith("/Todo?status=registered", nav.Uri);
        }
    }
}
