using System;
using System.Threading.Tasks;
using AspNetCoreTodo.Data;
using AspNetCoreTodo.Models;
using AspNetCoreTodo.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Xunit;

namespace AspNetCoreTodo.UnitTests
{
    public class TodoItemServiceShould
    {
        [Fact]
        public async Task MarkDoneWithId()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>().UseInMemoryDatabase(databaseName: "Test_MarkDone").Options;
            var guid = Guid.NewGuid();

            using (var context = new ApplicationDbContext(options))
            {
                               
                var fakeItem = new TodoItem
                {
                    Id = guid,
                    Title = "Fake title",
                    IsDone = false,
                    DueAt = DateTimeOffset.Now.AddDays(3),
                    UserId = "fake-000"
                };

                context.Items.Add(fakeItem);
                var saveResult = await context.SaveChangesAsync();

            }

            using (var context = new ApplicationDbContext(options))
            {
                var service = new TodoItemService(context);

                var fakeUser = new ApplicationUser
                {
                    Id = "fake-000",
                    UserName = "fake@fake.com"
                };

                var result = await service.MarkDoneAsync(guid, fakeUser);
                Assert.True(result);
            }
        }

        [Fact]
        public async Task AddNewItemAsIncompleteWithDueDate()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>().UseInMemoryDatabase(databaseName: "Test_AddNewItem").Options;
            // Set up a context (connection to the "DB") for writing
            using (var context = new ApplicationDbContext(options))
            {
                var service = new TodoItemService(context);
                var fakeUser = new ApplicationUser
                {
                    Id = "fake-000",
                    UserName = "fake@example.com"
                };
                await service.AddItemAsync(new TodoItem
                {
                    Title = "Testing?"
                }, fakeUser);
            }

            // Use a separate context to read data back from the "DB"
            using (var context = new ApplicationDbContext(options))
            {
                var itemsInDatabase = await context.Items.CountAsync();
                Assert.Equal(1, itemsInDatabase);

                var item = await context.Items.FirstAsync();
                Assert.Equal("Testing?", item.Title);
                Assert.Equal(false, item.IsDone);
                // Item should be due 3 days from now (give or take a second)
                //Took out of service
                //var difference = DateTimeOffset.Now.AddDays(3) - item.DueAt;
                //Assert.True(difference < TimeSpan.FromSeconds(3));
            }

        }
    }
}