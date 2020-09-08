using System;
using ST.Scriptable;

namespace ST
{
    [Serializable]
    public struct SsdAlteration
    {
        public bool destroyed;
        public Side side;
        public SsdAlterationType type;
        public HitLocationSlotType slotType;
        public int location;
    }

    public enum SsdAlterationType
    {
        Slot,
        Structural,
        Movement,
        Sidewall,
    }
}