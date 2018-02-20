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

        private async Task<List<Comment>> GetChildComments(MyCouchStore store, Comment rootComment)
        {
            //function(doc) {
            //    if (doc.$doctype == 'comment') {
            //        emit(doc.parent, doc);
            //    }
            //}

            var query = new Query("comments", "children")
            {
                Key = rootComment.Id
            };

            var children = await store.QueryAsync<Comment>(query);

            var result = new List<Comment>();
            foreach (var child in children)
            {
                if (child.Value != null)
                {
                    result.Add(child.Value);
                }
            }
            return result;
        }
    }
}