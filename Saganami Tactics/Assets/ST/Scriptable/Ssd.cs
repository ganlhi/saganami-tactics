using System;
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
}