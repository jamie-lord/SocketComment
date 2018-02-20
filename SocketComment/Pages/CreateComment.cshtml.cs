using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MyCouch;
using SocketComment.Models;
using System;
using System.Threading.Tasks;

namespace SocketComment.Pages
{
    public class CreateCommentModel : PageModel
    {
        [BindProperty]
        public Comment Comment { get; set; }

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

            return RedirectToPage("/Index");
        }
    }
}