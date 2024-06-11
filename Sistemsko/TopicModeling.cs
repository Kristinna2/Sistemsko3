using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SharpEntropy;
using SharpEntropy.IO;

namespace Sistemsko
{
    public class TopicModeling
    {
        private readonly GisModel model;

        public TopicModeling(string modelPath = "model.txt")
        {
            using (var reader = new StreamReader(modelPath))
            {
                model = new GisModel(new PlainTextGisModelReader(reader));
            }
        }

        public IEnumerable<string> GetTopics(IEnumerable<string> texts)
        {
            var topics = new List<string>();

            foreach (var text in texts)
            {
                var context = text.Split(' ');
                var bestOutcomeIndex = model.Evaluate(context).ToList().IndexOf(model.Evaluate(context).Max());
                topics.Add(model.GetOutcomeName(bestOutcomeIndex));
            }

            return topics;
        }
    }
}
