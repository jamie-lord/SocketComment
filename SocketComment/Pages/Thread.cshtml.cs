using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MyCouch;
using SocketComment.Models;
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

            }

            if (Thread == null)
            {
                return NotFound();
            }

            return Page();
        }
    }
}