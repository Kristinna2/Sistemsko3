using Sistemsko;
using System;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;

public class Program
{
    public static async Task Main()
    {
        var commentStream = new CommentStream();

        var observer = new CommentObserver("Comment Observer");
        string modelPath = "model.txt";

        if (!System.IO.File.Exists(modelPath))
        {
            Console.WriteLine($"Model file not found: {modelPath}");
            return;
        }

      //  var topicModeling = new TopicModeling(modelPath);

        var keywordFilteredStream = commentStream
                                    .Where(comment => comment.Text.Contains("delicious"));

        var subscription = keywordFilteredStream.Subscribe(observer);

        // Pretpostavljamo da imamo neki restoran sa id "restaurant-id"
        string restaurantId = "restaurant-id"; // Zameni sa stvarnim ID-om restorana
        await commentStream.GetCommentsAsync(restaurantId);

        // Prikazivanje rezultata Topic Modeling-a
        var comments = await new CommentService().FetchCommentsAsync(restaurantId);
       // var topics = topicModeling.GetTopics(comments.Select(c => c.Text));

      /*  Console.WriteLine("Detected Topics:");
        foreach (var topic in topics)
        {
            Console.WriteLine(topic);
        }*/

        Console.ReadLine();
        subscription.Dispose();
    }
}
