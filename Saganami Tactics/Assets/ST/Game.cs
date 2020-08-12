using System;
using System.Collections.Generic;
using System.Linq;
using ST.Common;
using ST.Scriptable;
using UnityEngine;

namespace ST
{
    public enum GameEvent
    {
        UpdateShipsFutureMovement,
        MoveShipsToMarkers,
        PlaceShipsMarkers,
        ResetThrustAndPlottings,
        IdentifyTargets,
        ClearTargets,
        FireMissiles,
        UpdateMissiles,
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
                    nextStep = TurnStep.Targeting;
                    break;
                case TurnStep.Targeting:
                    nextStep = TurnStep.Missiles;
                    break;
                case TurnStep.Missiles:
                    nextStep = TurnStep.Beams;
                    break;
                case TurnStep.Beams:
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

                case TurnStep.Targeting:
                    events.Add(GameEvent.IdentifyTargets);
                    break;

                case TurnStep.Missiles:
                    events.Add(GameEvent.FireMissiles);
                    events.Add(GameEvent.UpdateMissiles);
                    break;

                case TurnStep.Beams:
                    break;

                case TurnStep.End:
                    events.Add(GameEvent.ClearTargets);
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

                case TurnStep.Targeting:
                    break;

                case TurnStep.Missiles:
                    break;

                case TurnStep.Beams:
                    break;

                case TurnStep.End:
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }

            return events;
        }

        public static List<TargettingContext> IdentifyTargets(Ship attacker, List<Ship> allShips)
        {
            var targets = new List<TargettingContext>();

            var ennemyShips = allShips.Where(s => s.team != attacker.team);

            foreach (var ennemyShip in ennemyShips)
            {
                var targetingContexts = new List<TargettingContext>();

                targets.AddRange(TryTargetWithWeaponType(attacker, ennemyShip, WeaponType.Missile));
                targets.AddRange(TryTargetWithWeaponType(attacker, ennemyShip, WeaponType.Laser));
            }

            return targets;
        }

        private static List<TargettingContext> TryTargetWithWeaponType(Ship attacker, Ship target, WeaponType type)
        {
            var targettingContexts = new List<TargettingContext>();

            Vector3 targetPos;
            HitLocationSlotType slotType;
            switch (type)
            {
                case WeaponType.Missile:
                    targetPos = target.endMarkerPosition;
                    slotType = HitLocationSlotType.Missile;
                    break;
                case WeaponType.Laser:
                    targetPos = target.position;
                    slotType = HitLocationSlotType.Laser;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }

            var (mainBearing, _) = attacker.GetBearingTo(targetPos);
            if (mainBearing == Side.Bottom || mainBearing == Side.Top) return targettingContexts;

            var distance = attacker.position.DistanceTo(targetPos);

            var mounts = attacker.Ssd.weaponMounts.Where(m => m.side == mainBearing).ToArray();

            foreach (var mount in mounts)
            {
                if (mount.model.type != type) continue;

                var nbWeapons = SsdHelper.GetUndamagedValue(mount.weapons,
                    attacker.alterations.Where(a =>
                        a.side == mainBearing && a.type == SsdAlterationType.Slot && a.slotType == slotType));

                if (nbWeapons == 0 || (type == WeaponType.Missile && mount.ammo == 0)) continue;

                if (mount.model.GetMaxRange() < distance) continue;

                targettingContexts.Add(new TargettingContext()
                {
                    Mount = mount,
                    Side = mainBearing,
                    Target = target,
                    LaunchPoint = attacker.position,
                    LaunchDistance = distance,
                    Number = type == WeaponType.Missile
                        ? Math.Min((int) nbWeapons, mount.ammo)
                        : (int) nbWeapons
                });
            }

            return targettingContexts;
        }

        public static Missile UpdateMissile(Missile missile, Ship target)
        {
            switch (missile.status)
            {
                case MissileStatus.Launched:
                    missile.status = MissileStatus.Accelerating;
                    missile.nextMovePosition =
                        missile.launchPoint + .5f * (target.endMarkerPosition - missile.launchPoint);
                    break;
                case MissileStatus.Accelerating:
//                    if (!CanStillCatchTarget() || !CanTrackTarget())
//                    {
//                        missile.status = MissileStatus.Missed;
//                    }
//                    else if (!CanPassActiveDefenses())
//                    {
//                        missile.status = MissileStatus.Destroyed;
//                    }
//                    else
//                    {
                        missile.status = MissileStatus.Hitting;
                        missile.nextMovePosition = target.position;
//                        MakeReportToTarget(ReportType.MissilesHit,
//                            "Missiles from " + TargetData.Attacker.Name + ": " + TargetData.Missiles + " hits");
//                    }

                    break;
            }

            return missile;
        }
    }
}