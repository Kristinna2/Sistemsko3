using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sistemsko
{
    public class Comment
    {
        public string Id { get; set; }
        public string Text { get; set; }
        public int Rating { get; set; }
        public string RestaurantId { get; set; }
        public DateTime TimeCreated { get; set; }
    }
}
