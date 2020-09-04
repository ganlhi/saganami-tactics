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

    public enum ReportSeverity
    {
        Danger,
        Warning,
        Info
    }
    
    [Serializable]
    public struct Report
    {
        public ReportType type;
        public string message;
        public int turn;

        public static ReportSeverity GetSeverity(ReportType type)
        {
            switch (type)
            {
                case ReportType.ShipDestroyed:
                case ReportType.DamageTaken:
                    return ReportSeverity.Danger;
                case ReportType.ShipSurrendered:
                case ReportType.MissilesHit:
                case ReportType.BeamsHit:
                    return ReportSeverity.Warning;
                case ReportType.ShipDisengaged:
                case ReportType.MissilesMissed:
                case ReportType.MissilesStopped:
                case ReportType.BeamsMiss:
                case ReportType.Info:
                    return ReportSeverity.Info;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}