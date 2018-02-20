using System;
using System.ComponentModel.DataAnnotations;

namespace SocketComment.Models
{
    public class Comment
    {
        public string Id { get; set; }

        [Required]
        public string Author { get; set; }

        public DateTime Created { get; set; }

        [Required]
        public string Message { get; set; }

        public string Parent { get; set; }
    }
}
