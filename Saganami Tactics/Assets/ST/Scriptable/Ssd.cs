using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ST.Scriptable
{
    [CreateAssetMenu(menuName = "ST/Ship SSD")]
    public class Ssd : ScriptableObject
    {
        public string className;
        public ShipCategory category;
        public Faction faction;
        public int baseCost;
        public int crewRate;

        public uint[] movement;
        public uint[] structuralIntegrity;

        public WeaponMount[] weaponMounts;
        public SideDefenses[] defenses;
        public HitLocation[] hitLocations;
    }


    [Serializable]
    public struct WeaponMount
    {
        public Side side;
        public Weapon model;
        public uint[] weapons;
        public int ammo;

        public bool Equals(WeaponMount other)
        {
            return side == other.side && Equals(model, other.model);
        }

        public override bool Equals(object obj)
        {
            return obj is WeaponMount other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((int) side * 397) ^ (model != null ? model.GetHashCode() : 0);
            }
        }
    }

    [Serializable]
    public struct SideDefenses
    {
        public Side side;
        public bool wedge;
        public uint[] sidewall;
        public uint[] counterMissiles;
        public uint[] pointDefense;
        public int armorStrength;
    }

    [Serializable]
    public struct HitLocation
    {
        public int coreArmor;
        public bool passThrough;
        public bool structural;
        public HitLocationSlot[] slots;
    }

    [Serializable]
    public struct HitLocationSlot
    {
        public HitLocationSlotType type;
        public uint[] boxes;
    }

    public enum HitLocationSlotType
    {
        None,
        Missile,
        Laser,
        CounterMissile,
        PointDefense,
        Cargo,
        Hull,
        Decoy,
        ForwardImpeller,
        AftImpeller,
        ECCM,
        ECM,
        Bridge,
        Pivot,
        Roll,
        DamageControl,
    }

    public static class SsdHelper
    {
        private static Dictionary<string, Ssd> _availableSsds;

        public static Dictionary<string, Ssd> AvailableSsds
        {
            get
            {
                if (_availableSsds != null) return _availableSsds;

                var ssds = Resources.LoadAll<Ssd>("SSD");
                _availableSsds = ssds.ToDictionary((ssd) => ssd.className);

                return _availableSsds;
            }
        }

        public static uint GetUndamagedValue(IEnumerable<uint> boxes, IEnumerable<SsdAlteration> alterations)
        {
            var nbDamaged = alterations.Count(a =>
                a.status == SsdAlterationStatus.Damaged || a.status == SsdAlterationStatus.Destroyed);

            var undamagedBoxes = boxes.Skip(nbDamaged).ToArray();

            if (undamagedBoxes.Length == 0) return 0;

            if (undamagedBoxes[0] == 0) return (uint) undamagedBoxes.Length;

            return undamagedBoxes[0];
        }

        public static uint GetMaxPivot(Ssd ssd, IEnumerable<SsdAlteration> alterations)
        {
            var pivotAlterations = alterations.Where(a =>
                a.type == SsdAlterationType.Slot && a.slotType == HitLocationSlotType.Pivot);

            var boxes = Array.Empty<uint>();
            foreach (var hitLocation in ssd.hitLocations)
            {
                var slots = hitLocation.slots.Where(s => s.type == HitLocationSlotType.Pivot).ToList();
                if (slots.Any())
                {
                    boxes = slots.First().boxes;
                }
            }

            return GetUndamagedValue(boxes, pivotAlterations);
        }

        public static uint GetMaxRoll(Ssd ssd, IEnumerable<SsdAlteration> alterations)
        {
            var rollAlterations = alterations.Where(a =>
                a.type == SsdAlterationType.Slot && a.slotType == HitLocationSlotType.Roll);

            var boxes = Array.Empty<uint>();
            foreach (var hitLocation in ssd.hitLocations)
            {
                var slots = hitLocation.slots.Where(s => s.type == HitLocationSlotType.Roll).ToList();
                if (slots.Any())
                {
                    boxes = slots.First().boxes;
                }
            }

            return GetUndamagedValue(boxes, rollAlterations);
        }

        public static uint GetMaxThrust(Ssd ssd, IEnumerable<SsdAlteration> alterations)
        {
            var mvtAlterations = alterations.Where(a => a.type == SsdAlterationType.Movement);
            var boxes = ssd.movement;
            return GetUndamagedValue(boxes, mvtAlterations);
        }

        public static uint GetECM(Ssd ssd, IEnumerable<SsdAlteration> alterations)
        {
            var ecmAlterations = alterations.Where(a =>
                a.type == SsdAlterationType.Slot && a.slotType == HitLocationSlotType.ECM);

            var boxes = Array.Empty<uint>();
            foreach (var hitLocation in ssd.hitLocations)
            {
                var slots = hitLocation.slots.Where(s => s.type == HitLocationSlotType.ECM).ToList();
                if (slots.Any())
                {
                    boxes = slots.First().boxes;
                }
            }

            return GetUndamagedValue(boxes, ecmAlterations);
        }

        public static uint GetECCM(Ssd ssd, IEnumerable<SsdAlteration> alterations)
        {
            var eccmAlterations = alterations.Where(a =>
                a.type == SsdAlterationType.Slot && a.slotType == HitLocationSlotType.ECCM);

            var boxes = Array.Empty<uint>();
            foreach (var hitLocation in ssd.hitLocations)
            {
                var slots = hitLocation.slots.Where(s => s.type == HitLocationSlotType.ECCM).ToList();
                if (slots.Any())
                {
                    boxes = slots.First().boxes;
                }
            }

            return GetUndamagedValue(boxes, eccmAlterations);
        }

        public static bool AttemptCrewRateCheck(Ssd ssd)
        {
            var diceRoll = Dice.D10();
            return diceRoll >= ssd.crewRate;
        }

        public static bool HasWedge(Ssd ssd, Side side)
        {
            return ssd.defenses.First(d => d.side == side).wedge;
        }

        public static uint GetCM(Ssd ssd, Side side, IEnumerable<SsdAlteration> alterations)
        {
            var defenses = ssd.defenses.First(d => d.side == side);
            var cmAlterations = alterations.Where(a =>
                a.type == SsdAlterationType.Slot && a.slotType == HitLocationSlotType.CounterMissile && a.side == side);
            return GetUndamagedValue(defenses.counterMissiles, cmAlterations);
        }

        public static uint GetPD(Ssd ssd, Side side, IEnumerable<SsdAlteration> alterations)
        {
            var defenses = ssd.defenses.First(d => d.side == side);
            var pdAlterations = alterations.Where(a =>
                a.type == SsdAlterationType.Slot && a.slotType == HitLocationSlotType.PointDefense && a.side == side);
            return GetUndamagedValue(defenses.pointDefense, pdAlterations);
        }
    }
}