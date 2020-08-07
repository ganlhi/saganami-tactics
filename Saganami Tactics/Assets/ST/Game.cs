using System;
using System.Collections.Generic;

namespace ST
{
    public enum GameEvent
    {
        UpdateShipsFutureMovement,
        MoveShipsToMarkers,
        PlaceShipsMarkers,
        ResetThrustAndPlottings,
    }
    
    public static class Game
    {
        public static void NextStep(int turn, TurnStep step, out int nextTurn, out TurnStep nextStep)
        {
            nextTurn = turn;

            switch (step)
            {
                case TurnStep.Start:
                    nextStep = TurnStep.Plotting;
                    break;
                case TurnStep.Plotting:
                    nextStep = TurnStep.Movement;
                    break;
                case TurnStep.Movement:
                    nextStep = TurnStep.End;
                    break;
                case TurnStep.End:
                    nextStep = TurnStep.Start;
                    nextTurn += 1;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static List<GameEvent> OnStepStart(int turn, TurnStep step)
        {
            var events = new List<GameEvent>();
            
            switch (step)
            {
                case TurnStep.Start:
                    if (turn == 1) events.Add(GameEvent.PlaceShipsMarkers);
                    break;
                
                case TurnStep.Plotting:
                    break;
                
                case TurnStep.Movement:
                    events.Add(GameEvent.MoveShipsToMarkers);
                    break;
                
                case TurnStep.End:
                    break;
                
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return events;
        }

        public static List<GameEvent> OnStepEnd(int turn, TurnStep step)
        {
            var events = new List<GameEvent>();
            
            switch (step)
            {
                case TurnStep.Start:
                    break;
                
                case TurnStep.Plotting:
                    // TODO broadcast displacement?
                    break;

                case TurnStep.Movement:
                    events.Add(GameEvent.UpdateShipsFutureMovement);
                    events.Add(GameEvent.ResetThrustAndPlottings);
                    break;

                case TurnStep.End:
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }

            return events;
        }
    }
}