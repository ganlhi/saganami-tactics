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
        FireBeams,
        ResetRepairAttempts,
        ResetDeployedDecoys,
        CheckEndGame
    }

    public static class Game
    {
        public static void NextStep(int turn, TurnStep step, out int nextTurn, out TurnStep nextStep)
        {
            nextTurn = turn;

            switch (step)
            {
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
                    nextStep = TurnStep.Plotting;
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
                    events.Add(GameEvent.ClearTargets);
                    events.Add(GameEvent.ResetRepairAttempts);
                    events.Add(GameEvent.ResetDeployedDecoys);
                    events.Add(GameEvent.CheckEndGame);
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }

            return events;
        }

        public static List<TargetingContext> IdentifyTargets(Ship attacker, List<Ship> allShips)
        {
            var targets = new List<TargetingContext>();

            var ennemyShips = allShips.Where(s => s.team != attacker.team && s.Status == ShipStatus.Ok);

            foreach (var ennemyShip in ennemyShips)
            {
                targets.AddRange(TryTargetWithWeaponType(attacker, ennemyShip, WeaponType.Missile));
                targets.AddRange(TryTargetWithWeaponType(attacker, ennemyShip, WeaponType.Laser));
                targets.AddRange(TryTargetWithWeaponType(attacker, ennemyShip, WeaponType.Graser));
            }

            return targets;
        }

        private static List<TargetingContext> TryTargetWithWeaponType(Ship attacker, Ship target, WeaponType type)
        {
            var targetingContexts = new List<TargetingContext>();

            Vector3 targetPos;
            HitLocationSlotType slotType;
            var isShortRange = false;
            var canExploitSideWithoutSidewall = false;


            if (type == WeaponType.Missile)
            {
                if (attacker.position.DistanceTo(target.position) <= GameSettings.Default.MissileShortRange)
                {
                    targetPos = target.position;
                    isShortRange = true;
                }
                else
                {
                    if (attacker.position.DistanceTo(target.endMarkerPosition) <=
                        GameSettings.Default.MissileShortRange)
                    {
                        // Special case: current position of target is too far for short range attack
                        // but future position of target is too close to a long range attack. 
                        // This means the target can be attacked at short range next turn.
                        return targetingContexts;
                    }

                    targetPos = target.endMarkerPosition;
                }

                slotType = HitLocationSlotType.Missile;
            }
            else if (type == WeaponType.Laser)
            {
                isShortRange = true;
                canExploitSideWithoutSidewall = true;
                targetPos = target.position;
                slotType = HitLocationSlotType.Laser;
            }
            else if (type == WeaponType.Graser)
            {
                isShortRange = true;
                canExploitSideWithoutSidewall = true;
                targetPos = target.position;
                slotType = HitLocationSlotType.Graser;
            }
            else
            {
                // For future weapon types
                return targetingContexts;
            }

            var (mainBearing, _) = attacker.GetBearingTo(targetPos);
            var distance = attacker.position.DistanceTo(targetPos);
            var (defenseBearing, _) = target.GetBearingTo(attacker.position);

            var distanceToCheck = canExploitSideWithoutSidewall && target.GetUnprotectedSides().Contains(defenseBearing)
                ? Mathf.CeilToInt((float) distance / 2f)
                : distance;

            if (mainBearing == Side.Bottom || mainBearing == Side.Top) return targetingContexts;

            var mounts = attacker.Ssd.weaponMounts.Where(m => m.side == mainBearing).ToArray();

            foreach (var mount in mounts)
            {
                if (mount.model.type != type) continue;

                var nbWeapons = SsdHelper.GetUndamagedValue(mount.weapons,
                    attacker.alterations.Count(a =>
                        a.side == mainBearing && a.type == SsdAlterationType.Slot && a.slotType == slotType));

                if (nbWeapons == 0) continue;

                if (type == WeaponType.Missile)
                {
                    var remainingAmmo = SsdHelper.GetRemainingAmmo(attacker.Ssd, mount, attacker.consumedAmmo);
                    if (remainingAmmo <= 0) continue;
                }

                if (mount.model.GetMaxRange() < distanceToCheck) continue;

                targetingContexts.Add(new TargetingContext()
                {
                    Mount = mount,
                    Side = mainBearing,
                    Target = target,
                    ShortRange = isShortRange,
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

                    if (missile.shortRange)
                    {
                        // accelerate then immediately attack
                        return UpdateMissile(missile, attacker, target, turn, ref reports);
                    }

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
            var decoyMalus = target.deployedDecoy ? target.Ssd.decoyStrength : 0;

            var accuracy = rangeBand.Value.accuracy + activeEcm + wedgeMalus + decoyMalus;
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
            ref List<SsdAlteration> alterations,
            ref List<Tuple<int, int>> destroyedAmmo)
        {
            var weaponMount = context.Mount;


            var (defenseBearing, _) = target.GetBearingTo(attacker.position);

            var totalRange = Mathf.CeilToInt(attacker.position.DistanceTo(target.position));

            if (target.GetUnprotectedSides().Contains(defenseBearing))
            {
                totalRange = Mathf.CeilToInt((float) totalRange / 2f);
            }

            var rangeBand = weaponMount.model.GetRangeBand(totalRange);

            if (!rangeBand.HasValue)
            {
                // Should not happen, but let's consider this case a miss
                reports.Add(new Tuple<ReportType, string>(ReportType.BeamsMiss,
                    $"{weaponMount.model.type.ToString()}s from {attacker.name} are out of range"));
                return attacker.position;
            }

            var (mainBearing, _) = target.GetBearingTo(attacker.position);
            if (SsdHelper.HasWedge(target.Ssd, mainBearing))
            {
                // Beams cannot shoot through wedges
                reports.Add(new Tuple<ReportType, string>(ReportType.BeamsMiss,
                    $"{weaponMount.model.type.ToString()}s from {attacker.position} have been stopped by wedge"));

                return target.position; // TODO offset to hit wedge
            }

            var accuracy = rangeBand.Value.accuracy;
            var diceRolls = Dice.D10s(context.Number);

            var successes = diceRolls.Count(r => r >= accuracy);

            if (successes == 0)
            {
                reports.Add(new Tuple<ReportType, string>(ReportType.BeamsMiss,
                    $"{weaponMount.model.type.ToString()}s from {attacker.name} missed"));
                return target.position + Random.insideUnitSphere; // TODO do not shoot through the target ship
            }

            reports.Add(new Tuple<ReportType, string>(ReportType.MissilesHit,
                $"{weaponMount.model.type.ToString()}s from {attacker.name}: {successes} hits"));

            HitTarget(weaponMount.model, mainBearing, successes, totalRange, attacker, target,
                ref reports,
                ref alterations,
                ref destroyedAmmo);

            return target.position;
        }

        public static void HitTarget(Weapon weapon, Side targetSide, int hits, int range, Ship attacker, Ship target,
            ref List<Tuple<ReportType, string>> reports,
            ref List<SsdAlteration> alterations,
            ref List<Tuple<int, int>> destroyedAmmo)
        {
            var weaponType = weapon.type.ToString();

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
                    var location = Dice.D10();
                    
                    var firstLocation = location - weapon.span / 2;
                    for (var loc = firstLocation; loc < firstLocation + weapon.span; loc++)
                    {
                        if (loc <= 0 || loc > 10) continue;
                        ApplyDamagesToLocation(targetSide, (uint) loc, damages, target, weaponType, hitNum,
                            ref reports,
                            ref alterations,
                            ref destroyedAmmo);
                    }
                }
            }
        }

        private static void ApplyDamagesToLocation(Side side, uint location, int damages, Ship target,
            string weaponType,
            int hitNum,
            ref List<Tuple<ReportType, string>> reports,
            ref List<SsdAlteration> alterations,
            ref List<Tuple<int, int>> destroyedAmmo)
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
                        int weaponMountIndex;

                        switch (slot.type)
                        {
                            case HitLocationSlotType.None:
                                break;
                            case HitLocationSlotType.Missile:
                                weaponMountIndex = Array.FindIndex(target.Ssd.weaponMounts, m =>
                                    m.side == side && m.model.type == WeaponType.Missile);
                                if (weaponMountIndex == -1) continue;
                                var weaponMount = target.Ssd.weaponMounts[weaponMountIndex];

                                var missileAlterations = MakeAlterationsForBoxes(
                                    new SsdAlteration()
                                    {
                                        side = side,
                                        type = SsdAlterationType.Slot,
                                        slotType = HitLocationSlotType.Missile,
                                        location = currentLocation
                                    },
                                    1,
                                    weaponMount.weapons,
                                    target.alterations,
                                    alterations
                                );

                                if (missileAlterations.Any())
                                {
                                    alterations.AddRange(missileAlterations);
                                    remainingDamages -= missileAlterations.Count;
                                    reports.Add(new Tuple<ReportType, string>(ReportType.DamageTaken,
                                        $"#{hitNum} {weaponType} damaged: {side.ToFriendlyString()} missile launchers"));

                                    // Lose ammo
                                    var amount = Dice.TwoD10Minus().Item1;
                                    if (amount > 0)
                                    {
                                        var mountIndex = Array.FindIndex(target.Ssd.weaponMounts,
                                            m => m.Equals(weaponMount));
                                        destroyedAmmo.Add(new Tuple<int, int>(mountIndex, amount));
                                        reports.Add(new Tuple<ReportType, string>(ReportType.DamageTaken,
                                            $"#{hitNum} {weaponType} destroyed ammo: {amount} {side.ToFriendlyString()}'s missiles"
                                        ));
                                    }
                                }

                                break;
                            case HitLocationSlotType.Laser:
                            case HitLocationSlotType.Graser:
                                var beamWeaponType = slot.type == HitLocationSlotType.Laser
                                    ? WeaponType.Laser
                                    : WeaponType.Graser;

                                weaponMountIndex = Array.FindIndex(target.Ssd.weaponMounts, m =>
                                    m.side == side && m.model.type == beamWeaponType);
                                if (weaponMountIndex == -1) continue;
                                var boxes = target.Ssd.weaponMounts[weaponMountIndex].weapons;

                                var beamWeaponAlterations = MakeAlterationsForBoxes(
                                    new SsdAlteration()
                                    {
                                        side = side,
                                        type = SsdAlterationType.Slot,
                                        slotType = slot.type,
                                        location = currentLocation
                                    },
                                    1,
                                    boxes,
                                    target.alterations,
                                    alterations
                                );

                                if (beamWeaponAlterations.Any())
                                {
                                    alterations.AddRange(beamWeaponAlterations);
                                    remainingDamages -= beamWeaponAlterations.Count;
                                    reports.Add(new Tuple<ReportType, string>(ReportType.DamageTaken,
                                        $"#{hitNum} {weaponType} damaged: {side.ToFriendlyString()} {beamWeaponType.ToString().ToLower()}"));
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
                            case HitLocationSlotType.ECCM:
                            case HitLocationSlotType.ECM:
                            case HitLocationSlotType.Bridge:
                            case HitLocationSlotType.Pivot:
                            case HitLocationSlotType.Roll:
                            case HitLocationSlotType.DamageControl:
                            case HitLocationSlotType.Decoy:
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
                            case HitLocationSlotType.Hull:
                                var nb = Dice.TwoD10Minus().Item1;
                                var hullAlterations = MakeAlterationsForBoxes(
                                    new SsdAlteration()
                                    {
                                        type = SsdAlterationType.Slot,
                                        slotType = HitLocationSlotType.Hull,
                                    },
                                    nb,
                                    target.Ssd.hull,
                                    target.alterations,
                                    alterations
                                );

                                if (hullAlterations.Any())
                                {
                                    alterations.AddRange(hullAlterations);
                                    reports.Add(new Tuple<ReportType, string>(ReportType.DamageTaken,
                                        $"#{hitNum} {weaponType} damaged: Hull x{nb}"));
                                }
                                else
                                {
                                    reports.Add(new Tuple<ReportType, string>(ReportType.DamageTaken,
                                        $"#{hitNum} {weaponType} made structural damages: 1"));

                                    alterations.AddRange(MakeAlterationsForBoxes(
                                        new SsdAlteration()
                                        {
                                            destroyed = true,
                                            type = SsdAlterationType.Structural
                                        },
                                        1,
                                        target.Ssd.structuralIntegrity,
                                        target.alterations,
                                        alterations
                                    ));
                                }

                                if (remainingDamages <= nb)
                                    remainingDamages = 0;
                                else
                                    remainingDamages -= nb;

                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                    }
                }

                if (loc.passThrough && remainingDamages > 0)
                {
                    currentLocation++;
                    if (currentLocation > target.Ssd.hitLocations.Length) currentLocation = 1;
                }
                else if (loc.structural && remainingDamages > 0)
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
                else
                {
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

        public static bool AttemptCrewRateCheck(Ship ship)
        {
            var crewRate = ship.Ssd.crewRate;
            var diceRoll = Dice.D10();
            return diceRoll >= crewRate;
        }

        public static Vector3 GetTeamSpawnPoint(Team team, out Vector3 fwdDirection, out Vector3 rightDirection)
        {
            const float offset = 30f;

            switch (team)
            {
                case Team.Blue:
                    fwdDirection = Vector3.back;
                    rightDirection = Vector3.left;
                    return Vector3.forward * offset;
                case Team.Yellow:
                    fwdDirection = Vector3.forward;
                    rightDirection = Vector3.right;
                    return Vector3.back * offset;
                case Team.Green:
                    fwdDirection = Vector3.right;
                    rightDirection = Vector3.back;
                    return Vector3.left * offset;
                case Team.Magenta:
                    fwdDirection = Vector3.left;
                    rightDirection = Vector3.forward;
                    return Vector3.right * offset;
                default:
                    throw new ArgumentOutOfRangeException(nameof(team), team, null);
            }
        }

        public static Vector3 GetTeamSpawnPoint(Team team)
        {
            return GetTeamSpawnPoint(team, out var f, out var r);
        }

        public static List<ShipState> PrePlaceTeamShips(Team team, IEnumerable<ShipState> ships)
        {
            var basePosition = GetTeamSpawnPoint(team, out var fwdDirection, out var rightDirection);

            var rowOffsets = new Vector3[]
            {
                Vector3.zero,
                rightDirection,
                -rightDirection,
                rightDirection * 2,
                -rightDirection * 2,
                rightDirection * 3,
                -rightDirection * 3,
                rightDirection * 4,
                -rightDirection * 4,
                rightDirection * 5,
                -rightDirection * 5,
            };
            var colOffsets = new Vector3[]
            {
                Vector3.zero,
                -fwdDirection,
                -fwdDirection * 2,
                -fwdDirection * 3,
                -fwdDirection * 4,
                -fwdDirection * 5,
                fwdDirection,
                fwdDirection * 2,
                fwdDirection * 3,
                fwdDirection * 4,
                fwdDirection * 5,
            };
            var verticalOffsets = new Vector3[]
            {
                Vector3.zero,
                Vector3.up,
                Vector3.down,
                Vector3.up * 2,
                Vector3.down * 2,
                Vector3.up * 3,
                Vector3.down * 3,
                Vector3.up * 4,
                Vector3.down * 4,
                Vector3.up * 5,
                Vector3.down * 5,
            };

            var curRowOffset = 0;
            var curColOffset = 0;
            var curVerticalOffset = 0;

            var updatedShips = new List<ShipState>();

            foreach (var ship in ships)
            {
                var position = basePosition
                               + rowOffsets[curRowOffset]
                               + colOffsets[curColOffset]
                               + verticalOffsets[curVerticalOffset];

                var updatedShip = ship;

                updatedShip.position = position;
                updatedShip.rotation = Quaternion.LookRotation(fwdDirection);

                updatedShips.Add(updatedShip);

                if (curRowOffset < rowOffsets.Length - 1)
                {
                    curRowOffset++;
                }
                else if (curColOffset < colOffsets.Length - 1)
                {
                    curRowOffset = 0;
                    curColOffset++;
                }
                else if (curVerticalOffset < verticalOffsets.Length - 1)
                {
                    curRowOffset = 0;
                    curColOffset = 0;
                    curVerticalOffset++;
                }
                else
                {
                    throw new IndexOutOfRangeException("Too many ships to place");
                }
            }

            return updatedShips;
        }

        public static bool ShouldEndGame(IEnumerable<Ship> ships)
        {
            var okShips = ships.Where(s => s.Status == ShipStatus.Ok).ToList();
            return okShips.Select(s => s.team).Distinct().ToList().Count < 2;
        }

        public static int GetTeamScore(Team team, IEnumerable<Ship> ships, out List<ScoreLine> scoreDetails)
        {
            var totalScore = 0;
            scoreDetails = new List<ScoreLine>();

            var ennemyShips = ships.Where(s => s.team != team).ToList();

            foreach (var ennemyShip in ennemyShips)
            {
                var ssd = ennemyShip.Ssd;
                var score = 0;
                if (ennemyShip.Status == ShipStatus.Destroyed)
                {
                    score = ssd.baseCost;
                    scoreDetails.Add(new ScoreLine() {Reason = $"Destroyed ship: {ennemyShip.name}", Score = score});
                }
                else if (ennemyShip.Status == ShipStatus.Surrendered)
                {
                    score = ssd.baseCost
                            - Mathf.CeilToInt((float) ssd.crewOfficers / 5f)
                            - Mathf.CeilToInt((float) ssd.crewEnlisted / 100f);
                    scoreDetails.Add(new ScoreLine() {Reason = $"Surrendered ship: {ennemyShip.name}", Score = score});
                }
                else
                {
                    var damagedBoxesRatio = SsdHelper.GetDamagedBoxesRatio(ssd, ennemyShip.alterations);

                    if (damagedBoxesRatio > 0)
                    {
                        score = Mathf.CeilToInt(ssd.baseCost * damagedBoxesRatio);
                        scoreDetails.Add(new ScoreLine() {Reason = $"Damaged ship: {ennemyShip.name}", Score = score});
                    }
                }

                totalScore += score;
            }

            return totalScore;
        }
    }
}