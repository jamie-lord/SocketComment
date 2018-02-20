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
        public Comment Comment { get; set; }

        public async Task<IActionResult> OnGet(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return NotFound();
            }
            
            using (var store = new MyCouchStore("http://localhost:5984", "comments"))
            {
                Comment = await store.GetByIdAsync<Comment>(id);
            }

            return Page();
        }
    }
}