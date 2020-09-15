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
        public int scale;
        public int crewRate;
        public int crewOfficers;
        public int crewEnlisted;
        public uint serviceYearFrom;
        public uint serviceYearTo;

        public int crew => crewOfficers + crewEnlisted;

        public int[] movement;
        public int[] structuralIntegrity;
        public int[] hull;

        public int decoyStrength;

        public WeaponMount[] weaponMounts;

        public SideDefenses[] defenses = new[]
        {
            new SideDefenses()
            {
                side = Side.Top,
                wedge = true,
            },
            new SideDefenses()
            {
                side = Side.Bottom,
                wedge = true,
            },
            new SideDefenses()
            {
                side = Side.Aft,
                sidewall = new int[] {0},
                noSidewallModifier = -1,
            },
            new SideDefenses()
            {
                side = Side.Forward,
                noSidewallModifier = -1,
            },
            new SideDefenses()
            {
                side = Side.Port,
                noSidewallModifier = -3,
            },
            new SideDefenses()
            {
                side = Side.Starboard,
                noSidewallModifier = -3,
            },
        };

        public HitLocation[] hitLocations = new[]
        {
            // 1
            new HitLocation()
            {
                passThrough = true,
                slots = new[]
                {
                    new HitLocationSlot()
                    {
                        type = HitLocationSlotType.Missile
                    },
                    new HitLocationSlot()
                    {
                        type = HitLocationSlotType.Hull
                    },
                    new HitLocationSlot()
                    {
                        type = HitLocationSlotType.Decoy
                    },
                    new HitLocationSlot()
                    {
                        type = HitLocationSlotType.Hull
                    },
                }
            },
            // 2
            new HitLocation()
            {
                passThrough = true,
                slots = new[]
                {
                    new HitLocationSlot()
                    {
                        type = HitLocationSlotType.Hull
                    },
                    new HitLocationSlot()
                    {
                        type = HitLocationSlotType.PointDefense
                    },
                    new HitLocationSlot()
                    {
                        type = HitLocationSlotType.Laser
                    },
                    new HitLocationSlot()
                    {
                        type = HitLocationSlotType.Hull
                    },
                }
            },
            // 3
            new HitLocation()
            {
                passThrough = true,
                slots = new[]
                {
                    new HitLocationSlot()
                    {
                        type = HitLocationSlotType.Hull
                    },
                    new HitLocationSlot()
                    {
                        type = HitLocationSlotType.ForwardImpeller
                    },
                    new HitLocationSlot()
                    {
                        type = HitLocationSlotType.Hull
                    },
                    new HitLocationSlot()
                    {
                        type = HitLocationSlotType.Hull
                    },
                }
            },
            // 4
            new HitLocation()
            {
                passThrough = true,
                slots = new[]
                {
                    new HitLocationSlot()
                    {
                        type = HitLocationSlotType.ECCM
                    },
                    new HitLocationSlot()
                    {
                        type = HitLocationSlotType.Missile
                    },
                    new HitLocationSlot()
                    {
                        type = HitLocationSlotType.Hull
                    },
                    new HitLocationSlot()
                    {
                        type = HitLocationSlotType.Hull
                    },
                }
            },
            // 5
            new HitLocation()
            {
                structural = true,
                coreArmor = 1,
                slots = new[]
                {
                    new HitLocationSlot()
                    {
                        type = HitLocationSlotType.Hull
                    },
                    new HitLocationSlot()
                    {
                        type = HitLocationSlotType.Roll
                    },
                    new HitLocationSlot()
                    {
                        type = HitLocationSlotType.Hull
                    },
                    new HitLocationSlot()
                    {
                        type = HitLocationSlotType.Bridge
                    },
                }
            },
            // 6
            new HitLocation()
            {
                structural = true,
                coreArmor = 1,
                slots = new[]
                {
                    new HitLocationSlot()
                    {
                        type = HitLocationSlotType.Hull
                    },
                    new HitLocationSlot()
                    {
                        type = HitLocationSlotType.ECM
                    },
                    new HitLocationSlot()
                    {
                        type = HitLocationSlotType.Pivot
                    },
                    new HitLocationSlot()
                    {
                        type = HitLocationSlotType.Hull
                    },
                }
            },
            // 7
            new HitLocation()
            {
                passThrough = true,
                slots = new[]
                {
                    new HitLocationSlot()
                    {
                        type = HitLocationSlotType.Hull
                    },
                    new HitLocationSlot()
                    {
                        type = HitLocationSlotType.Laser
                    },
                    new HitLocationSlot()
                    {
                        type = HitLocationSlotType.Hull
                    },
                    new HitLocationSlot()
                    {
                        type = HitLocationSlotType.Hull
                    },
                }
            },
            // 8
            new HitLocation()
            {
                passThrough = true,
                slots = new[]
                {
                    new HitLocationSlot()
                    {
                        type = HitLocationSlotType.Hull
                    },
                    new HitLocationSlot()
                    {
                        type = HitLocationSlotType.AftImpeller
                    },
                    new HitLocationSlot()
                    {
                        type = HitLocationSlotType.Hull
                    },
                    new HitLocationSlot()
                    {
                        type = HitLocationSlotType.Hull
                    },
                }
            },
            // 9
            new HitLocation()
            {
                passThrough = true,
                slots = new[]
                {
                    new HitLocationSlot()
                    {
                        type = HitLocationSlotType.Hull
                    },
                    new HitLocationSlot()
                    {
                        type = HitLocationSlotType.CounterMissile
                    },
                    new HitLocationSlot()
                    {
                        type = HitLocationSlotType.Hull
                    },
                    new HitLocationSlot()
                    {
                        type = HitLocationSlotType.Hull
                    },
                }
            },
            // 10
            new HitLocation()
            {
                slots = new[]
                {
                    new HitLocationSlot()
                    {
                        type = HitLocationSlotType.Hull
                    },
                    new HitLocationSlot()
                    {
                        type = HitLocationSlotType.DamageControl
                    },
                    new HitLocationSlot()
                    {
                        type = HitLocationSlotType.Missile
                    },
                    new HitLocationSlot()
                    {
                        type = HitLocationSlotType.Hull
                    },
                }
            },
        };

        public List<string> sampleNames;
    }


    [Serializable]
    public struct WeaponMount
    {
        public Side side;
        public Weapon model;
        public int[] weapons;
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
        public int[] sidewall;
        public int[] counterMissiles;
        public int[] pointDefense;
        public int noSidewallModifier;
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
        public int[] boxes;
    }

    public enum HitLocationSlotType
    {
        None,
        Missile,
        Laser,
        Graser,
        CounterMissile,
        PointDefense,
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
        private static List<ShipCategory> _availableCategories;
        private static List<Faction> _availableFactions;

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

        public static List<ShipCategory> AvailableCategories
        {
            get
            {
                if (_availableCategories != null) return _availableCategories;

                var categories = Resources.LoadAll<ShipCategory>("SSD/Categories");
                _availableCategories = categories
                    .Where(cat => AvailableSsds.Values.Any(ssd => ssd.category == cat))
                    .ToList();

                _availableCategories.Sort((a, b) => a.DisplayOrder.CompareTo(b.DisplayOrder));

                return _availableCategories;
            }
        }

        public static List<Faction> AvailableFactions
        {
            get
            {
                if (_availableFactions != null) return _availableFactions;

                var factions = Resources.LoadAll<Faction>("SSD/Factions");
                _availableFactions = factions
                    .Where(fac => AvailableSsds.Values.Any(ssd => ssd.faction == fac))
                    .ToList();

                _availableFactions.Sort((a, b) => a.DisplayOrder.CompareTo(b.DisplayOrder));

                return _availableFactions;
            }
        }

        public static int GetUndamagedValue(IEnumerable<int> boxes, int nbDamaged, int valueIfNone = 0)
        {
            var undamagedBoxes = boxes.Skip(nbDamaged).ToArray();

            if (undamagedBoxes.Length == 0) return valueIfNone;

            if (undamagedBoxes[0] == 0) return undamagedBoxes.Length;

            return undamagedBoxes[0];
        }

        public static int GetMaxPivot(Ssd ssd, IEnumerable<SsdAlteration> alterations, int usedRolls, int thrust)
        {
            var pivotAlterations = alterations.Count(a =>
                a.type == SsdAlterationType.Slot && a.slotType == HitLocationSlotType.Pivot);

            var boxes = Array.Empty<int>();
            foreach (var hitLocation in ssd.hitLocations)
            {
                var slots = hitLocation.slots.Where(s => s.type == HitLocationSlotType.Pivot).ToList();
                if (slots.Any())
                {
                    boxes = slots.First().boxes;
                }
            }

            var pivot = GetUndamagedValue(boxes, pivotAlterations);
            if (pivot == 0) return 0;

            if (GetBridge(ssd, alterations) < 1)
            {
                return usedRolls == 0 && thrust == 0 ? 1 : 0;
            }

            return pivot;
        }

        public static int GetMaxRoll(Ssd ssd, IEnumerable<SsdAlteration> alterations, int usedPivots, int thrust)
        {
            var rollAlterations = alterations.Count(a =>
                a.type == SsdAlterationType.Slot && a.slotType == HitLocationSlotType.Roll);

            var boxes = Array.Empty<int>();
            foreach (var hitLocation in ssd.hitLocations)
            {
                var slots = hitLocation.slots.Where(s => s.type == HitLocationSlotType.Roll).ToList();
                if (slots.Any())
                {
                    boxes = slots.First().boxes;
                }
            }

            var roll = GetUndamagedValue(boxes, rollAlterations);
            if (roll == 0) return 0;

            if (GetBridge(ssd, alterations) < 1)
            {
                return usedPivots == 0 && thrust == 0 ? 1 : 0;
            }

            return roll;
        }

        public static int GetMaxThrust(Ssd ssd, IEnumerable<SsdAlteration> alterations, int usedPivots, int usedRolls)
        {
            var mvtAlterations = alterations.Count(a => a.type == SsdAlterationType.Movement);
            var boxes = ssd.movement;
            
            var thrust = GetUndamagedValue(boxes, mvtAlterations);
            if (thrust == 0) return 0;

            if (GetBridge(ssd, alterations) < 1)
            {
                return usedPivots == 0 && usedRolls == 0 ? 1 : 0;
            }

            return thrust;
        }

        public static int GetECM(Ssd ssd, IEnumerable<SsdAlteration> alterations)
        {
            var ecmAlterations = alterations.Count(a =>
                a.type == SsdAlterationType.Slot && a.slotType == HitLocationSlotType.ECM);

            var boxes = Array.Empty<int>();
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

        public static int GetECCM(Ssd ssd, IEnumerable<SsdAlteration> alterations)
        {
            var eccmAlterations = alterations.Count(a =>
                a.type == SsdAlterationType.Slot && a.slotType == HitLocationSlotType.ECCM);

            var boxes = Array.Empty<int>();
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

        public static int GetCM(Ssd ssd, Side side, IEnumerable<SsdAlteration> alterations)
        {
            var defenses = ssd.defenses.First(d => d.side == side);
            var cmAlterations = alterations.Count(a =>
                a.type == SsdAlterationType.Slot && a.slotType == HitLocationSlotType.CounterMissile && a.side == side);
            return GetUndamagedValue(defenses.counterMissiles, cmAlterations);
        }

        public static int GetPD(Ssd ssd, Side side, IEnumerable<SsdAlteration> alterations)
        {
            var defenses = ssd.defenses.First(d => d.side == side);
            var pdAlterations = alterations.Count(a =>
                a.type == SsdAlterationType.Slot && a.slotType == HitLocationSlotType.PointDefense && a.side == side);
            return GetUndamagedValue(defenses.pointDefense, pdAlterations);
        }

        public static int GetSidewall(Ssd ssd, Side side, IEnumerable<SsdAlteration> alterations)
        {
            var sidewallAlterations = alterations.Count(a => a.type == SsdAlterationType.Sidewall && a.side == side);
            var sideDefenses = ssd.defenses.First(d => d.side == side);
            var boxes = sideDefenses.sidewall;
            if (boxes == null || boxes.Length == 0)
            {
            }

            return GetUndamagedValue(boxes, sidewallAlterations, (int) sideDefenses.noSidewallModifier);
        }

        public static int GetRemainingStructuralPoints(Ssd ssd, IEnumerable<SsdAlteration> alterations)
        {
            var structuralAlterations = alterations.Count(a => a.type == SsdAlterationType.Structural);
            return GetUndamagedValue(ssd.structuralIntegrity, structuralAlterations);
        }

        public static int GetDamageControl(Ssd ssd, IEnumerable<SsdAlteration> alterations,
            IEnumerable<bool> repairAttempts)
        {
            var dcAlterations = alterations.Count(a =>
                a.type == SsdAlterationType.Slot && a.slotType == HitLocationSlotType.DamageControl);

            var boxes = Array.Empty<int>();
            foreach (var hitLocation in ssd.hitLocations)
            {
                var slots = hitLocation.slots.Where(s => s.type == HitLocationSlotType.DamageControl).ToList();
                if (slots.Any())
                {
                    boxes = slots.First().boxes;
                }
            }

            return Math.Max(0, GetUndamagedValue(boxes, dcAlterations) - (int) repairAttempts.Count());
        }

        public static int GetBridge(Ssd ssd, IEnumerable<SsdAlteration> alterations)
        {
            var bridgeAlterations = alterations.Count(a =>
                a.type == SsdAlterationType.Slot && a.slotType == HitLocationSlotType.Bridge);

            var boxes = Array.Empty<int>();
            foreach (var hitLocation in ssd.hitLocations)
            {
                var slots = hitLocation.slots.Where(s => s.type == HitLocationSlotType.Bridge).ToList();
                if (slots.Any())
                {
                    boxes = slots.First().boxes;
                }
            }

            return GetUndamagedValue(boxes, bridgeAlterations);
        }

        public static int GetRemainingAmmo(Ssd ssd, WeaponMount mount, Dictionary<int, int> consumedAmmo)
        {
            var remainingAmmo = mount.ammo;

            var mountIndex = Array.FindIndex(ssd.weaponMounts, m => m.Equals(mount));

            if (consumedAmmo.ContainsKey(mountIndex))
            {
                remainingAmmo -= consumedAmmo[mountIndex];
            }

            return remainingAmmo;
        }

        public static int GetRemainingDecoys(Ssd ssd, List<SsdAlteration> alterations)
        {
            var decoysAlterations = alterations.Count(a =>
                a.type == SsdAlterationType.Slot && a.slotType == HitLocationSlotType.Decoy);

            var boxes = Array.Empty<int>();
            foreach (var hitLocation in ssd.hitLocations)
            {
                var slots = hitLocation.slots.Where(s => s.type == HitLocationSlotType.Decoy).ToList();
                if (slots.Any())
                {
                    boxes = slots.First().boxes;
                }
            }

            return GetUndamagedValue(boxes, decoysAlterations);
        }

        public static string SlotTypeToString(HitLocationSlotType slotType)
        {
            switch (slotType)
            {
                case HitLocationSlotType.None:
                case HitLocationSlotType.Missile:
                case HitLocationSlotType.Laser:
                case HitLocationSlotType.Graser:
                case HitLocationSlotType.Hull:
                case HitLocationSlotType.Decoy:
                case HitLocationSlotType.ECCM:
                case HitLocationSlotType.ECM:
                case HitLocationSlotType.Bridge:
                case HitLocationSlotType.Pivot:
                case HitLocationSlotType.Roll:
                    return slotType.ToString();

                case HitLocationSlotType.CounterMissile:
                    return "Counter missile";
                case HitLocationSlotType.PointDefense:
                    return "Point defense";
                case HitLocationSlotType.ForwardImpeller:
                    return "Forward impeller";
                case HitLocationSlotType.AftImpeller:
                    return "Aft impeller";
                case HitLocationSlotType.DamageControl:
                    return "Damage control";
                default:
                    throw new ArgumentOutOfRangeException(nameof(slotType), slotType, null);
            }
        }

        public static float GetDamagedBoxesRatio(Ssd ssd, IEnumerable<SsdAlteration> alterations)
        {
            var totalBoxes = ssd.hitLocations.Sum(location =>
                                 location.slots.Sum(slot =>
                                     slot.type != HitLocationSlotType.Decoy ? slot.boxes.Length : 0))
                             + ssd.movement.Length
                             + ssd.structuralIntegrity.Length
                             + ssd.weaponMounts.Sum(mount => mount.weapons.Length)
                             + ssd.defenses.Sum(defense =>
                                 defense.sidewall.Length
                                 + defense.counterMissiles.Length
                                 + defense.pointDefense.Length);

            return (float) alterations.Count(a => a.slotType != HitLocationSlotType.Decoy) / totalBoxes;
        }
    }
}