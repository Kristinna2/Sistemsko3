﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            Console.WriteLine($"{name}: {restaurant.Id}!");
        }

        public void OnError(Exception e)
        {
            Console.WriteLine($"{name}: An error occurred: {e.Message}");
        }

        public void OnCompleted()
        {
            Console.WriteLine($"{name}: All restaurants successfully returned.");
        }
    }
}
