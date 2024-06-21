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

            
            PerformTopicModeling(reviews);
        }
        catch (Exception e)
        {
            Console.WriteLine("Došlo je do greške: " + e.Message);
        }
        finally
        {
            subscription1.Dispose();
            //subscription2.Dispose();
            //subscription3.Dispose();
        }
    }

    static void PerformTopicModeling(List<Document> documents)
    {
        var mlContext = new MLContext();

        var data = mlContext.Data.LoadFromEnumerable(documents);

     
        var textPipeline = mlContext.Transforms.Text
            .NormalizeText("NormalizedText", "Text")
            .Append(mlContext.Transforms.Text.TokenizeIntoWords("Tokens", "NormalizedText"))
            .Append(mlContext.Transforms.Text.RemoveDefaultStopWords("Tokens")) // Uklanjanje engleskih stop reči
            .Append(mlContext.Transforms.Text.ProduceWordBags("BagOfWords", "Tokens"))
            .Append(mlContext.Transforms.Text.LatentDirichletAllocation("Topics", "BagOfWords", numberOfTopics: 2));

        // Treniranje modela
        var model = textPipeline.Fit(data);

        // Transformisanje podataka
        var transformedData = model.Transform(data);

        // Prikazivanje tema
        var topics = mlContext.Data.CreateEnumerable<TransformedDocument>(transformedData, reuseRowObject: false);

        foreach (var doc in topics)
        {
            Console.WriteLine($"Comment: {doc.Text}");
            Console.WriteLine("Topics:");

            for (int i = 0; i < doc.Topics.Length; i++)
            {
                string topicName = i == 0 ? "Food" : "Service"; // Prilagoditi nazive temama kako odgovaraju analiziranom sadržaju

                Console.WriteLine($"  Topic {topicName}: {doc.Topics[i]}");
            }

            Console.WriteLine();
        }
    }
}

public class Document
{
    public string Text { get; set; }
}

public class TransformedDocument : Document
{
    public float[] Topics { get; set; }
}
