using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SocketComment.Models;
using System;
using System.Threading.Tasks;

namespace SocketComment.FrontEnd.Pages
{
    [Route("create")]
    public class CreateModel : PageModel
    {
        public CreateModel(CommentService commentService)
        {
            _commentService = commentService;
        }

        private CommentService _commentService;

        [BindProperty]
        public Comment Comment { get; set; }

        public IActionResult OnGet(string parentId)
        {
            if (!string.IsNullOrWhiteSpace(parentId))
            {
                Comment = new Models.Comment
                {
                    Parent = parentId
                };
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            Comment.Created = DateTime.Now;

            if (!ModelState.IsValid)
            {
                return Page();
            }

            var resultComment = await _commentService.StoreComment(Comment);

            if (string.IsNullOrWhiteSpace(Comment.Parent))
            {
                return RedirectToPage("Index");
            }
            else
            {
                return Redirect("/Thread/" + Comment.Parent);
            }
        }
    }
}