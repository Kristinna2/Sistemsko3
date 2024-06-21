using Microsoft.ML;
using Microsoft.ML.Data;
using Newtonsoft.Json;
using sistemsko2;
using System.Net.Http.Headers;
using System.Reactive.Subjects;
using System.Xml.Linq;

public class Program
{
    public async static Task Main()
    {
        var restaurantStream = new RestaurantStream();

        var observer1 = new RestaurantObserver("Observer 1");
        //var observer2 = new RestaurantObserver("Observer 2");
        //var observer3 = new RestaurantObserver("Observer 3");

        var subscription1 = restaurantStream.Subscribe(observer1);
        //var subscription2 = restaurantStream.Subscribe(observer2);
        //var subscription3 = restaurantStream.Subscribe(observer3);

        var reviewObserver = new ReviewObserver("ReviewObserver");
        var reviewSubscription = restaurantStream.Subscribe(reviewObserver);

        string location;
        Console.WriteLine("Enter location:");
        location = System.Console.ReadLine()!;

        try
        {
            await restaurantStream.GetRestaurants(location);

            if (restaurantStream.allRestaurants.Count == 0)
            {
                Console.WriteLine("No restaurants found for the given location.");
                return;
            }

            Console.WriteLine("We have: " + restaurantStream.allRestaurants.Count + " restaurants\n");

            string restaurantId;
            Console.WriteLine("Enter restaurant ID to get reviews:");
            restaurantId = System.Console.ReadLine()!;
            var restaurant = restaurantStream.allRestaurants.FirstOrDefault(r => r.Id == restaurantId);

            if (restaurant != null)
            {
                Console.WriteLine($"Restaurant Name: {restaurant.Name}");
            }
            else
            {
                Console.WriteLine("Restaurant not found or ID is incorrect.");
                return;
            }

            await restaurantStream.GetReviewsForRestaurant(restaurantId);

            if (restaurantStream.allReviews.Count == 0)
            {
                Console.WriteLine("No reviews found for the given restaurant ID.");
                return;
            }

            var reviews = restaurantStream.allReviews.Select(r => new Document { Text = r.Text }).ToList();
            //PerformTopicModeling(reviews);
            TopicModeling.PerformTopicModeling(restaurantStream.allReviews);


        }
        catch (Exception e)
        {
            Console.WriteLine("Došlo je do greške: " + e.Message);
        }
        finally
        {
            subscription1.Dispose();
            reviewSubscription.Dispose();
            //subscription2.Dispose();
            //subscription3.Dispose();
        }
    }
}
   

