using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ST.Play.UI
{
    public class ShipsList : MonoBehaviour
    {
        private List<ShipView> _ships;

        public List<ShipView> Ships
        {
            get => _ships;
            set
            {
                _ships = value;
                BuildListContent();
            }
        }

        public event EventHandler<ShipView> OnSelectShip;
        public event EventHandler<ShipView> OnFocusCameraOnShip;

        private ShipView _selectedShip;

#pragma warning disable 649
        [SerializeField] private Transform content;
        [SerializeField] private GameObject shipListButtonPrefab;
#pragma warning restore 649

        private void Start()
        {
            foreach (Transform child in content)
            {
                Destroy(child.gameObject);
            }
        }

        private void BuildListContent()
        {
            foreach (Transform child in content)
            {
                Destroy(child.gameObject);
            }

            foreach (var shipView in _ships)
            {
                var btn = Instantiate(shipListButtonPrefab, content).GetComponent<ShipsListButton>();
                btn.ship = shipView.ship;
                btn.Selected = shipView == _selectedShip;
                btn.OnSelect += (sender, args) => { OnSelectShip?.Invoke(this, shipView); };
                btn.OnFocusCamera += (sender, args) => { OnFocusCameraOnShip?.Invoke(this, shipView); };
                var shipLog = shipView.GetComponent<ShipLog>();
                shipLog.OnReportsLogged += (sender, args) =>
                {
                    ReportSeverity? severity = null;
                    if (shipLog.Reports.Any(r => Report.GetSeverity(r.type) == ReportSeverity.Danger))
                    {
                        severity = ReportSeverity.Danger;
                    }
                    else if (shipLog.Reports.Any(r => Report.GetSeverity(r.type) == ReportSeverity.Warning))
                    {
                        severity = ReportSeverity.Warning;
                    }
                    else if (shipLog.Reports.Any(r => Report.GetSeverity(r.type) == ReportSeverity.Info))
                    {
                        severity = ReportSeverity.Info;
                    }
                    
                    btn.UpdateNotification(severity);
                };
            }
        }

        public void SetSelectedShip(ShipView shipView)
        {
            _selectedShip = shipView;
            foreach (Transform child in content)
            {
                var btn = child.GetComponent<ShipsListButton>();

                if (btn == null) continue;

                if (btn.ship.uid == shipView.ship.uid)
                {
                    btn.Selected = true;

                    // Remove any notification dot
                    btn.UpdateNotification(null);
                }
                else if (btn.Selected)
                {
                    btn.Selected = false;
                }
            }
        }
    }
}