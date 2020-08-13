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
        MoveMissiles,
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
                    nextStep = TurnStep.MissilesUpdates;
                    break;
                case TurnStep.MissilesUpdates:
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

                case TurnStep.MissilesUpdates:
                    events.Add(GameEvent.FireMissiles);
                    events.Add(GameEvent.UpdateMissiles);
                    break;

                case TurnStep.Missiles:
                    events.Add(GameEvent.MoveMissiles);
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
//                    events.Add(GameEvent.PlaceShipsMarkers);
                    break;

                case TurnStep.Movement:
                    events.Add(GameEvent.UpdateShipsFutureMovement);
                    events.Add(GameEvent.ResetThrustAndPlottings);
                    break;

                case TurnStep.Targeting:
                    break;

                case TurnStep.MissilesUpdates:
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

        public static Missile UpdateMissile(
            Missile missile,
            Ship attacker,
            Ship target,
            int turn,
            out List<Tuple<ReportType, string>> reports)
        {
            reports = new List<Tuple<ReportType, string>>();

            switch (missile.status)
            {
                case MissileStatus.Launched:
                    missile.status = MissileStatus.Accelerating;
                    missile.nextMovePosition =
                        missile.launchPoint + .5f * (target.endMarkerPosition - missile.launchPoint);
                    break;
                case MissileStatus.Accelerating:
                    if (!CanStillCatchTarget(missile, attacker, target, out var r))
                    {
                        reports.AddRange(r);
                        missile.status = MissileStatus.Missed;
                    }
//                    else if (!CanTrackTarget())
                    //                    {
                    //                        missile.status = MissileStatus.Missed;
                    //                    }
                    //                    else if (!CanPassActiveDefenses())
                    //                    {
                    //                        missile.status = MissileStatus.Destroyed;
                    //                    }
                    else
                    {
                        missile.status = MissileStatus.Hitting;
                        missile.nextMovePosition = target.position;
                        reports.Add(new Tuple<ReportType, string>(ReportType.MissilesHit,
                            "Missiles from " + attacker.name + ": " + missile.number + " hits"));
                    }

                    break;
            }

            missile.updatedAtTurn = turn;

            return missile;
        }


        private static bool CanStillCatchTarget(Missile missile, Ship attacker, Ship target,
            out List<Tuple<ReportType, string>> reports)
        {
            reports = new List<Tuple<ReportType, string>>();

            if (target.Status != ShipStatus.Ok)
            {
                reports.Add(new Tuple<ReportType, string>(ReportType.MissilesMissed,
                    "Missiles from " + attacker.name + " lost their target"));
                return false;
            }

            var totalRange = missile.launchPoint.DistanceTo(target.position);
            var maxRange = missile.weapon.GetMaxRange();

            if (maxRange >= totalRange)
            {
                return true;
            }

            reports.Add(new Tuple<ReportType, string>(ReportType.MissilesMissed,
                "Missiles from " + attacker.name + " are out of range"));
            return false;
        }

//        private static bool CanTrackTarget()
//        {
//            var activeEcm = TargetData.Target.SSD.ECM;
//            var activeEccm = TargetData.Attacker.SSD.ECCM;
//
//            if (TargetData.Attacker.AttemptCrewRateCheck())
//            {
//                activeEcm = Math.Max(0, activeEcm - activeEccm);
//            }
//
//            var totalRange = Mathf.CeilToInt(TargetData.LaunchPoint.DistanceTo(TargetData.Target.transform.position));
//            var rangeBand = TargetData.Weapon.GetRangeBand(totalRange);
//
//            if (!rangeBand.HasValue)
//            {
//                // Should not happen, but let's consider this case a miss
//                MakeReportToTarget(ReportType.MissilesMissed,
//                    "Missiles from " + TargetData.Attacker.Name + " are out of range");
//                return false;
//            }
//
//            var accuracy = rangeBand.Value.accuracy + activeEcm;
//            var diceRolls = Dice.D10s(TargetData.Missiles);
//            Debug.Log("Rolled against " + accuracy + "+ accuracy: " + string.Join(", ", diceRolls));
//            var successes = diceRolls.Count(r => r >= accuracy);
//            Debug.Log("Successes: " + successes);
//            if (successes == 0)
//            {
//                MakeReportToTarget(ReportType.MissilesMissed,
//                    "Missiles from " + TargetData.Attacker.Name + " lost their target");
//                return false;
//            }
//
//            TargetData.Missiles = successes;
//
//            return true;
//        }
//
//        private static bool CanPassActiveDefenses()
//        {
//            var defenseBearing = Bearing.Compute(TargetData.Target.transform, TargetData.LaunchPoint);
//
//            var cm = defenseBearing.Wedge.HasValue ? 0 : TargetData.Target.SSD.CM(defenseBearing.Side);
//            var pd = TargetData.Target.SSD.PD(defenseBearing.Side);
//            var activeDefenses = cm + pd;
//            Debug.Log("Active defenses: " + cm + " CM + " + pd + " PD = " + activeDefenses);
//
//            var remaining = Math.Min(TargetData.Missiles, activeDefenses);
//            Debug.Log("Potential intercepts: " + remaining);
//            var diceRolls = Dice.D10s(remaining);
//            Debug.Log("Rolled against " + TargetData.Weapon.evasion + "+ evasion: " + string.Join(", ", diceRolls));
//            var failures = diceRolls.Count(r => r < TargetData.Weapon.evasion);
//            Debug.Log("Failures: " + failures);
//            TargetData.Missiles = Math.Max(0, TargetData.Missiles - failures);
//            Debug.Log("Remaining missiles: " + TargetData.Missiles);
//            if (TargetData.Missiles == 0)
//            {
//                MakeReportToTarget(ReportType.MissilesStopped,
//                    "Missiles from " + TargetData.Attacker.Name + " have been destroyed by active defenses");
//                return false;
//            }
//
//            return true;
//        }
    }
}