using Newtonsoft.Json;
using NLog;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;

namespace sistemsko2
{
    public class RestaurantStream : IObservable<Restaurant>, IObservable<Review>
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

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
            Logger.Info($"Getting reviews for restaurant ID: {restaurantId}");
            string apiKey = "2_q8j2LVao1x_I29nr04hZCpCB5pBrb_xHgah4cUtOdkMqvYi46Bz0w74falKDkJkcOBs6BYlNaop0RhGIk4YCHAm1l-TG-Z7VNQbTbKaKQm2oI-H41jhJXR22GcZHYx";  // Replace with your actual API key
            HttpClient client = new HttpClient();

            try
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

                //int offset = 1;
                //int limit = 20; // Yelp API's max limit per request is 20
               // bool moreReviewsAvailable = true;

               // allReviews.Clear(); 

               
                    var url = $"https://api.yelp.com/v3/businesses/{restaurantId}/reviews?"; 
                    Logger.Info($"Request URL: {url}");
                    var response = await client.GetAsync(url);
                    var content = await response.Content.ReadAsStringAsync();
                    Logger.Info($"Response Status Code: {response.StatusCode}");
                    Logger.Info($"Response content: {content}");

                    if (!response.IsSuccessStatusCode)
                    {
                        Logger.Error($"Failed to fetch reviews: {response.StatusCode}");
                        Logger.Error($"Response content: {content}");
                        
                    }

                    dynamic deserializedObject = JsonConvert.DeserializeObject<dynamic>(content);
                    var reviews = deserializedObject.reviews;

                    if (reviews == null || reviews.Count == 0)
                    {
                        Logger.Warn("No reviews found.");
                        
                    }

                    Logger.Info($"Number of reviews fetched: {reviews.Count}");
                    foreach (var review in reviews)
                    {
                        var newReview = new Review
                        {
                            Text = review.text,
                            Rating = review.rating.ToString()
                        };

                        allReviews.Add(newReview);
                        reviewSubject.OnNext(newReview);
                    }

                   
                 //   offset += reviews.Count;
                  //  moreReviewsAvailable = reviews.Count == limit;
                

                reviewSubject.OnCompleted();
                Logger.Info("Successfully retrieved reviews.");
            }
            catch (Exception e)
            {
                Logger.Error(e, "Error getting reviews.");
                reviewSubject.OnError(e);
            }
        }

        public async Task GetRestaurants(string location)
        {
            Logger.Info($"Getting restaurants for location: {location}");
            string apiKey = "2_q8j2LVao1x_I29nr04hZCpCB5pBrb_xHgah4cUtOdkMqvYi46Bz0w74falKDkJkcOBs6BYlNaop0RhGIk4YCHAm1l-TG-Z7VNQbTbKaKQm2oI-H41jhJXR22GcZHYx";  // Replace with your actual API key
            HttpClient client = new HttpClient();

            var url = $"https://api.yelp.com/v3/businesses/search?location={location}&categories=restaurants";
            Logger.Info($"Request URL: {url}");

            try
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
                var response = await client.GetAsync(url);
                var content = await response.Content.ReadAsStringAsync();
                Logger.Info($"Response Status Code: {response.StatusCode}");
                Logger.Info($"Response content: {content}");

                if (!response.IsSuccessStatusCode)
                {
                    Logger.Error($"Failed to fetch restaurants: {response.StatusCode}");
                    Logger.Error($"Response content: {content}");
                    return;
                }

                dynamic deserializedObject = JsonConvert.DeserializeObject<dynamic>(content);
                var businesses = deserializedObject.businesses;

                if (businesses == null || businesses.Count == 0)
                {
                    Logger.Warn("No businesses found.");
                    return;
                }

                allRestaurants.Clear(); // Resetujemo listu restorana pre svakog API poziva
                foreach (var business in businesses)
                {
                    Logger.Info($"Business found: {business.name}");
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
                Logger.Info("Successfully retrieved restaurants.");
            }
            catch (Exception e)
            {
                Logger.Error(e, "Error getting restaurants.");
                restaurantSubject.OnError(e);
            }
        }

        public IDisposable Subscribe(IObserver<Restaurant> observer)
        {
            return restaurantSubject
                .ObserveOn(Scheduler.Default)
                .SubscribeOn(Scheduler.ThreadPool)
                .Subscribe(observer);
        }

        public IDisposable Subscribe(IObserver<Review> observer)
        {
            return reviewSubject
                .ObserveOn(Scheduler.Default)
                .SubscribeOn(Scheduler.ThreadPool)
                .Subscribe(observer);
        }
    }
}