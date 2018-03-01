using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace SocketComment.FrontEnd.Controllers
{
    [Produces("application/json")]
    [Route("api/comment")]
    public class CommentController : Controller
    {
        public CommentController(CommentService commentService)
        {
            _commentService = commentService;
        }

        private CommentService _commentService;

        [HttpGet("{id}")]
        public async Task<Models.Comment> Get(string id)
        {
            return await _commentService.GetComment(id);
        }

        [HttpGet("delete/{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return NotFound();
            }

            var commentId = await _commentService.ShadowDeleteComment(id);

            return Redirect("/thread/" + commentId);
        }
    }
}
