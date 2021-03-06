﻿using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SocketComment.Models;

namespace SocketComment.FrontEnd.Pages
{
    public class EditModel : PageModel
    {
        public EditModel(CommentService commentService)
        {
            _commentService = commentService;
        }

        private CommentService _commentService;

        [BindProperty]
        public Comment Comment { get; set; }

        public async Task<IActionResult> OnGet(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return NotFound();
            }

            Comment = await _commentService.GetComment(id);

            if (Comment.Deleted == true)
            {
                return new BadRequestResult();
            }

            return Page();
        }

        public async Task<IActionResult> OnPost(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return new BadRequestResult();
            }

            var comment = await _commentService.GetComment(id);

            if (comment == null)
            {
                return NotFound();
            }

            Comment.LastEdit = DateTime.Now;
            Comment.Id = comment.Id;
            Comment.Rev = comment.Rev;
            Comment.Created = comment.Created;
            Comment.Deleted = comment.Deleted;

            if (!ModelState.IsValid)
            {
                return Page();
            }

            var resultComment = await _commentService.StoreComment(Comment);

            if (string.IsNullOrWhiteSpace(Comment.Id))
            {
                return NotFound();
            }
            else
            {
                return Redirect("/Thread/" + Comment.Id);
            }
        }
    }
}