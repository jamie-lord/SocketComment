using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MyCouch;
using SocketComment.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SocketComment.Pages
{
    public class ThreadModel : PageModel, IDisposable
    {
        [BindProperty]
        public Thread Thread { get; set; }

        public async Task<IActionResult> OnGet(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return NotFound();
            }

            var rootComment = await _commentsStore.GetByIdAsync<Models.Comment>(id);

            if (rootComment != null)
            {
                Thread = new Thread()
                {
                    Root = rootComment
                };
            }
            else
            {
                return NotFound();
            }

            Thread.Children = GetChildComments(Thread.Root);

            if (Thread == null)
            {
                return NotFound();
            }

            return Page();
        }

        private MyCouchStore _commentsStore = new MyCouchStore("http://localhost:5984", "comments");

        private const int MAX_COMMENTS = 100;

        private IEnumerable<Thread> GetChildComments(Models.Comment rootComment, int count = 0)
        {
            // Returns comments sorted by created datetime
            //function(doc) {
            //    if (doc.$doctype == 'comment') {
            //        emit([doc.parent, doc.created], doc);
            //    }
            //}

            if (count > MAX_COMMENTS)
            {
                yield break;
            }

            count++;

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
                        Children = GetChildComments(child.Value, count)
                    };
                    yield return thread;
                }
            }
        }

        public void Dispose()
        {
            _commentsStore.Dispose();
        }
    }
}