using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace SocketComment.FrontEnd.Pages.Comment
{
    [Produces("application/json")]
    [Route("Comment")]
    public class CommentController : Controller
    {
        public CommentController(CommentService commentService)
        {
            _commentService = commentService;
        }

        private CommentService _commentService;

        // GET: api/Comment/5
        [HttpGet("{id}", Name = "Get")]
        public string Get(int id)
        {
            return "value";
        }

        // POST: api/Comment
        [HttpPost]
        public void Post([FromBody]string value)
        {
        }

        [HttpGet("DeleteThread/{id}", Name = "DeleteThread")]
        public async Task<IActionResult> DeleteThread(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return NotFound();
            }
            var parentId = await _commentService.DeleteThread(id);
            if (parentId != null)
            {
                return Redirect("/Thread/" + parentId);
            }
            return RedirectToPage("/Index");
        }

        [HttpGet("DeleteComment/{id}", Name = "DeleteComment")]
        public async Task<IActionResult> DeleteComment(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return NotFound();
            }

            var commentId = await _commentService.ShadowDeleteComment(id);

            return Redirect("/Thread/" + commentId);
        }
    }
}
