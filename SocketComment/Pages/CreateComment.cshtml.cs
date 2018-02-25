using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Threading.Tasks;

namespace SocketComment.Pages
{
    public class CreateCommentModel : PageModel
    {
        public CreateCommentModel(CommentService commentService)
        {
            _commentService = commentService;
        }

        private CommentService _commentService;

        [BindProperty]
        public Models.Comment Comment { get; set; }

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