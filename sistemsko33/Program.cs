using System;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using NLog;
using NLog.Config;
using sistemsko2;

public class Program
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    public static async Task Main(string[] args)
    {
       
        string nlogConfigPath = @"C:\Users\Kristina\Desktop\sistemsko33\NLog.config";
        LogManager.Configuration = new XmlLoggingConfiguration(nlogConfigPath);
        Logger.Info("Starting web server...");

        var restaurantStream = new RestaurantStream();

        var observer1 = new RestaurantObserver("Observer 1");
        var subscription1 = restaurantStream.Subscribe(observer1);

        var reviewObserver = new ReviewObserver("ReviewObserver");
        var reviewSubscription = restaurantStream.Subscribe(reviewObserver);

        var listener = new HttpListener();
        listener.Prefixes.Add("http://localhost:8080/");
        listener.Start();

        Logger.Info("Listening for requests at http://localhost:8080/");

        while (true)
        {
            var context = await listener.GetContextAsync();
            var request = context.Request;
            var response = context.Response;

            try
            {
                string responseString = await HandleRequest(restaurantStream, request);
                var buffer = Encoding.UTF8.GetBytes(responseString);

                response.ContentLength64 = buffer.Length;
                var responseOutput = response.OutputStream;
                await responseOutput.WriteAsync(buffer, 0, buffer.Length);
                responseOutput.Close();

                Logger.Info($"Processed request: {request.Url}");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error processing request");
                response.StatusCode = 500;
                response.StatusDescription = "Internal Server Error";
                response.Close();
            }
        }
    }

    private static async Task<string> HandleRequest(RestaurantStream restaurantStream, HttpListenerRequest request)
    {
        var segments = request.Url.Segments;
        if (segments.Length == 2)
        {
            string segment = segments[1].TrimEnd('/');
            Logger.Info($"Handling request for segment: {segment}");

            // Check if the segment contains only letters and underscores (for location)
            if (segment.All(c => char.IsLetter(c) || c == '_'))
            {
                string location = segment.Replace("_", " ");
                try
                {
                    Console.WriteLine($"Fetching restaurants for location: {location}");
                    await restaurantStream.GetRestaurants(location);
                    return FormatRestaurantIdsAsHtml(restaurantStream.allRestaurants);
                }
                catch (Exception e)
                {
                    
                    Console.WriteLine($"Error fetching restaurants for location {location}: {e.Message}");
                    return "Error fetching restaurants. Please try again later.";
                }

            }
        }
        else if (segments.Length == 3 && segments[1].Trim('/') == "id")
        {
            string restaurantId = segments[2].Trim('/');
            Logger.Info($"Fetching reviews for restaurant ID: {restaurantId}");
            await restaurantStream.GetReviewsForRestaurant(restaurantId);
            if (restaurantStream.allReviews.Any())
            {
                var topics = TopicModeling.PerformTopicModeling(restaurantStream.allReviews);
                return FormatReviewsWithTopicsAsHtml(restaurantStream.allReviews, topics);
            }
            else
            {
                Logger.Warn($"No reviews found for restaurant ID: {restaurantId}");
                return "No reviews found.";
            }
        }
        else
        {
            Logger.Warn($"Invalid URL format: {request.Url}");
            return "Invalid endpoint or parameters.";
        }

        return "Invalid endpoint or parameters.";
    }

    private static string FormatRestaurantIdsAsHtml(List<Restaurant> restaurants)
    {
        if (restaurants == null || !restaurants.Any())
        {
            Logger.Warn("No restaurants found.");
            return "<html><body><h1>No Restaurants found</h1></body></html>";
        }

        var sb = new StringBuilder();
        sb.Append("<html><body><h1>Restaurant IDs and Names</h1><ul>");
        foreach (var restaurant in restaurants)
        {
            sb.AppendFormat("<li><strong>ID:</strong> {0}, <strong>Name:</strong> {1}</li>", restaurant.Id, restaurant.Name);
        }
        sb.Append("</ul></body></html>");
        return sb.ToString();
    }

    private static string FormatReviewsWithTopicsAsHtml(List<Review> reviews, List<float[]> topics)
    {
        if (reviews == null || !reviews.Any())
        {
            return "<html><body><h1>No Reviews found</h1></body></html>";
        }

        var sb = new StringBuilder();
        sb.Append("<html><body><h1>Reviews</h1><ul>");
        for (int i = 0; i < reviews.Count; i++)
        {
            sb.AppendFormat("<li><strong>Comment:</strong> {0}<br><strong>Topics:</strong> Food: {1}%, Service: {2}%</li>",
                reviews[i].Text,
                (topics[i][0] * 100).ToString("F2"),
                (topics[i][1] * 100).ToString("F2"));
        }
        sb.Append("</ul></body></html>");
        return sb.ToString();
    }
}
