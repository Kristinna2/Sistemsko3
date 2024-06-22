using sistemsko2;
using System;

namespace sistemsko2
{
    public class RestaurantObserver : IObserver<Restaurant>
    {
        private readonly string name;

        public RestaurantObserver(string name)
        {
            this.name = name;
        }

        public void OnNext(Restaurant restaurant)
        {
            
            NLog.LogManager.GetCurrentClassLogger().Info($"{name}: {restaurant.Id}");
        }

        public void OnError(Exception e)
        {
            NLog.LogManager.GetCurrentClassLogger().Error($"{name}: An error occurred: {e.Message}");
        }

        public void OnCompleted()
        {
            NLog.LogManager.GetCurrentClassLogger().Info($"{name}: All restaurants successfully returned.");
        }
    }
}
