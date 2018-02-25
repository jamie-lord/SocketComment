using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace SocketComment.FrontEnd.Pages
{
    public class IndexModel : PageModel
    {
        public IndexModel(CommentService commentService)
        {
            _commentService = commentService;
        }

        private CommentService _commentService;

        [BindProperty]
        public IEnumerable<Models.Comment> RootComments { get; set; }

        public IActionResult OnGet()
        {
            RootComments = _commentService.GetAllRootComments();
            return Page();
        }
    }
}