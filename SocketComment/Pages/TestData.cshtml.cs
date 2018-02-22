using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MyCouch;
using SocketComment.Models;
using Bogus;
using System.Collections.Generic;

namespace SocketComment.Pages
{
    public class TestDataModel : PageModel
    {
        public IActionResult OnGet()
        {
            var faker = new Faker("en");

            using (var store = new MyCouchStore("http://localhost:5984", "comments"))
            {
                var thread = RandomThread(store, faker, null);
            }

            return RedirectToPage("Index");
        }

        private Thread RandomThread(MyCouchStore store, Faker faker, string parentId, int depth = 0)
        {
            if (depth == 5)
            {
                return null;
            }

            depth++;

            var thread = new Thread
            {
                Root = RandomComment(store, faker, parentId)
            };

            var children = new List<Thread>();
            for (int i = 0; i < faker.Random.Int(0, 10); i++)
            {
                var child = RandomThread(store, faker, thread.Root.Id, depth);
                if (child != null)
                {
                    children.Add(child);
                }
            }

            thread.Children = children;
            return thread;
        }

        private Comment RandomComment(MyCouchStore store, Faker faker, string parentId)
        {
            var comment = new Comment
            {
                Author = faker.Internet.UserName(),
                Created = faker.Date.Past(5),
                Message = faker.Lorem.Sentences(faker.Random.Int(1, 10), " "),
                Parent = parentId
            };
            comment = store.StoreAsync(comment).Result;
            return comment;
        }
    }
}