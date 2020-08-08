using System;
using ST.Scriptable;

namespace ST
{
    [Serializable]
    public struct SsdAlteration
    {
        public uint turn;
        public SsdAlterationStatus status;
        public Side side;
        public SsdAlterationType type;
        public HitLocationSlotType? slotType;
        public uint location;
    }

    public enum SsdAlterationStatus
    {
        Destroyed,
        Damaged,
        Fixed
    }

    public enum SsdAlterationType
    {
        Slot,
        Structural,
        Movement,
    }
}