using Microsoft.AspNetCore.Mvc;
using MyCouch;
using SocketComment.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SocketComment.Pages.Comment
{
    [Produces("application/json")]
    [Route("Comment")]
    public class CommentController : Controller, IDisposable
    {
        // GET: api/Comment/5
        [HttpGet("{id}", Name = "Get")]
        public string Get(int id)
        {
            return "value";
        }

        // POST: api/Comment
        [HttpPost]
        public void Post([FromBody]string value)
        {
        }

        [HttpGet("DeleteThread/{id}", Name = "DeleteThread")]
        public async Task<IActionResult> DeleteThread(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return NotFound();
            }

            var rootComment = await _commentsStore.GetByIdAsync<Models.Comment>(id);

            if (rootComment == null)
            {
                return NotFound();
            }

            var thread = new Thread()
            {
                Root = rootComment,
                Children = GetChildComments(rootComment)
            };

            foreach (var i in thread.ChildIds)
            {
                await _commentsStore.DeleteAsync(i);
            }

            if (rootComment.Parent != null)
            {
                return Redirect("/Thread/" + rootComment.Parent);
            }
            return RedirectToPage("/Index");
        }

        [HttpGet("DeleteComment/{id}", Name = "DeleteComment")]
        public async Task<IActionResult> DeleteComment(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return NotFound();
            }

            var comment = await _commentsStore.GetByIdAsync<Models.Comment>(id);

            if (comment == null)
            {
                return NotFound();
            }

            DeleteComment(comment);

            comment = await _commentsStore.StoreAsync(comment);

            return Redirect("/Thread/" + comment.Id);
        }

        private MyCouchStore _commentsStore = new MyCouchStore("http://localhost:5984", "comments");

        private IEnumerable<Thread> GetChildComments(Models.Comment rootComment)
        {
            // Returns comments sorted by created datetime
            //function(doc) {
            //    if (doc.$doctype == 'comment') {
            //        emit([doc.parent, doc.created], doc);
            //    }
            //}

            var query = new Query("comments", "children")
            {
                StartKey = new object[] { rootComment.Id },
                EndKey = new object[] { rootComment.Id, new object() }
            };

            var children = _commentsStore.QueryAsync<Models.Comment>(query).Result;

            foreach (var child in children)
            {
                if (child.Value != null)
                {
                    var thread = new Thread()
                    {
                        Root = child.Value,
                        Children = GetChildComments(child.Value)
                    };
                    yield return thread;
                }
            }
        }

        private void DeleteComment(Models.Comment comment)
        {
            comment.Author = null;
            comment.Message = null;
            comment.Deleted = true;
        }

        public new void Dispose()
        {
            _commentsStore.Dispose();
            base.Dispose();
        }
    }
}
