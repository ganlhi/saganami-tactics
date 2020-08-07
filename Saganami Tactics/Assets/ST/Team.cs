using System;
using ST.Scriptable;
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
                    return GameSettings.Default.BlueTeam;
                case Team.Yellow:
                    return GameSettings.Default.YellowTeam;
                case Team.Green:
                    return GameSettings.Default.GreenTeam;
                case Team.Magenta:
                    return GameSettings.Default.MagentaTeam;
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