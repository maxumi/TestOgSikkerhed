using BlazorApp1.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace BlazorApp1.Data.Context
{
    public class TodoDbContext : DbContext
    {
        public TodoDbContext(DbContextOptions<TodoDbContext> options)
            : base(options)
        {
        }

        public DbSet<Todo> ToDoList { get; set; }
        public DbSet<CprNr> CprNrList
        {
            get; set;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Todo>()
                .HasOne(t => t.CprNr)
                .WithMany(c => c.TodoList)
                .HasForeignKey(t => t.UserId)
                .HasPrincipalKey(c => c.Id);
        }
    }
}
