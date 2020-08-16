using System;
using System.Collections.Generic;
using System.Linq;
using ST.Scriptable;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ST.Common.UI
{
    public class SsdSide : MonoBehaviour
    {
        public Side Side
        {
            set => sideText.text = value.ToFriendlyString();
        }

        private List<WeaponMount> _weaponMounts;
        private SideDefenses _sideDefenses;

        public Tuple<List<WeaponMount>, SideDefenses> WeaponMountsAndDefenses
        {
            set
            {
                _weaponMounts = value.Item1;
                _sideDefenses = value.Item2;
                SetupUi();
            }
        }

        private List<SsdAlteration> _alterations = new List<SsdAlteration>();

        public List<SsdAlteration> Alterations
        {
            set
            {
                _alterations = value;
                UpdateUi();
            }
        }

#pragma warning disable 649
        [SerializeField] private float minHeight = 95f;
        [SerializeField] private float lineHeight = 25f;
        [SerializeField] private TextMeshProUGUI sideText;
        [SerializeField] private TextMeshProUGUI armorText;
        [SerializeField] private GameObject armor;
        [SerializeField] private GameObject wedge;
        [SerializeField] private Transform slotsContent;
        [SerializeField] private SsdWeaponMount ssdWeaponMountPrefab;
        [SerializeField] private SsdSideDefense ssdSideDefensePrefab;
#pragma warning restore 649

        private SsdWeaponMount[] _ssdWeaponMounts;
        private Dictionary<string, SsdSideDefense> _ssdSideDefenses;

        private void SetupUi()
        {
            armor.SetActive(_sideDefenses.armorStrength > 0);
            armorText.text = _sideDefenses.armorStrength.ToString();
            wedge.SetActive(_sideDefenses.wedge);

            foreach (Transform child in slotsContent)
            {
                Destroy(child.gameObject);
            }

            _ssdWeaponMounts = new SsdWeaponMount[_weaponMounts.Count];
            _ssdSideDefenses = new Dictionary<string, SsdSideDefense>();

            var i = 0;
            foreach (var weaponMount in _weaponMounts)
            {
                var ssdWeaponMount = Instantiate(ssdWeaponMountPrefab, slotsContent).GetComponent<SsdWeaponMount>();
                ssdWeaponMount.WeaponMount = weaponMount;

                _ssdWeaponMounts[i] = ssdWeaponMount;
                i++;
            }

            var nbDefenses = 0;

            if (_sideDefenses.sidewall != null && _sideDefenses.sidewall.Any())
            {
                nbDefenses++;
                var ssdSideDefense = Instantiate(ssdSideDefensePrefab, slotsContent).GetComponent<SsdSideDefense>();
                ssdSideDefense.Name = "Sidewall";
                ssdSideDefense.Boxes = _sideDefenses.sidewall;

                _ssdSideDefenses.Add("sidewall", ssdSideDefense);
            }

            if (_sideDefenses.counterMissiles != null && _sideDefenses.counterMissiles.Any())
            {
                nbDefenses++;
                var ssdSideDefense = Instantiate(ssdSideDefensePrefab, slotsContent).GetComponent<SsdSideDefense>();
                ssdSideDefense.Name = "Counter Missiles";
                ssdSideDefense.Boxes = _sideDefenses.counterMissiles;

                _ssdSideDefenses.Add("counterMissiles", ssdSideDefense);
            }

            if (_sideDefenses.pointDefense != null && _sideDefenses.pointDefense.Any())
            {
                nbDefenses++;
                var ssdSideDefense = Instantiate(ssdSideDefensePrefab, slotsContent).GetComponent<SsdSideDefense>();
                ssdSideDefense.Name = "Point Defenses";
                ssdSideDefense.Boxes = _sideDefenses.pointDefense;

                _ssdSideDefenses.Add("pointDefense", ssdSideDefense);
            }

            var rt = GetComponent<RectTransform>();
            var nbLines = _weaponMounts.Count + nbDefenses;
            rt.sizeDelta = new Vector2(rt.sizeDelta.x, Mathf.Max(nbLines * lineHeight + 5f, minHeight));
        }

        private void UpdateUi()
        {
            // Weapon Mounts
            for (var i = 0; i < _weaponMounts.Count; i++)
            {
                var weaponMount = _weaponMounts[i];

                var slotType = weaponMount.model.type == WeaponType.Missile
                    ? HitLocationSlotType.Missile
                    : HitLocationSlotType.Laser;

                var ssdWeaponMount = _ssdWeaponMounts[i];

                ssdWeaponMount.Alterations = _alterations
                    .Where(a => a.type == SsdAlterationType.Slot && a.slotType == slotType).ToList();
            }

            // Side defenses
            if (_ssdSideDefenses.TryGetValue("sidewall", out var ssdSidewall))
            {
                ssdSidewall.Alterations = _alterations.Where(a => a.type == SsdAlterationType.Sidewall).ToList();
            }

            if (_ssdSideDefenses.TryGetValue("counterMissiles", out var ssdCounterMissiles))
            {
                ssdCounterMissiles.Alterations = _alterations.Where(a =>
                    a.type == SsdAlterationType.Slot && a.slotType == HitLocationSlotType.CounterMissile).ToList();
            }

            if (_ssdSideDefenses.TryGetValue("pointDefense", out var ssdPointDefense))
            {
                ssdPointDefense.Alterations = _alterations.Where(a =>
                    a.type == SsdAlterationType.Slot && a.slotType == HitLocationSlotType.PointDefense).ToList();
            }
        }
    }
}