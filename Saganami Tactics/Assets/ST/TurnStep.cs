using System;

namespace ST
{
    public enum TurnStep
    {
        Plotting,
        Movement,
        Targeting,
        MissilesUpdates,
        Missiles,
        Beams,
        CrewActions,
    }
    
    public static class TurnStepExtensions
    {
        public static string ToFriendlyString(this TurnStep me)
        {
            switch(me)
            {
                case TurnStep.Plotting:
                    return "Plotting";
                case TurnStep.Movement:
                    return "Movement";
                case TurnStep.Targeting:
                    return "Targeting";
                case TurnStep.MissilesUpdates:
                    return "Missiles";
                case TurnStep.Missiles:
                    return "Missiles";
                case TurnStep.Beams:
                    return "Beams";
                case TurnStep.CrewActions:
                    return "Crew actions";
                default:
                    throw new ArgumentOutOfRangeException(nameof(me), me, null);
            }
        }
    }
}