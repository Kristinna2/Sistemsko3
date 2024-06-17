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
        var observer2 = new RestaurantObserver("Observer 2");
        var observer3 = new RestaurantObserver("Observer 3");

        var subscription1 = restaurantStream.Subscribe(observer1);
        var subscription2 = restaurantStream.Subscribe(observer2);
        var subscription3 = restaurantStream.Subscribe(observer3);

        string location;
        Console.WriteLine("Enter location:");
        location = System.Console.ReadLine()!;
        await restaurantStream.GetRestaurants(location);

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
        }
        await restaurantStream.GetReviewsForRestaurant(restaurantId);


        var reviews = restaurantStream.allReviews.Select(r => new Document { Text = r.Text }).ToList();

        // Perform topic modeling
        PerformTopicModeling(reviews);

        subscription1.Dispose();
        subscription2.Dispose();
        subscription3.Dispose();
    }

    static void PerformTopicModeling(List<Document> documents)
    {
        var mlContext = new MLContext();

        var data = mlContext.Data.LoadFromEnumerable(documents);

        // Lista srpskih stop reči
        var srpskeStopReci = new[] { "i", "a", "ali", "pa", "u", "na", "je", "da", "se", "za", "sa", "su", "koji", "koja", "koje", "od", "do", "s", "bez", "iz", "pre", "posle", "kroz", "ili", "o", "kao", "veoma", "mnogo" };

        // Kreiranje text processing pipeline-a
        var textPipeline = mlContext.Transforms.Text
            .NormalizeText("NormalizedText", "Text")
            .Append(mlContext.Transforms.Text.TokenizeIntoWords("Tokens", "NormalizedText"))
            .Append(mlContext.Transforms.Text.RemoveDefaultStopWords("Tokens")) // Uklanjanje engleskih stop reči
            .Append(mlContext.Transforms.Text.RemoveStopWords("Tokens", stopwords: srpskeStopReci)) // Uklanjanje srpskih stop reči
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
                string topicName = i == 0 ? "Restorani" : "Uslužnost"; // Prilagoditi nazive temama kako odgovaraju analiziranom sadržaju

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
