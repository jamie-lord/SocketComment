using System.Collections.Generic;
using System.Linq;

namespace SocketComment.Models
{
    public class Thread
    {
        public Comment Root { get; set; }

        public IEnumerable<Thread> Children { get; set; }

        public int ChildCount
        {
            get
            {
                if (Children == null)
                {
                    return 0;
                }
                return Children.Count() + Children.Sum(c => c.ChildCount);
            }
        }

        public IEnumerable<string> AllChildIds
        {
            get
            {
                return Children.SelectMany(c => c.AllChildIds).Concat(new List<string> { Root.Id });
            }
        }
    }
}
