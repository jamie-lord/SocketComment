using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MyCouch;
using SocketComment.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SocketComment.Pages
{
    public class ThreadModel : PageModel
    {
        [BindProperty]
        public Thread Thread { get; set; }

        public async Task<IActionResult> OnGet(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return NotFound();
            }

            using (var store = new MyCouchStore("http://localhost:5984", "comments"))
            {
                var rootComment = await store.GetByIdAsync<Comment>(id);

                if (rootComment != null)
                {
                    Thread = new Thread()
                    {
                        Root = rootComment
                    };
                }

                Thread.Children = await GetChildComments(store, Thread.Root);
            }

            if (Thread == null)
            {
                return NotFound();
            }

            return Page();
        }

        private const int MAX_DEPTH = 4;
        private const int MAX_COMMENTS = 100;

        private async Task<List<Thread>> GetChildComments(MyCouchStore store, Comment rootComment, int depth = 0, int count = 0)
        {
            //function(doc) {
            //    if (doc.$doctype == 'comment') {
            //        emit(doc.parent, doc);
            //    }
            //}

            if (depth > MAX_DEPTH || count > MAX_COMMENTS)
            {
                return null;
            }

            depth++;
            count++;

            var query = new Query("comments", "children")
            {
                Key = rootComment.Id
            };

            var children = await store.QueryAsync<Comment>(query);

            var result = new List<Thread>();
            foreach (var child in children)
            {
                if (child.Value != null)
                {
                    var thread = new Thread()
                    {
                        Root = child.Value,
                        Children = await GetChildComments(store, child.Value, depth, count)
                    };
                    result.Add(thread);
                }
            }
            return result;
        }
    }
}