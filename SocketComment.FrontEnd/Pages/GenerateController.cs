using System;
using System.Collections.Generic;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace SocketComment.FrontEnd.Pages
{
    public class GenerateController : Controller
    {
        public GenerateController(CommentService commentService)
        {
            _commentService = commentService;
        }

        private CommentService _commentService;

        [HttpGet("HackerNews/{id}", Name = "HackerNews")]
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

            return Redirect("/Thread/" + rootId);
        }

        private class HackerNewsItem
        {
            public int id { get; set; }
            public DateTime created_at { get; set; }
            public int created_at_i { get; set; }
            public string type { get; set; }
            public string author { get; set; }
            public string title { get; set; }
            public object url { get; set; }
            public string text { get; set; }
            public object points { get; set; }
            public int? parent_id { get; set; }
            public int? story_id { get; set; }
            public List<HackerNewsItem> children { get; set; }
            public List<object> options { get; set; }

            public string SaveChildren(CommentService commentService, string parentId)
            {
                var comment = new Models.Comment
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
                    comment.Message = $"Hacker News item type: {type}\n{title}";
                }
                comment = commentService.StoreComment(comment).Result;

                foreach (var child in children)
                {
                    child.SaveChildren(commentService, comment.Id);
                }

                return comment.Id;
            }
        }
    }
}