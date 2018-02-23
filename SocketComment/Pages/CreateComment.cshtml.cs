using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MyCouch;
using System;
using System.Threading.Tasks;

namespace SocketComment.Pages
{
    public class CreateCommentModel : PageModel
    {
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

            using (var store = new MyCouchStore("http://localhost:5984", "comments"))
            {
                var resultComment = await store.StoreAsync(Comment);
            }

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