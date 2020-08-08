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

        public WeaponMounts weaponMounts;
        public SidesDefenses defenses;
        public HitLocation[] hitLocations;
    }

    [Serializable]
    public struct WeaponMounts
    {
        public WeaponMount[] forward;
        public WeaponMount[] aft;
        public WeaponMount[] port;
        public WeaponMount[] starboard;
    }

    [Serializable]
    public struct WeaponMount
    {
        public Weapon model;
        public uint[] weapons;
        public int ammo;
    }

    [Serializable]
    public struct SidesDefenses
    {
        public SideDefenses forward;
        public SideDefenses aft;
        public SideDefenses port;
        public SideDefenses starboard;
        public SideDefenses top;
        public SideDefenses bottom;
    }

    [Serializable]
    public struct SideDefenses
    {
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
    }
}