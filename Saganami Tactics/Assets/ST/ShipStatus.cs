using System;

namespace ST
{
    public enum ShipStatus
    {
        Ok,
        Destroyed,
        Surrendered,
        Disengaged,
    }
    
    public static class ShipStatusExtensions
    {
        public static int ToIndex(this ShipStatus me)
        {
            switch (me)
            {
                case ShipStatus.Ok:
                    return 0;
                case ShipStatus.Destroyed:
                    return 1;
                case ShipStatus.Surrendered:
                    return 2;
                case ShipStatus.Disengaged:
                    return 3;
                default:
                    throw new ArgumentOutOfRangeException(nameof(me), me, null);
            }
        }
    }

    public static class ShipStatusHelper
    {
        public static ShipStatus FromIndex(int index)
        {
            switch (index)
            {
                case 0: return ShipStatus.Ok;
                case 1: return ShipStatus.Destroyed;
                case 2: return ShipStatus.Surrendered;
                case 3: return ShipStatus.Disengaged;
                default:
                    throw new ArgumentOutOfRangeException(nameof(index), "ShipStatus index out of range");
            }
        }
    }
}