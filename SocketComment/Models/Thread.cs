using System.Collections.Generic;

namespace SocketComment.Models
{
    public class Thread
    {
        public Comment Root { get; set; }

        public List<Thread> Children { get; set; }
    }
}
