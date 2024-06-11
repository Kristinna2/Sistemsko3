using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sistemsko
{
    public class CommentObserver : IObserver<Comment>
    {
        private readonly string name;
        public CommentObserver(string name)
        {
            this.name = name;
        }

        public void OnNext(Comment comment)
        {
            Console.WriteLine($"{name}: Comment: {comment.Text} -- Rating: {comment.Rating} -- Restaurant: {comment.RestaurantId}");
        }

        public void OnError(Exception e)
        {
            Console.WriteLine($"{name}: Error: {e.Message}");
        }

        public void OnCompleted()
        {
            Console.WriteLine($"{name}: All comments have been successfully retrieved.");
        }
    }
}
