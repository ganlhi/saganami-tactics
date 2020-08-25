using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using ST.Scriptable;
using UnityEngine;

namespace ST.Common.UI
{
    public class SsdPanel : MonoBehaviour
    {
        private Ssd _ssd;

        public Ssd Ssd
        {
            get => _ssd;
            set
            {
                if (_ssd != null && _ssd.name == value.name) return;
                _ssd = value;
                UpdateSsd();
            }
        }

        [CanBeNull] private string _shipName;

        [CanBeNull]
        public string ShipName
        {
            get => _shipName;
            set
            {
                _shipName = value;
                if (_ssd != null) SetHeader();
            }
        }

        private List<SsdAlteration> _alterations = new List<SsdAlteration>();

        public List<SsdAlteration> Alterations
        {
            get => _alterations;
            set
            {
                _alterations = value;
                PropagateAlterations();
            }
        }
        
        private Dictionary<int, int> _consumedAmmo = new Dictionary<int, int>();

        public Dictionary<int, int> ConsumedAmmo
        {
            get => _consumedAmmo;
            set
            {
                _consumedAmmo = value;
                PropagateConsumedAmmo();
            }
        }

#pragma warning disable 649
        [SerializeField] private SsdHeader ssdHeader;
        [SerializeField] private SsdCore ssdCore;
        [SerializeField] private Transform ssdWeaponsContent;
        [SerializeField] private SsdWeapon ssdWeaponPrefab;
        [SerializeField] private Transform ssdHitLocationsContent;
        [SerializeField] private SsdHitLocation ssdHitLocationPrefab;
        [SerializeField] private Transform ssdSidesContent;
        [SerializeField] private SsdSide ssdSidePrefab;
#pragma warning restore 649

        private Dictionary<int, SsdHitLocation> _ssdHitLocations;
        private Dictionary<Side, SsdSide> _ssdSides;

        private readonly List<Side> _sides = new List<Side>()
            {Side.Forward, Side.Aft, Side.Port, Side.Starboard, Side.Top, Side.Bottom};


        private void Start()
        {
            if (_ssd != null)
                UpdateSsd();
        }

        private void UpdateSsd()
        {
            SetHeader();
            SetCore();
            SetHitLocations();
            SetSides();
            SetWeapons();
            PropagateAlterations();
        }

        private void PropagateAlterations()
        {
            // Core
            ssdCore.StructuralIntegrityAlterations =
                _alterations.Where(a => a.type == SsdAlterationType.Structural).ToList();
            ssdCore.MovementAlterations = _alterations.Where(a => a.type == SsdAlterationType.Movement).ToList();

            // Hit locations
            for (var i = 0; i < _ssd.hitLocations.Length; i++)
            {
                var locNumber = i + 1;

                if (_ssdHitLocations.TryGetValue(locNumber, out var ssdHitLocation))
                {
                    ssdHitLocation.Alterations = _alterations
                        .Where(a => a.type == SsdAlterationType.Slot && a.location == locNumber).ToList();
                }
            }

            // Sides 
            foreach (var side in _sides)
            {
                if (_ssdSides.TryGetValue(side, out var ssdSide))
                {
                    ssdSide.Alterations = _alterations.Where(a =>
                            a.side == side &&
                            (a.type == SsdAlterationType.Slot || a.type == SsdAlterationType.Sidewall))
                        .ToList();
                }
            }
        }

        private void PropagateConsumedAmmo()
        {
            foreach (var side in _sides)
            {
                if (_ssdSides.TryGetValue(side, out var ssdSide))
                {
                    var weaponMounts = ssdSide.WeaponMountsAndDefenses.Item1;

                    var remainingAmmos = new List<int>();
                    
                    foreach (var weaponMount in weaponMounts)
                    {
                        var remainingAmmo = SsdHelper.GetRemainingAmmo(_ssd, weaponMount, _consumedAmmo);
                        remainingAmmos.Add(remainingAmmo);
                    }
                    
                    ssdSide.RemainingAmmos = remainingAmmos;
                }
            }
        }

        private void SetHeader()
        {
            ssdHeader.ShipName = _shipName ?? _ssd.className;
            ssdHeader.Ssd = _ssd;
        }

        private void SetCore()
        {
            ssdCore.StructuralIntegrity = _ssd.structuralIntegrity;
            ssdCore.Movement = _ssd.movement;
        }

        private void SetWeapons()
        {
            foreach (Transform child in ssdWeaponsContent)
            {
                Destroy(child.gameObject);
            }

            var weapons = new List<Weapon>();
            foreach (var weapon in _ssd.weaponMounts.Select(w => w.model))
            {
                if (!weapons.Contains(weapon)) weapons.Add(weapon);
            }

            foreach (var weapon in weapons)
            {
                var ssdWeapon = Instantiate(ssdWeaponPrefab, ssdWeaponsContent).GetComponent<SsdWeapon>();
                ssdWeapon.Weapon = weapon;
            }
        }

        private void SetHitLocations()
        {
            _ssdHitLocations = new Dictionary<int, SsdHitLocation>();

            foreach (Transform child in ssdHitLocationsContent)
            {
                Destroy(child.gameObject);
            }

            for (var i = 0; i < _ssd.hitLocations.Length; i++)
            {
                var hitLocation = _ssd.hitLocations[i];

                var locNumber = i + 1;

                var ssdHitLocation = Instantiate(ssdHitLocationPrefab, ssdHitLocationsContent)
                    .GetComponent<SsdHitLocation>();
                ssdHitLocation.HitLocation = hitLocation;
                ssdHitLocation.Number = locNumber;

                _ssdHitLocations.Add(locNumber, ssdHitLocation);
            }
        }

        private void SetSides()
        {
            _ssdSides = new Dictionary<Side, SsdSide>();

            foreach (Transform child in ssdSidesContent)
            {
                Destroy(child.gameObject);
            }

            foreach (var side in _sides)
            {
                var ssdSide = Instantiate(ssdSidePrefab, ssdSidesContent).GetComponent<SsdSide>();
                ssdSide.Side = side;
                ssdSide.WeaponMountsAndDefenses = new Tuple<List<WeaponMount>, SideDefenses>(
                    _ssd.weaponMounts.Where(wm => wm.side == side).ToList(),
                    _ssd.defenses.First(sd => sd.side == side)
                );
                _ssdSides.Add(side, ssdSide);
            }
        }
    }
}