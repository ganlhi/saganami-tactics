using System;
using System.Collections.Generic;
using System.Linq;
using ST.Scriptable;
using UnityEngine;

namespace ST.Common.UI
{
    public class SsdCore : MonoBehaviour
    {
        public uint[] StructuralIntegrity
        {
            set
            {
                _structuralIntegrity = value;
                UpdateSi();
            }
        }

        public uint[] Movement
        {
            set
            {
                _movement = value;
                UpdateMvt();
            }
        }

        public uint[] Hull
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
                UpdateCanRepair();
            }
        }

        private bool _canRepair;
        private uint[] _structuralIntegrity;
        private uint[] _movement;
        private uint[] _hull;
        private List<SsdAlteration> _siAlterations = new List<SsdAlteration>();
        private List<SsdAlteration> _mvtAlterations = new List<SsdAlteration>();
        private List<SsdAlteration> _hullAlterations = new List<SsdAlteration>();

        #pragma warning disable 649
                [SerializeField] private SsdBoxes siBoxes;
                [SerializeField] private SsdBoxes mvtBoxes;
                [SerializeField] private SsdBoxes hullBoxes;
        #pragma warning restore 649

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
            hullBoxes.CanRepair = _hullAlterations.Count > 0 && _canRepair;
            hullBoxes.Damages = new Tuple<int, int>(0, _hullAlterations.Count);
        }

        private void UpdateCanRepair()
        {
            hullBoxes.CanRepair = _hullAlterations.Count > 0 && _canRepair;
        }
    }
}