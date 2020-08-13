using System;

namespace ST
{
    public enum ReportType
    {
        ShipDestroyed,
        ShipSurrendered,
        ShipDisengaged,
        MissilesMissed,
        MissilesStopped,
        MissilesHit,
        BeamsMiss,
        BeamsHit,
        DamageTaken,
        Info,
    }
    
    [Serializable]
    public struct Report
    {
        public ReportType type;
        public string message;
        public int turn;
    }
}