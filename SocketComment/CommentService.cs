using MyCouch;
using SocketComment.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SocketComment
{
    public class CommentService : IDisposable
    {
        public async Task<Thread> GetThread(string rootCommentId)
        {
            if (string.IsNullOrWhiteSpace(rootCommentId))
            {
                return null;
            }

            var rootComment = await _commentStore.GetByIdAsync<Comment>(rootCommentId);

            if (rootComment == null)
            {
                return null;
            }

            Thread thread = new Thread()
            {
                Root = rootComment
            };

            thread.Children = GetChildComments(thread.Root);

            return thread;
        }

        public async Task<Comment> StoreComment(Comment comment)
        {
            if (comment == null)
            {
                return null;
            }

            return await _commentStore.StoreAsync(comment);
        }

        private MyCouchStore _commentStore = new MyCouchStore("http://localhost:5984", "comments");

        private const int MAX_COMMENTS = 100;

        private IEnumerable<Thread> GetChildComments(Comment rootComment, int count = 0)
        {
            // Returns comments sorted by created datetime
            //function(doc) {
            //    if (doc.$doctype == 'comment') {
            //        emit([doc.parent, doc.created], doc);
            //    }
            //}

            if (count > MAX_COMMENTS)
            {
                yield break;
            }

            count++;

            var query = new Query("comments", "children")
            {
                StartKey = new object[] { rootComment.Id },
                EndKey = new object[] { rootComment.Id, new object() }
            };

            var children = _commentStore.QueryAsync<Comment>(query).Result;

            foreach (var child in children)
            {
                if (child.Value != null)
                {
                    var thread = new Thread()
                    {
                        Root = child.Value,
                        Children = GetChildComments(child.Value, count)
                    };
                    yield return thread;
                }
            }
        }

        public void Dispose()
        {
            _commentStore.Dispose();
        }
    }
}
