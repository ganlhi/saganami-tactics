using System;

namespace ST
{
    public enum TurnStep
    {
        Start,
//        Plotting,
        Movement,
//        FirePlotting,
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