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
        FireBeams
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
                    nextStep = TurnStep.CrewActions;
                    break;
                case TurnStep.CrewActions:
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
                    events.Add(GameEvent.FireBeams);
                    break;
                
                case TurnStep.CrewActions:
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

                case TurnStep.CrewActions:
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
                    attacker.alterations.Count(a =>
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
            ref List<Tuple<ReportType, string>> reports)
        {
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
                    else if (!CanPassActiveDefenses(ref missile, attacker, target, ref reports, out var hitSide))
                    {
                        missile.status = MissileStatus.Destroyed;

                        missile.position += .75f * (target.position - missile.position);
                    }
                    else
                    {
                        missile.status = MissileStatus.Hitting;
                        missile.position = target.position;
                        missile.hitSide = hitSide;
                        missile.attackRange = Mathf.CeilToInt(missile.launchPoint.DistanceTo(target.position));

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
            ref List<Tuple<ReportType, string>> reports, out Side hitSide)
        {
            var (mainBearing, secondaryBearing) = target.GetBearingTo(missile.position);

            uint cm, pd;
            if (SsdHelper.HasWedge(target.Ssd, mainBearing))
            {
                cm = 0;
                pd = SsdHelper.GetPD(target.Ssd, secondaryBearing, target.alterations);
                hitSide = secondaryBearing;
            }
            else
            {
                cm = SsdHelper.GetCM(target.Ssd, mainBearing, target.alterations);
                pd = SsdHelper.GetPD(target.Ssd, mainBearing, target.alterations);
                hitSide = mainBearing;
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

        public static Vector3 FireBeam(TargetingContext context, Ship attacker, Ship target,
            ref List<Tuple<ReportType, string>> reports,
            ref List<SsdAlteration> alterations)
        {
            var weaponMount = context.Mount;

            var totalRange = Mathf.CeilToInt(attacker.position.DistanceTo(target.position));
            var rangeBand = weaponMount.model.GetRangeBand(totalRange);

            if (!rangeBand.HasValue)
            {
                // Should not happen, but let's consider this case a miss
                reports.Add(new Tuple<ReportType, string>(ReportType.BeamsMiss,
                    $"Lasers from {attacker.name} are out of range"));
                return attacker.position;
            }

            var (mainBearing, _) = target.GetBearingTo(attacker.position);
            if (SsdHelper.HasWedge(target.Ssd, mainBearing))
            {
                // Beams cannot shoot through wedges
                reports.Add(new Tuple<ReportType, string>(ReportType.BeamsMiss,
                    $"Lasers from {attacker.position} have been stopped by wedge"));

                return target.position; // TODO offset to hit wedge
            }

            var accuracy = rangeBand.Value.accuracy;
            var diceRolls = Dice.D10s(context.Number);

            var successes = diceRolls.Count(r => r >= accuracy);

            if (successes == 0)
            {
                reports.Add(new Tuple<ReportType, string>(ReportType.BeamsMiss,
                    $"Lasers from {attacker.name} missed"));
                return target.position + Random.insideUnitSphere; // TODO do not shoot through the target ship
            }

            reports.Add(new Tuple<ReportType, string>(ReportType.MissilesHit,
                $"Lasers from {attacker.name}: {successes} hits"));

            HitTarget(weaponMount.model, mainBearing, successes, totalRange, attacker, target, 
                ref reports,
                ref alterations);

            return target.position;
        }

        public static void HitTarget(Weapon weapon, Side targetSide, int hits, int range, Ship attacker, Ship target,
            ref List<Tuple<ReportType, string>> reports,
            ref List<SsdAlteration> alterations)
        {
            var weaponType = weapon.type == WeaponType.Missile ? "Missile" : "Laser";

            var rangeBand = weapon.GetRangeBand(range);
            var sidewallStrength = SsdHelper.GetSidewall(target.Ssd, targetSide, target.alterations);

            var diceRolls = Dice.MultipleTwoD10Minus(hits);

            var hitNum = 0;
            foreach (var (result, doubleZero) in diceRolls)
            {
                hitNum += 1;
                var penetrationResult = result - (int) sidewallStrength;

                if (penetrationResult <= 0)
                {
                    reports.Add(new Tuple<ReportType, string>(ReportType.Info,
                        $"#{hitNum} {weaponType} from {attacker.name} have been stopped by sidewall"));

                    if (doubleZero)
                    {
                        reports.Add(new Tuple<ReportType, string>(ReportType.DamageTaken,
                            $"#{hitNum} {weaponType} damaged: {targetSide.ToFriendlyString()} sidewall"));

                        alterations.AddRange(MakeAlterationsForBoxes(
                            new SsdAlteration() {type = SsdAlterationType.Sidewall, side = targetSide},
                            1,
                            target.Ssd.defenses.First(sd => sd.side == targetSide).sidewall,
                            target.alterations,
                            alterations
                        ));
                    }
                }
                else
                {
                    if (!rangeBand.HasValue) continue;

                    var actualPenetration = Math.Min(penetrationResult, rangeBand.Value.penetration);
                    var damages = rangeBand.Value.damage + actualPenetration;
                    var location = (uint) Dice.D10();

                    ApplyDamagesToLocation(targetSide, location, damages, target, weaponType, hitNum, ref reports,
                        ref alterations);
                }
            }
        }

        private static void ApplyDamagesToLocation(Side side, uint location, int damages, Ship target,
            string weaponType,
            int hitNum,
            ref List<Tuple<ReportType, string>> reports,
            ref List<SsdAlteration> alterations)
        {
            var remainingDamages = damages;
            var currentLocation = location;
            while (remainingDamages > 0)
            {
                var loc = target.Ssd.hitLocations[currentLocation - 1];
                remainingDamages -= loc.coreArmor;

                if (remainingDamages > 0)
                {
                    for (var i = 0; i < loc.slots.Length && remainingDamages > 0; i++)
                    {
                        var slot = loc.slots[i];
                        switch (slot.type)
                        {
                            case HitLocationSlotType.None:
                                break;
                            case HitLocationSlotType.Missile:
                                var missileAlterations = MakeAlterationsForBoxes(
                                    new SsdAlteration()
                                    {
                                        side = side,
                                        type = SsdAlterationType.Slot,
                                        slotType = HitLocationSlotType.Missile,
                                        location = currentLocation
                                    },
                                    1,
                                    target.Ssd.weaponMounts.First(m =>
                                        m.side == side && m.model.type == WeaponType.Missile).weapons,
                                    target.alterations,
                                    alterations
                                );

                                if (missileAlterations.Any())
                                {
                                    alterations.AddRange(missileAlterations);
                                    remainingDamages -= missileAlterations.Count;
                                    reports.Add(new Tuple<ReportType, string>(ReportType.DamageTaken,
                                        $"#{hitNum} {weaponType} damaged: {side.ToFriendlyString()} missiles"));
                                }

                                break;
                            case HitLocationSlotType.Laser:
                                var laserAlterations = MakeAlterationsForBoxes(
                                    new SsdAlteration()
                                    {
                                        side = side,
                                        type = SsdAlterationType.Slot,
                                        slotType = HitLocationSlotType.Laser,
                                        location = currentLocation
                                    },
                                    1,
                                    target.Ssd.weaponMounts.First(m =>
                                        m.side == side && m.model.type == WeaponType.Laser).weapons,
                                    target.alterations,
                                    alterations
                                );

                                if (laserAlterations.Any())
                                {
                                    alterations.AddRange(laserAlterations);
                                    remainingDamages -= laserAlterations.Count;
                                    reports.Add(new Tuple<ReportType, string>(ReportType.DamageTaken,
                                        $"#{hitNum} {weaponType} damaged: {side.ToFriendlyString()} lasers"));
                                }

                                break;
                            case HitLocationSlotType.CounterMissile:
                            case HitLocationSlotType.PointDefense:
                                var sideDefenses = target.Ssd.defenses.First(sd => sd.side == side);
                                var sideDefenseAlterations = MakeAlterationsForBoxes(
                                    new SsdAlteration()
                                    {
                                        side = side,
                                        type = SsdAlterationType.Slot,
                                        slotType = slot.type,
                                        location = currentLocation
                                    },
                                    1,
                                    slot.type == HitLocationSlotType.CounterMissile
                                        ? sideDefenses.counterMissiles
                                        : sideDefenses.pointDefense,
                                    target.alterations,
                                    alterations
                                );

                                if (sideDefenseAlterations.Any())
                                {
                                    alterations.AddRange(sideDefenseAlterations);
                                    remainingDamages -= sideDefenseAlterations.Count;
                                    reports.Add(new Tuple<ReportType, string>(ReportType.DamageTaken,
                                        $"#{hitNum} {weaponType} damaged: {side.ToFriendlyString()} {(slot.type == HitLocationSlotType.CounterMissile ? "CM" : "PD")}"));
                                }

                                break;
                            case HitLocationSlotType.Decoy:
                                // TODO
                                break;
                            case HitLocationSlotType.ForwardImpeller:
                            case HitLocationSlotType.AftImpeller:
                                var impellerAlterations = MakeAlterationsForBoxes(
                                    new SsdAlteration()
                                    {
                                        side = side,
                                        type = SsdAlterationType.Slot,
                                        slotType = slot.type,
                                        location = currentLocation
                                    },
                                    1,
                                    slot.boxes,
                                    target.alterations,
                                    alterations
                                );

                                if (impellerAlterations.Any())
                                {
                                    alterations.AddRange(impellerAlterations);
                                    remainingDamages -= impellerAlterations.Count();
                                    reports.Add(new Tuple<ReportType, string>(ReportType.DamageTaken,
                                        $"#{hitNum} {weaponType} damaged: {(slot.type == HitLocationSlotType.ForwardImpeller ? "forward" : "aft")} impeller"));

                                    // damage movement boxes
                                    alterations.AddRange(MakeAlterationsForBoxes(
                                        new SsdAlteration() {type = SsdAlterationType.Movement},
                                        impellerAlterations.Count,
                                        target.Ssd.movement,
                                        target.alterations,
                                        alterations
                                    ));
                                }

                                break;
                            case HitLocationSlotType.Cargo:
                            case HitLocationSlotType.Hull:
                            case HitLocationSlotType.ECCM:
                            case HitLocationSlotType.ECM:
                            case HitLocationSlotType.Bridge:
                            case HitLocationSlotType.Pivot:
                            case HitLocationSlotType.Roll:
                            case HitLocationSlotType.DamageControl:
                                var slotAlterations = MakeAlterationsForBoxes(
                                    new SsdAlteration()
                                    {
                                        side = side,
                                        type = SsdAlterationType.Slot,
                                        slotType = slot.type,
                                        location = currentLocation
                                    },
                                    1,
                                    slot.boxes,
                                    target.alterations,
                                    alterations
                                );

                                if (slotAlterations.Any())
                                {
                                    alterations.AddRange(slotAlterations);
                                    remainingDamages -= slotAlterations.Count();
                                    reports.Add(new Tuple<ReportType, string>(ReportType.DamageTaken,
                                        $"#{hitNum} {weaponType} damaged: {slot.type} (location {currentLocation})"));
                                }

                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                    }
                }

                if (loc.passThrough)
                {
                    currentLocation++;
                    if (currentLocation > target.Ssd.hitLocations.Length) currentLocation = 1;
                }
                else if (loc.structural)
                {
                    var roll = Dice.TwoD10Minus().Item1;
                    var structuralDamages = Math.Min(remainingDamages, roll);

                    if (structuralDamages > 0)
                    {
                        reports.Add(new Tuple<ReportType, string>(ReportType.DamageTaken,
                            $"#{hitNum} {weaponType} made structural damages: {structuralDamages}"));

                        alterations.AddRange(MakeAlterationsForBoxes(
                            new SsdAlteration()
                            {
                                destroyed = true,
                                type = SsdAlterationType.Structural
                            },
                            structuralDamages,
                            target.Ssd.structuralIntegrity,
                            target.alterations,
                            alterations
                        ));
                    }

                    break; // remaining damages are lost;
                }
            }
        }

        private static List<SsdAlteration> MakeAlterationsForBoxes(SsdAlteration template, int damages,
            IReadOnlyCollection<uint> boxes,
            IEnumerable<SsdAlteration> existingAlterations, IEnumerable<SsdAlteration> pendingAlterations)
        {
            var alterations = new List<SsdAlteration>();

            var nbDamaged =
                existingAlterations.Count(a =>
                    a.location == template.location &&
                    a.side == template.side &&
                    a.type == template.type &&
                    a.slotType == template.slotType)
                + pendingAlterations.Count(a =>
                    a.location == template.location &&
                    a.side == template.side &&
                    a.type == template.type &&
                    a.slotType == template.slotType);

            if (nbDamaged >= boxes.Count) return alterations;

            var todoDamages = Math.Min(damages, boxes.Count - nbDamaged);

            for (var i = 0; i < todoDamages; i++)
            {
                alterations.Add(template);
            }

            return alterations;
        }
    }
}