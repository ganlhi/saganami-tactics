using System;
using System.Collections.Generic;
using System.Linq;
using ST.Common;
using ST.Scriptable;
using UnityEngine;
using Random = UnityEngine.Random;

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

        public static List<TargetingContext> IdentifyTargets(Ship attacker, List<Ship> allShips)
        {
            var targets = new List<TargetingContext>();

            var ennemyShips = allShips.Where(s => s.team != attacker.team);

            foreach (var ennemyShip in ennemyShips)
            {
                targets.AddRange(TryTargetWithWeaponType(attacker, ennemyShip, WeaponType.Missile));
                targets.AddRange(TryTargetWithWeaponType(attacker, ennemyShip, WeaponType.Laser));
            }

            return targets;
        }

        private static List<TargetingContext> TryTargetWithWeaponType(Ship attacker, Ship target, WeaponType type)
        {
            var targetingContexts = new List<TargetingContext>();

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
            if (mainBearing == Side.Bottom || mainBearing == Side.Top) return targetingContexts;

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

                targetingContexts.Add(new TargetingContext()
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

            return targetingContexts;
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
                    missile.position =
                        missile.launchPoint + .5f * (target.endMarkerPosition - missile.launchPoint);
                    break;
                case MissileStatus.Accelerating:
                    if (!CanStillCatchTarget(missile, attacker, target, ref reports))
                    {
                        missile.status = MissileStatus.Missed;

                        var dir = missile.launchPoint.DirectionTo(missile.position);
                        var distToTarget = missile.launchPoint.DistanceTo(target.position);
                        var distMaxRange = missile.weapon.GetMaxRange();
                        missile.position = missile.launchPoint + dir * Mathf.Min(distMaxRange, distToTarget);
                    }
                    else if (!CanTrackTarget(ref missile, attacker, target, ref reports))
                    {
                        missile.status = MissileStatus.Missed;

                        missile.position = target.position + Random.onUnitSphere;
                    }
                    else if (!CanPassActiveDefenses(ref missile, attacker, target, ref reports))
                    {
                        missile.status = MissileStatus.Destroyed;

                        missile.position += .75f * (target.position - missile.position);
                    }
                    else
                    {
                        missile.status = MissileStatus.Hitting;
                        missile.position = target.position;
                        reports.Add(new Tuple<ReportType, string>(ReportType.MissilesHit,
                            "Missiles from " + attacker.name + ": " + missile.number + " hits"));
                    }

                    break;
            }

            missile.updatedAtTurn = turn;

            return missile;
        }


        private static bool CanStillCatchTarget(Missile missile, Ship attacker, Ship target,
            ref List<Tuple<ReportType, string>> reports)
        {
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

        private static bool CanTrackTarget(ref Missile missile, Ship attacker, Ship target,
            ref List<Tuple<ReportType, string>> reports)
        {
            var activeEcm = SsdHelper.GetECM(target.Ssd, target.alterations);
            var activeEccm = SsdHelper.GetECCM(attacker.Ssd, attacker.alterations);

            if (SsdHelper.AttemptCrewRateCheck(attacker.Ssd))
            {
                activeEcm = Math.Max(0, activeEcm - activeEccm);
            }

            var totalRange = Mathf.CeilToInt(missile.launchPoint.DistanceTo(target.position));
            var rangeBand = missile.weapon.GetRangeBand(totalRange);

            if (!rangeBand.HasValue)
            {
                // Should not happen, but let's consider this case a miss
                reports.Add(new Tuple<ReportType, string>(ReportType.MissilesMissed,
                    "Missiles from " + attacker.name + " are out of range"));
                return false;
            }

            var (mainBearing, _) = target.GetBearingTo(missile.position);
            var wedgeMalus = SsdHelper.HasWedge(target.Ssd, mainBearing) ? 4 : 0;

            var accuracy = rangeBand.Value.accuracy + activeEcm + wedgeMalus;
            var diceRolls = Dice.D10s(missile.number);

            var successes = diceRolls.Count(r => r >= accuracy);
            
            if (successes == 0)
            {
                reports.Add(new Tuple<ReportType, string>(ReportType.MissilesMissed,
                    "Missiles from " + attacker.name + " lost their target"));
                return false;
            }

            missile.number = successes;

            return true;
        }

        private static bool CanPassActiveDefenses(ref Missile missile, Ship attacker, Ship target,
            ref List<Tuple<ReportType, string>> reports)
        {
            var (mainBearing, secondaryBearing) = target.GetBearingTo(missile.position);

            uint cm, pd;
            if (SsdHelper.HasWedge(target.Ssd, mainBearing))
            {
                cm = 0;
                pd = SsdHelper.GetPD(target.Ssd, secondaryBearing, target.alterations);
            }
            else
            {
                cm = SsdHelper.GetCM(target.Ssd, mainBearing, target.alterations);
                pd = SsdHelper.GetPD(target.Ssd, mainBearing, target.alterations);
            }

            var activeDefenses = (int) (cm + pd);

            var remaining = Math.Min(missile.number, activeDefenses);
            
            var diceRolls = Dice.D10s(remaining);
            var evasion = missile.weapon.evasion;
            
            var failures = diceRolls.Count(r => r < evasion);
            
            missile.number = Math.Max(0, missile.number - failures);

            if (missile.number > 0) return true;

            reports.Add(new Tuple<ReportType, string>(ReportType.MissilesStopped,
                "Missiles from " + attacker.name + " have been destroyed by active defenses"));
            
            return false;
        }
    }
}