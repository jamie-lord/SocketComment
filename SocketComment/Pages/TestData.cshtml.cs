using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MyCouch;
using SocketComment.Models;
using Bogus;
using System.Collections.Generic;
using System;
using Bogus.DataSets;

namespace SocketComment.Pages
{
    public class TestDataModel : PageModel
    {
        public IActionResult OnGet(int count = 100)
        {
            _count = count;
            string rootId = null;
            using (var store = new MyCouchStore("http://localhost:5984", "comments"))
            {
                var thread = RandomThread(store, null, 0);
                rootId = thread.Root.Id;
            }

            if (rootId != null)
            {
                return Redirect("/Thread/" + rootId);
            }
            else
            {
                return RedirectToPage("Index");
            }
        }

        private int _count;

        private const int MAX_CHILD_COMMENTS_PER_THREAD = 20;
        private const int MAX_CHILD_COMMENT_DEPTH = 5;

        private Random _random = new Random();

        private Thread RandomThread(MyCouchStore store, Comment parent, int depth)
        {
            if (_count < 0 || depth > MAX_CHILD_COMMENT_DEPTH)
            {
                return null;
            }

            depth++;

            var thread = new Thread
            {
                Root = RandomComment(store, parent)
            };

            if (thread.Root == null)
            {
                return null;
            }

            var children = new List<Thread>();
            for (int i = 0; i < _random.Next(MAX_CHILD_COMMENT_DEPTH - depth, MAX_CHILD_COMMENTS_PER_THREAD + 1); i++)
            {
                var child = RandomThread(store, thread.Root, depth);
                if (child != null)
                {
                    children.Add(child);
                }
            }

            thread.Children = children;
            return thread;
        }

        private Comment RandomComment(MyCouchStore store, Comment parent)
        {
            if (_count < 0)
            {
                return null;
            }
            _count--;

            Faker faker = null;

            switch (_random.Next(0, 4))
            {
                case 0:
                    faker = new Faker("ko");
                    break;
                case 1:
                    faker = new Faker("ru");
                    break;
                case 2:
                    faker = new Faker("cz");
                    break;
                case 3:
                    faker = new Faker("sv");
                    break;
            }

            var comment = new Comment
            {
                Author = faker.Name.FullName(Name.Gender.Female),
                Message = faker.Lorem.Sentences(_random.Next(1, 11), " "),
                Parent = parent?.Id
            };

            if (parent?.Created == null)
            {
                comment.Created = faker.Date.Past(5);
            }
            else
            {
                comment.Created = Future(2, parent.Created);
            }

            comment = store.StoreAsync(comment).Result;
            return comment;
        }

        private DateTime Future(int daysToGoForward = 1, DateTime? refDate = null)
        {
            var minDate = refDate ?? DateTime.Now;
            var maxDate = minDate.AddDays(daysToGoForward);
            var totalTimeSpanTicks = (maxDate - minDate).Ticks;
            //find % of the timespan
            var partTimeSpanTicks = _random.NextDouble() * totalTimeSpanTicks;
            var partTimeSpan = TimeSpan.FromTicks(Convert.ToInt64(partTimeSpanTicks));
            return minDate + partTimeSpan;
        }
    }
}