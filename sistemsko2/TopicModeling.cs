using Microsoft.ML;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sistemsko2
{
    public class TopicModeling
    {
       public static void PerformTopicModeling(List<Review> reviews)
        {
            var mlContext = new MLContext();

            var documents = reviews.Select(r => new Document { Text = r.Text }).ToList();
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

}
public class Document
{
    public string Text { get; set; }
}

public class TransformedDocument : Document
{
    public float[] Topics { get; set; }
}
