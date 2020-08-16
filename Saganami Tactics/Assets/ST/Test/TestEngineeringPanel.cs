using System;
using System.Collections.Generic;
using ST.Common.UI;
using ST.Scriptable;
using UnityEngine;

namespace ST.Test
{
    public class TestEngineeringPanel : MonoBehaviour
    {
        private SsdPanel _ssdPanel;

        private void Awake()
        {
            _ssdPanel = GetComponent<SsdPanel>();
        }

        public void SetSsd(string ssdName)
        {
            if (ssdName == null) return;
            _ssdPanel.Ssd = SsdHelper.AvailableSsds[ssdName];
            _ssdPanel.Alterations = new List<SsdAlteration>();
        }

        public void MakeDamages()
        {
            var alterations = new List<SsdAlteration>();

            alterations.Add(new SsdAlteration()
            {
                destroyed = true,
                type = SsdAlterationType.Structural,
            });

            for (var i = 0; i < 2; i++)
            {
                alterations.Add(new SsdAlteration()
                {
                    type = SsdAlterationType.Movement
                });
            }
            
            alterations.Add(new SsdAlteration()
            {
                destroyed = true,
                type = SsdAlterationType.Sidewall,
                side = Side.Port
            });
            
            alterations.Add(new SsdAlteration()
            {
                type = SsdAlterationType.Slot,
                slotType = HitLocationSlotType.CounterMissile,
                side = Side.Port
            });

            for (var i = 0; i < 2; i++)
            {
                alterations.Add(new SsdAlteration()
                {
                    type = SsdAlterationType.Slot,
                    slotType = HitLocationSlotType.Laser,
                    side = Side.Starboard
                });
            }
            
            for (var i = 0; i < 2; i++)
            {
                alterations.Add(new SsdAlteration()
                {
                    type = SsdAlterationType.Slot,
                    slotType = HitLocationSlotType.ForwardImpeller,
                    location = 3
                });
            }

            _ssdPanel.Alterations = alterations;
        }
    }
}