using System;
using System.IO;
using System.Threading.Tasks;

namespace Client
{
    internal class RandomPayloadGenerator
    {
        public RandomPayloadGenerator(Random random)
        {
            rng = random;
        }

        public async Task Initialize()
        {
            if (lorumIpsum == null)
            {
                lorumIpsum = await File.ReadAllTextAsync("LorumIpsum.txt");
                halfSize = lorumIpsum.Length / 2;
            }
        }

        public string GetTextBlock()
        {
            int startIndex = rng.Next(halfSize);
            int endIndex = rng.Next(startIndex, lorumIpsum.Length);

            return lorumIpsum.Substring(startIndex, endIndex - startIndex);
        }

        private readonly Random rng;
        private static string lorumIpsum = null;
        private static int halfSize = 0;
    }
}
