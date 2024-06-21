using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;

namespace sistemsko2
{
    public class RestaurantStream : IObservable<Restaurant>
    {
        private readonly Subject<Restaurant> restaurantSubject;
        private readonly Subject<Review> reviewSubject;

        public List<Review> allReviews = new List<Review>();
        public List<Restaurant> allRestaurants = new List<Restaurant>();

        public RestaurantStream()
        {
            restaurantSubject = new Subject<Restaurant>();
            reviewSubject = new Subject<Review>();
        }

        public async Task GetReviewsForRestaurant(string restaurantId)
        {
            string apiKey = "2_q8j2LVao1x_I29nr04hZCpCB5pBrb_xHgah4cUtOdkMqvYi46Bz0w74falKDkJkcOBs6BYlNaop0RhGIk4YCHAm1l-TG-Z7VNQbTbKaKQm2oI-H41jhJXR22GcZHYx";
            HttpClient client = new HttpClient();

            try
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

                var url = $"https://api.yelp.com/v3/businesses/{restaurantId}/reviews";
                var response = await client.GetAsync(url);
                var content = await response.Content.ReadAsStringAsync();

                response.EnsureSuccessStatusCode();

                dynamic deserializedObject = JsonConvert.DeserializeObject<dynamic>(content);
                var reviews = deserializedObject.reviews;

                if (reviews == null)
                {
                    Console.WriteLine("No reviews found.");
                    return;
                }

                foreach (var review in reviews)
                {
                    Console.WriteLine(review.text);
                    var newReview = new Review
                    {
                        Text = review.text,
                        Rating = review.rating.ToString()
                    };

                    allReviews.Add(newReview);
                    reviewSubject.OnNext(newReview);
                }
                reviewSubject.OnCompleted();
            }
            catch (Exception e)
            {
                Console.WriteLine("Došlo je do greške: " + e.Message);
                reviewSubject.OnError(e);
            }
        }

        public async Task GetRestaurants(string location)
        {
            Console.WriteLine("....GETTING RESTAURANTS....");
            string apiKey = "nc7_TwT-BLJtkQuUPEq8CfTD7bMgNBBCahFxIZADrNDPZouqEiPETQLOceDxSujICL8TgpaKZGO5a04YkDYZyZbrWiR59vBZs_invIMotfJNJzcID5DYKcLUL5psZnYx";
            HttpClient client = new HttpClient();

            var url = $"https://api.yelp.com/v3/businesses/search?location={location}&categories=restaurants";

            try
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
                var response = await client.GetAsync(url);
                var content = await response.Content.ReadAsStringAsync();

                response.EnsureSuccessStatusCode();

                dynamic deserializedObject = JsonConvert.DeserializeObject<dynamic>(content);
                var businesses = deserializedObject.businesses;

                if (businesses == null)
                {
                    Console.WriteLine("No businesses found.");
                    return;
                }

                foreach (var business in businesses)
                {
                    Console.WriteLine("Business found.");
                    var newRestaurant = new Restaurant
                    {
                        Id = business.id,
                        Alias = business.alias,
                        Rating = business.rating,
                        Name = business.name
                    };
                    allRestaurants.Add(newRestaurant);
                    restaurantSubject.OnNext(newRestaurant);
                }
                restaurantSubject.OnCompleted();
            }
            catch (Exception e)
            {
                Console.WriteLine("Došlo je do greške: " + e.Message);
                restaurantSubject.OnError(e);
            }
        }

        public IDisposable Subscribe(IObserver<Restaurant> observer)
        {
            return restaurantSubject.Subscribe(observer);
        }

        public IDisposable Subscribe(IObserver<Review> observer)
        {
            return reviewSubject.Subscribe(observer);
        }
    }
}


