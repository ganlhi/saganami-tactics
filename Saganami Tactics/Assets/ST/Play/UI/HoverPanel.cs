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
        [SerializeField] private TextMeshProUGUI curCurText;
        [SerializeField] private TextMeshProUGUI curFutText;
        [SerializeField] private TextMeshProUGUI futFutText;
        [SerializeField] private GameObject curCurObj;
        [SerializeField] private GameObject curFutObj;
        [SerializeField] private GameObject futFutObj;
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
            curCurObj.SetActive(false);
            curFutObj.SetActive(false);
            futFutObj.SetActive(false);

            if (_shipView != null)
            {
                objectText.text =
                    $"{_shipView.ship.name}\n{_shipView.ship.Ssd.className} ({_shipView.ship.Ssd.category.Code})";

                if (_selectedShip != null)
                {
                    var selectedShipPosition = _selectedShip.transform.position;
                    var targetShipPosition = _shipView.transform.position;
                    var selectedShipFuturePosition = _selectedShip.EndMarker.transform.position;
                    var targetShipFuturePosition = _shipView.EndMarker.transform.position;
                    
                    var curCur = Mathf.CeilToInt(selectedShipPosition.DistanceTo(targetShipPosition));
                    var curFut = Mathf.CeilToInt(selectedShipPosition.DistanceTo(targetShipFuturePosition));
                    var futFut = Mathf.CeilToInt(selectedShipFuturePosition.DistanceTo(targetShipFuturePosition));

                    curCurObj.SetActive(true);
                    curFutObj.SetActive(true);
                    futFutObj.SetActive(true);
                    curCurText.text = curCur.ToString();
                    curFutText.text = curFut.ToString();
                    futFutText.text = futFut.ToString();
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
                        curCurObj.SetActive(true);
                        var distToMissile = Mathf.CeilToInt(
                            _selectedShip.transform.position.DistanceTo(_missileView.transform.position));
                        
                        curCurText.text = distToMissile.ToString();
                    }
                }
            }
        }
    }
}