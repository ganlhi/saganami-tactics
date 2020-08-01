using System;
using UnityEngine;

namespace ST
{
    public enum Team
    {
        Blue,
        Yellow,
        Green,
        Magenta,
    }

    public static class TeamExtensions
    {
        public static Color ToColor(this Team me)
        {
            switch (me)
            {
                case Team.Blue:
                    return Color.blue;
                case Team.Yellow:
                    return Color.yellow;
                case Team.Green:
                    return Color.green;
                case Team.Magenta:
                    return Color.magenta;
                default:
                    throw new ArgumentOutOfRangeException(nameof(me), me, null);
            }
        }

        public static int ToIndex(this Team me)
        {
            switch (me)
            {
                case Team.Blue:
                    return 1;
                case Team.Yellow:
                    return 2;
                case Team.Green:
                    return 3;
                case Team.Magenta:
                    return 4;
                default:
                    throw new ArgumentOutOfRangeException(nameof(me), me, null);
            }
        }
    }

    public static class TeamHelper
    {
        public static Team FromIndex(int index)
        {
            switch (index)
            {
                case 1: return Team.Blue;
                case 2: return Team.Yellow;
                case 3: return Team.Green;
                case 4: return Team.Magenta;
                default:
                    throw new ArgumentOutOfRangeException(nameof(index), "Team index out of range");
            }
        }
    }
}