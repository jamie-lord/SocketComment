using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using CacheManager.Core;
using MyCouch;
using SocketComment.Models;

namespace SocketComment
{
    public class CommentService : IDisposable
    {
        private const string USERNAME = "CommentService";
        private const string PASSWORD = "CommentService";
        private const string SERVER = "http://" + USERNAME + ":" + PASSWORD + "@localhost:5984";
        private const string DATABASE = "comments";

        private readonly MyCouchStore _commentStore;
        private readonly ICacheManager<Comment> _commentCache;

        public CommentService()
        {
            using (var client = new MyCouchServerClient(SERVER))
            {
                var d = client.Databases.PutAsync(DATABASE).Result;

                if (!client.Databases.HeadAsync(DATABASE).Result.IsSuccess)
                {
                    throw new Exception($"Database '{DATABASE}' not found!");
                }
            }

            using (var client = new MyCouchClient(SERVER, DATABASE))
            {
                if (!client.Documents.HeadAsync("_design/comments").Result.IsSuccess)
                {
                    var assembly = Assembly.GetExecutingAssembly();
                    var docName = "SocketComment.DesignDocuments.Comments.json";
                    using (Stream stream = assembly.GetManifestResourceStream(docName))
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        string docString = reader.ReadToEnd();

                        if (string.IsNullOrWhiteSpace(docString))
                        {
                            throw new Exception("Failed to load document json file");
                        }

                        var doc = client.Documents.PostAsync(docString).Result;

                        if (!doc.IsSuccess)
                        {
                            throw new Exception("Failed to setup default view document");
                        }
                    }
                }
            }

            _commentStore = new MyCouchStore(SERVER, DATABASE);
            _commentCache = CacheFactory.Build<Comment>(settings =>
            {
                settings.WithMicrosoftMemoryCacheHandle();
            });
        }

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

            var rootComment = await GetComment(rootCommentId);

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

        public async Task<Comment> GetComment(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return null;
            }
            var comment = _commentCache.Get(id);
            if (comment == null)
            {
                comment = await _commentStore.GetByIdAsync<Comment>(id);
                if (comment != null)
                {
                    _commentCache.Add(comment.Id, comment);
                }
            }
            return comment;
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
            comment = await _commentStore.StoreAsync(comment);
            _commentCache.Put(comment.Id, comment);
            return comment;
        }

        /// <summary>
        /// Returns all comments that do not have a parent comment.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Comment> GetAllRootComments()
        {
            var query = new Query("comments", "all_roots");

            var result = _commentStore.QueryAsync(query).Result;

            if (result == null)
            {
                yield break;
            }

            foreach (var c in result)
            {
                if (c.Id != null)
                {
                    yield return GetComment(c.Id).Result;
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
            var rootComment = await GetComment(id);

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
                await DeleteComment(i);
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
            var comment = await GetComment(id);
            if (comment == null)
            {
                return null;
            }

            comment.Author = null;
            comment.Message = null;
            comment.Deleted = true;
            comment.LastEdit = null;

            comment = await StoreComment(comment);
            return comment.Id;
        }

        /// <summary>
        /// Returns false if object not found.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<bool> DeleteComment(string id)
        {
            var success = await _commentStore.DeleteAsync(id);
            if (success)
            {
                _commentCache.Remove(id);
            }

            return success;
        }

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

            var children = _commentStore.QueryAsync(query).Result;

            foreach (var child in children)
            {
                if (child.Id != null)
                {
                    var comment = GetComment(child.Id).Result;
                    if (comment == null)
                    {
                        continue;
                    }

                    var thread = new Thread()
                    {
                        Root = comment,
                        Children = GetChildComments(comment, maxComments, count)
                    };
                    yield return thread;
                }
            }
        }

        public void Dispose()
        {
            _commentStore.Dispose();
            _commentCache.Dispose();
        }
    }
}
