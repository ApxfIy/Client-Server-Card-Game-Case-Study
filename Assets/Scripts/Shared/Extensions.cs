using System;
using System.Collections.Generic;

namespace WarGame.Shared
{
    public static class Extensions
    {
        public static string RankToString(this CardRank rank) => rank switch
        {
            CardRank.Ace   => "A",
            CardRank.King  => "K",
            CardRank.Queen => "Q",
            CardRank.Jack  => "J",
            CardRank.Ten   => "10",
            _              => ((int)rank).ToString()
        };
        
        private static readonly Random Rng = new Random();

        // Source - https://stackoverflow.com/questions/273313/randomize-a-listt
        public static void Shuffle<T>(this IList<T> list)
        {
            var n = list.Count;
            while (n > 1) {
                n--;
                var k = Rng.Next(n + 1);
                (list[k], list[n]) = (list[n], list[k]);
            }
        }

        public static T GetRandomItem<T>(this IList<T> list)
        {
            return list[Rng.Next(list.Count)];
        }
    }
}