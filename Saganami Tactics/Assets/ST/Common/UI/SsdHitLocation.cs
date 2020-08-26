using System;
using System.Collections.Generic;
using System.Linq;
using ST.Scriptable;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ST.Common.UI
{
    public class SsdHitLocation : MonoBehaviour
    {
        public int Number
        {
            set => numberText.text = value.ToString();
        }

        private HitLocation _hitLocation;

        public HitLocation HitLocation
        {
            set
            {
                _hitLocation = value;
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

        private bool _canRepair;

        public bool CanRepair
        {
            get => _canRepair;
            set
            {
                _canRepair = value;
                UpdateCanRepair();
            }
        }

        public event EventHandler<HitLocationSlot> OnRepair; 

#pragma warning disable 649
        [SerializeField] private TextMeshProUGUI numberText;
        [SerializeField] private TextMeshProUGUI coreArmorText;
        [SerializeField] private GameObject coreArmor;
        [SerializeField] private Transform slotsContent;
        [SerializeField] private SsdSlot ssdSlotPrefab;
        [SerializeField] private Image overflowImage;
        [SerializeField] private Sprite passThroughIcon;
        [SerializeField] private Sprite structuralIcon;
#pragma warning restore 649

        private SsdSlot[] _ssdSlots;

        private void SetupUi()
        {
            coreArmor.SetActive(_hitLocation.coreArmor > 0);
            coreArmorText.text = _hitLocation.coreArmor.ToString();

            overflowImage.gameObject.SetActive(_hitLocation.passThrough || _hitLocation.structural);
            if (_hitLocation.passThrough)
            {
                overflowImage.sprite = passThroughIcon;
            }
            else if (_hitLocation.structural)
            {
                overflowImage.sprite = structuralIcon;
            }

            _ssdSlots = new SsdSlot[_hitLocation.slots.Length];

            foreach (Transform child in slotsContent)
            {
                Destroy(child.gameObject);
            }

            var i = 0;
            foreach (var slot in _hitLocation.slots)
            {
                var ssdSlot = Instantiate(ssdSlotPrefab, slotsContent).GetComponent<SsdSlot>();
                ssdSlot.Slot = slot;

                ssdSlot.OnRepair += (sender, args) => OnRepair.Invoke(this, slot);

                _ssdSlots[i] = ssdSlot;
                i++;
            }
        }

        private void UpdateUi()
        {
            for (var i = 0; i < _hitLocation.slots.Length; i++)
            {
                var ssdSlot = _ssdSlots[i];
                var slot = _hitLocation.slots[i];
                ssdSlot.Alterations = _alterations.Where(a => a.slotType == slot.type).ToList();
            }
        }

        private void UpdateCanRepair()
        {
            for (var i = 0; i < _hitLocation.slots.Length; i++)
            {
                var ssdSlot = _ssdSlots[i];
                var slot = _hitLocation.slots[i];
                ssdSlot.CanRepair = _canRepair;
            }
        }
    }
}