using System;
using JetBrains.Annotations;
using ST.Common;
using TMPro;
using UnityEngine;

namespace ST.Play.UI
{
    public class HoverPanel : MonoBehaviour
    {
        [CanBeNull] private ShipView _shipView;
        [CanBeNull] private MissileView _missileView;
        [CanBeNull] private ShipView _selectedShip;

        public ShipView Ship
        {
            get => _shipView;
            set
            {
                _shipView = value;
                _missileView = null;
                UpdateUi();
            }
        }

        public MissileView Missile
        {
            get => _missileView;
            set
            {
                _shipView = null;
                _missileView = value;
                UpdateUi();
            }
        }

        public ShipView SelectedShip
        {
            get => _selectedShip;
            set
            {
                _selectedShip = value;
                UpdateUi();
            }
        }

        private CanvasGroup _canvasGroup;

#pragma warning disable 649
        [SerializeField] private TextMeshProUGUI objectText;
        [SerializeField] private TextMeshProUGUI distancesText;
#pragma warning restore 649

        private void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
        }

        public void SetVisible(bool visible)
        {
            _canvasGroup.alpha = visible ? 1 : 0;
            _canvasGroup.blocksRaycasts = visible;
        }

        private void UpdateUi()
        {
            SetVisible(_shipView != null || _missileView != null);
            distancesText.text = "N/A";

            if (_shipView != null)
            {
                objectText.text =
                    $"{_shipView.ship.name}\n{_shipView.ship.Ssd.className} ({_shipView.ship.Ssd.category.Code})";

                if (_selectedShip != null)
                {
                    var distToShip = Mathf.CeilToInt(_selectedShip.ship.position.DistanceTo(_shipView.ship.position));

                    var distToMarker = Mathf.CeilToInt(
                        _selectedShip.ship.position.DistanceTo(_shipView.EndMarker.transform.position));

                    distancesText.text = $"Current: {distToShip} - Future: {distToMarker}";
                }
            }

            if (_missileView != null)
            {
                var missile = _missileView.missile;
                var attacker = GameManager.GetShipById(missile.attackerId);
                var target = GameManager.GetShipById(missile.targetId);

                if (attacker == null || target == null)
                {
                    SetVisible(false);
                }
                else
                {
                    objectText.text = $"{missile.number} missiles from {attacker.ship.name} to {target.ship.name}";

                    if (_selectedShip != null)
                    {
                        var distToMissile = Mathf.CeilToInt(
                            _selectedShip.ship.position.DistanceTo(_missileView.missile.position));
                        distancesText.text = $"{distToMissile}";
                    }
                }
            }
        }
    }
}