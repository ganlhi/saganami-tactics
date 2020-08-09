using System;

namespace ST
{
    public enum Side
    {
        Aft,
        Forward,
        Port,
        Starboard,
        Top,
        Bottom,
    }

    public static class SideExtensions
    {
        public static string ToFriendlyString(this Side me)
        {
            switch (me)
            {
                case Side.Aft:
                    return "Aft";
                case Side.Forward:
                    return "Forward";
                case Side.Port:
                    return "Port";
                case Side.Starboard:
                    return "Starboard";
                case Side.Top:
                    return "Top";
                case Side.Bottom:
                    return "Bottom";
                default:
                    throw new ArgumentOutOfRangeException(nameof(me), me, null);
            }
        }
    }

    public static class SideHelper
    {
        public static Side FromString(string name)
        {
            switch (name)
            {
                case "Aft":
                    return Side.Aft;
                case "Forward":
                    return Side.Forward;
                case "Port":
                    return Side.Port;
                case "Starboard":
                    return Side.Starboard;
                case "Top":
                    return Side.Top;
                case "Bottom":
                    return Side.Bottom;
                default:
                    throw new ArgumentOutOfRangeException(nameof(name), name, "Unexpected side name");
            }
        }
    }
}