using System;
using System.ComponentModel.DataAnnotations;

namespace SocketComment.Models
{
    public class Comment
    {
        public string Id { get; set; }

        public string Rev { get; set; }

        [Required]
        public string Author { get; set; }

        public DateTime Created { get; set; }

        [Required]
        [DataType(DataType.MultilineText)]
        public string Message { get; set; }

        public string Parent { get; set; }

        public bool Deleted { get; set; }
    }
}
