using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sistemsko
{
    public class CommentService
    {
        private readonly HttpClient client = new HttpClient();
        private readonly string apiKey = "YOUR_YELP_API_KEY"; // Zameni sa tvojim API ključem

        public async Task<IEnumerable<Comment>> FetchCommentsAsync(string restaurantId)
        {
            var url = $"https://api.yelp.com/v3/businesses/{restaurantId}/reviews";
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
           // var url = $"https://api.yelp.com/v3/businesses/{restaurantId}/reviews?api_key={apiKey}";??????


            var response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();

            var jsonResponse = JObject.Parse(content);
            var commentsJson = jsonResponse["reviews"];

            if (commentsJson == null)
            {
                return Enumerable.Empty<Comment>();
            }

            return commentsJson.Select(comment => new Comment
            {
                Id = (string)comment["id"],
                Text = (string)comment["text"],
                Rating = (int)comment["rating"],
                RestaurantId = restaurantId,
                TimeCreated = (DateTime)comment["time_created"]
            });
        }
    }
}
