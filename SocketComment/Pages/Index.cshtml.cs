using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MyCouch;
using SocketComment.Models;

namespace SocketComment.Pages
{
    public class IndexModel : PageModel
    {
        [BindProperty]
        public List<Comment> RootComments { get; set; }

        public async Task<IActionResult> OnGet()
        {
            using (var store = new MyCouchStore("http://localhost:5984", "comments"))
            {
                var query = new Query("comments", "all_roots");

                var r = await store.QueryAsync<Comment>(query);

                RootComments = new List<Comment>();
                foreach (var row in r)
                {
                    RootComments.Add(row.Value);
                }
            }

            return Page();
        }
    }
}