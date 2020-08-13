using System;

namespace ST
{
    public enum TurnStep
    {
        Start,
        Plotting,
        Movement,
        Targeting,
        MissilesUpdates,
        Missiles,
        Beams,
//        CrewActions,
        End
    }
    
    public static class TurnStepExtensions
    {
        public static string ToFriendlyString(this TurnStep me)
        {
            switch(me)
            {
                case TurnStep.Start:
                    return "Start";
                case TurnStep.Plotting:
                    return "Plotting";
                case TurnStep.Movement:
                    return "Movement";
                case TurnStep.Targeting:
                    return "Targeting";
                case TurnStep.End:
                    return "End";
                case TurnStep.MissilesUpdates:
                    return "Missiles";
                case TurnStep.Missiles:
                    return "Missiles";
                case TurnStep.Beams:
                    return "Beams";
                default:
                    throw new ArgumentOutOfRangeException(nameof(me), me, null);
            }
        }
    }
}