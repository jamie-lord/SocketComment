using MyCouch;
using SocketComment.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SocketComment
{
    public class CommentService : IDisposable
    {
        /// <summary>
        /// Returns a complete thread with the specified number of comments, default is all comments.
        /// </summary>
        /// <param name="rootCommentId"></param>
        /// <returns></returns>
        public async Task<Thread> GetThread(string rootCommentId, int maxCommentCount = int.MaxValue)
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

            thread.Children = GetChildComments(thread.Root, maxCommentCount);

            return thread;
        }

        /// <summary>
        /// Stores a single comment and returns the complete object as it would be found in the dataabase.
        /// </summary>
        /// <param name="comment"></param>
        /// <returns></returns>
        public async Task<Comment> StoreComment(Comment comment)
        {
            if (comment == null)
            {
                return null;
            }

            return await _commentStore.StoreAsync(comment);
        }

        //function(doc) {
        //    if (doc.$doctype == "comment" && doc.parent == null) {
        //        emit(null, doc);
        //    }
        //}

        /// <summary>
        /// Returns all comments that do not have a parent comment.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Comment> GetAllRootComments()
        {
            var query = new Query("comments", "all_roots");

            var result = _commentStore.QueryAsync<Comment>(query).Result;

            if (result == null)
            {
                yield break;
            }

            foreach (var c in result)
            {
                if (c.Value != null)
                {
                    yield return c.Value;
                }
            }

            yield break;
        }

        /// <summary>
        /// Returns comment parent id if available.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<string> DeleteThread(string id)
        {
            var rootComment = await _commentStore.GetByIdAsync<Comment>(id);

            if (rootComment == null)
            {
                return null;
            }

            var thread = new Thread()
            {
                Root = rootComment,
                Children = GetChildComments(rootComment, int.MaxValue)
            };

            foreach (var i in thread.AllChildIds)
            {
                await _commentStore.DeleteAsync(i);
            }

            return rootComment.Parent;
        }

        /// <summary>
        /// Remves author and message properties on comment and sets Deleted to true.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<string> ShadowDeleteComment(string id)
        {
            var comment = await _commentStore.GetByIdAsync<Comment>(id);
            if (comment == null)
            {
                return null;
            }

            comment.Author = null;
            comment.Message = null;
            comment.Deleted = true;

            comment = await _commentStore.StoreAsync(comment);
            return comment.Id;
        }

        /// <summary>
        /// Returns false if object not found.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<bool> DeleteComment(string id)
        {
            return await _commentStore.DeleteAsync(id);
        }

        private MyCouchStore _commentStore = new MyCouchStore("http://localhost:5984", "comments");

        // Returns comments sorted by created datetime
        //function(doc) {
        //    if (doc.$doctype == 'comment') {
        //        emit([doc.parent, doc.created], doc);
        //    }
        //}

        private IEnumerable<Thread> GetChildComments(Comment rootComment, int maxComments, int count = 0)
        {
            if (count > maxComments)
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
                        Children = GetChildComments(child.Value, maxComments, count)
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
