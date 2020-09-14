using System;
using System.Collections.Generic;
using System.Linq;
using ST.Scriptable;
using TMPro;
using UnityEngine;

namespace ST.Common.UI
{
    public class SsdCore : MonoBehaviour
    {
        public int[] StructuralIntegrity
        {
            set
            {
                _structuralIntegrity = value;
                UpdateSi();
            }
        }

        public int[] Movement
        {
            set
            {
                _movement = value;
                UpdateMvt();
            }
        }

        public int[] Hull
        {
            set
            {
                _hull = value;
                UpdateHull();
            }
        }

        public List<SsdAlteration> StructuralIntegrityAlterations
        {
            set
            {
                _siAlterations = value;
                UpdateSi();
            }
        }

        public List<SsdAlteration> MovementAlterations
        {
            set
            {
                _mvtAlterations = value;
                UpdateMvt();
            }
        }

        public List<SsdAlteration> HullAlterations
        {
            set
            {
                _hullAlterations = value;
                UpdateHull();
            }
        }
        
        public bool CanRepair
        {
            get => _canRepair;
            set
            {
                _canRepair = value;
                UpdateHull();
            }
        }
        
        public int DecoysStrength
        {
            get => _decoysStrength;
            set
            {
                _decoysStrength = value;
                decoysStrengthText.text = _decoysStrength.ToString();
            }
        }

        public event EventHandler OnRepairHull;

        private bool _canRepair;
        private int _decoysStrength;
        private int[] _structuralIntegrity;
        private int[] _movement;
        private int[] _hull;
        private List<SsdAlteration> _siAlterations = new List<SsdAlteration>();
        private List<SsdAlteration> _mvtAlterations = new List<SsdAlteration>();
        private List<SsdAlteration> _hullAlterations = new List<SsdAlteration>();

        #pragma warning disable 649
                [SerializeField] private SsdBoxes siBoxes;
                [SerializeField] private SsdBoxes mvtBoxes;
                [SerializeField] private SsdBoxes hullBoxes;
                [SerializeField] private TMP_Text decoysStrengthText;
        #pragma warning restore 649

        private void Start()
        {
            hullBoxes.OnRepair += (sender, args) =>
            {
                OnRepairHull?.Invoke(this, EventArgs.Empty);
                
            };
        }

        private void UpdateSi()
        {
            siBoxes.CurrentValue = (int) SsdHelper.GetUndamagedValue(_structuralIntegrity, _siAlterations.Count);
            siBoxes.CanRepair = false;
            siBoxes.Damages = new Tuple<int, int>(_siAlterations.Count, 0);
        }
        
        private void UpdateMvt()
        {
            mvtBoxes.CurrentValue = (int) SsdHelper.GetUndamagedValue(_movement, _mvtAlterations.Count);
            mvtBoxes.CanRepair = false;
            mvtBoxes.Damages = new Tuple<int, int>(0, _mvtAlterations.Count);
        }
        
        private void UpdateHull()
        {
            hullBoxes.CurrentValue = (int) SsdHelper.GetUndamagedValue(_hull, _hullAlterations.Count);

            var nbDestroyed = _hullAlterations.Count(a => a.destroyed);
            var nbDamaged = _hullAlterations.Count(a => !a.destroyed);

            hullBoxes.CanRepair = nbDamaged > 0 && _canRepair;
            hullBoxes.Damages = new Tuple<int, int>(nbDestroyed, nbDamaged);
        }
    }
}