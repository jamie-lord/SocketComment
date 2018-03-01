using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace SocketComment.FrontEnd.Controllers
{
    [Produces("application/json")]
    [Route("api/thread")]
    public class ThreadController : Controller
    {
        public ThreadController(CommentService commentService)
        {
            _commentService = commentService;
        }

        private CommentService _commentService;

        [HttpGet("{id}")]
        public async Task<Models.Thread> Get(string id, int? count)
        {
            return await _commentService.GetThread(id, count ?? int.MaxValue);
        }

        [HttpGet("delete/{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return NotFound();
            }
            var parentId = await _commentService.DeleteThread(id);
            if (parentId != null)
            {
                return Redirect("/thread/" + parentId);
            }
            return RedirectToPage("/index");
        }
    }
}