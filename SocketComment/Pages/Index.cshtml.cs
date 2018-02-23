using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MyCouch;

namespace SocketComment.Pages
{
    public class IndexModel : PageModel
    {
        [BindProperty]
        public List<Models.Comment> RootComments { get; set; }

        public async Task<IActionResult> OnGet()
        {
            //function(doc) {
            //    if (doc.$doctype == "comment" && doc.parent == null) {
            //        emit(null, doc);
            //    }
            //}

            using (var store = new MyCouchStore("http://localhost:5984", "comments"))
            {
                var query = new Query("comments", "all_roots");

                var r = await store.QueryAsync<Models.Comment>(query);

                RootComments = new List<Models.Comment>();
                foreach (var row in r)
                {
                    RootComments.Add(row.Value);
                }
            }

            return Page();
        }
    }
}