using System;

namespace ST
{
    public enum TurnStep
    {
        Start,
        Plotting,
        Movement,
//        Targetting,
//        Missiles,
//        Beams,
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
                case TurnStep.End:
                    return "End";
                default:
                    throw new ArgumentOutOfRangeException(nameof(me), me, null);
            }
        }
    }
}