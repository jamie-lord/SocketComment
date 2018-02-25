using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SocketComment.Models;
using System.Threading.Tasks;

namespace SocketComment.FrontEnd.Pages
{
    public class ThreadModel : PageModel
    {
        public ThreadModel(CommentService commentService)
        {
            _commentService = commentService;
        }

        private CommentService _commentService;

        [BindProperty]
        public Thread Thread { get; set; }

        public async Task<IActionResult> OnGet(string id)
        {
            Thread = await _commentService.GetThread(id, 100);

            if (Thread == null)
            {
                return NotFound();
            }

            return Page();
        }
    }
}