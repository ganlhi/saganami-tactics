using System;
using System.Collections.Generic;
using System.Linq;
using Random = UnityEngine.Random;

namespace ST
{
    public static class Dice
    {
        public static int[] D10s(int nb)
        {
            var results = new List<int>();
            for (var i = 0; i < nb; i++)
            {
                results.Add(fromRange(1, 10));
            }
            return results.ToArray();
        }

        public static int D10()
        {
            return D10s(1).First();
        }

        public static Tuple<int, bool> TwoD10Minus()
        {
            var rolls = D10s(2);
            return new Tuple<int, bool>(rolls.Max() - rolls.Min(), rolls.Sum() == 0);;
        }

        public static Tuple<int, bool>[] MultipleTwoD10Minus(int nb)
        {
            var results = new List<Tuple<int, bool>>();
            for (var i = 0; i < nb; i++)
            {
                results.Add(TwoD10Minus());
            }
            return results.ToArray();
        }

        private static int fromRange(int from, int to)
        {
            return Random.Range(from, to + 1);
        }
    }
}