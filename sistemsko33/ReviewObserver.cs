using sistemsko2;
using System;

namespace sistemsko2
{
    public class ReviewObserver : IObserver<Review>
    {
        private readonly string name;

        public ReviewObserver(string name)
        {
            this.name = name;
        }

        public void OnNext(Review review)
        {
           
            NLog.LogManager.GetCurrentClassLogger().Info($"{name}: Review: {review.Text}");
        }

        public void OnError(Exception e)
        {
            NLog.LogManager.GetCurrentClassLogger().Error($"{name}: An error occurred: {e.Message}");
        }

        public void OnCompleted()
        {
            NLog.LogManager.GetCurrentClassLogger().Info($"{name}: All reviews successfully returned.");
        }
    }
}
