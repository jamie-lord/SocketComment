using System;
using System.Collections.Generic;
using System.Net;
using Bogus;
using Bogus.DataSets;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using SocketComment.Models;

namespace SocketComment.FrontEnd.Controllers
{
    [Route("generate")]
    public class GenerateController : Controller
    {
        public GenerateController(CommentService commentService)
        {
            _commentService = commentService;
        }

        private CommentService _commentService;

        [HttpPost("hackernews")]
        public IActionResult HackerNews(int id)
        {
            var url = "https://hn.algolia.com/api/v1/items/" + id;

            var webClient = new WebClient();
            var result = webClient.DownloadString(new Uri(url));

            if (string.IsNullOrWhiteSpace(result))
            {
                return NotFound();
            }

            var comment = JsonConvert.DeserializeObject<HackerNewsItem>(result);

            if (comment == null)
            {
                return NotFound();
            }

            var rootId = comment.SaveChildren(_commentService, null);

            return Redirect("/thread/" + rootId);
        }

        [HttpGet("random")]
        public IActionResult Random(int count = 100)
        {
            _count = count;
            var thread = RandomThread(null, 0);

            if (thread.Root.Id != null)
            {
                return Redirect("/Thread/" + thread.Root.Id);
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

        private Thread RandomThread(Comment parent, int depth)
        {
            if (_count <= 0 || depth > MAX_CHILD_COMMENT_DEPTH)
            {
                return null;
            }

            depth++;

            var thread = new Thread
            {
                Root = RandomComment(parent)
            };

            if (thread.Root == null)
            {
                return null;
            }

            var children = new List<Thread>();
            for (int i = 0; i < _random.Next(MAX_CHILD_COMMENT_DEPTH - depth, MAX_CHILD_COMMENTS_PER_THREAD + 1); i++)
            {
                var child = RandomThread(thread.Root, depth);
                if (child != null)
                {
                    children.Add(child);
                }
            }

            thread.Children = children;
            return thread;
        }

        private Comment RandomComment(Comment parent)
        {
            if (_count <= 0)
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

            comment = _commentService.StoreComment(comment).Result;
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

        private class HackerNewsItem
        {
            public int id { get; set; }
            public DateTime created_at { get; set; }
            public int created_at_i { get; set; }
            public string type { get; set; }
            public string author { get; set; }
            public string title { get; set; }
            public string url { get; set; }
            public string text { get; set; }
            public object points { get; set; }
            public int? parent_id { get; set; }
            public int? story_id { get; set; }
            public List<HackerNewsItem> children { get; set; }
            public List<object> options { get; set; }

            public string SaveChildren(CommentService commentService, string parentId)
            {
                var comment = new Comment
                {
                    Author = author,
                    Created = created_at,
                    Parent = parentId
                };
                if (type == "comment")
                {
                    comment.Message = text;
                }
                else
                {
                    comment.Message = $"<h1><small>{points}</small> {title}</h1><p>{url}</p>";
                }

                if (comment.Message == null)
                {
                    comment.Deleted = true;
                }

                comment = commentService.StoreComment(comment).Result;

                if (children != null)
                {
                    foreach (var child in children)
                    {
                        child.SaveChildren(commentService, comment.Id);
                    }
                }

                return comment.Id;
            }
        }
    }
}