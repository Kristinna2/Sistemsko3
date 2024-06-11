using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;

namespace Sistemsko
{
    public class CommentStream : IObservable<Comment>
    {
        private readonly Subject<Comment> commentSubject = new Subject<Comment>();
        private readonly CommentService commentService = new CommentService();

        public async Task GetCommentsAsync(string restaurantId)
        {
            try
            {
                var comments = await commentService.FetchCommentsAsync(restaurantId);
                foreach (var comment in comments)
                {
                    commentSubject.OnNext(comment);
                }
                commentSubject.OnCompleted();
            }
            catch (Exception ex)
            {
                commentSubject.OnError(ex);
            }
        }

        public IDisposable Subscribe(IObserver<Comment> observer)
        {
            return commentSubject.Subscribe(observer);
        }
    }
}
