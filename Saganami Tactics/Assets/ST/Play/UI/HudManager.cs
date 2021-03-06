using System.Collections.Generic;
using JetBrains.Annotations;
using ST.Common;
using UnityEngine;

namespace ST.Play.UI
{
    [RequireComponent(typeof(GameManager))]
    public class HudManager : MonoBehaviour
    {
        private GameManager _gameManager;
        private UiManager _uiManager;
        private Camera _camera;

        [CanBeNull] private ShipView _hoverShip;
        [CanBeNull] private ShipMarker _hoverEndMarker;
        [CanBeNull] private MissileView _hoverMissile;

#pragma warning disable 649
        [SerializeField] private LayerMask hoverMask;
        [SerializeField] private Transform selectionMarker;
        [SerializeField] private Transform targetMarkersContainer;
        [SerializeField] private TargetMarker targetMarkerPrefab;
#pragma warning restore 649

        private void Awake()
        {
            _gameManager = GetComponent<GameManager>();
            _uiManager = GetComponent<UiManager>();
            _camera = Camera.main;
        }

        private void Start()
        {
            _gameManager.OnTurnStepChange += (sender, step) => RemoveTargetMarkers();
            _gameManager.OnTargetsIdentified += (sender, args) => SetTargetMarkers();
            _gameManager.OnSelectShip += (sender, selection) => SetTargetMarkers();
        }

        private void Update()
        {
            UpdateSelectionMarkerPosition();
            DetectOveredObject();
        }

        private void LateUpdate()
        {
            HandleClick();
        }

        private void UpdateSelectionMarkerPosition()
        {
            if (_gameManager.SelectedShip != null && (_gameManager.SelectedShip.ship.Status == ShipStatus.Ok ||
                                                      _gameManager.SelectedShip.ship.Status == ShipStatus.Surrendered))
            {
                var position = _gameManager.SelectedShip.transform.position;
                var shipDir = _camera.transform.position.DirectionTo(position);

                if (Vector3.Angle(shipDir, _camera.transform.forward) > 90)
                {
                    selectionMarker.gameObject.SetActive(false);
                }
                else
                {
                    var pt = _camera.WorldToScreenPoint(position);

                    selectionMarker.gameObject.SetActive(true);
                    selectionMarker.position = pt;
                }
            }
            else
            {
                selectionMarker.gameObject.SetActive(false);
            }
        }

        private void DetectOveredObject()
        {
            _hoverShip = null;
            _hoverEndMarker = null;
            _hoverMissile = null;

            var ray = _camera.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out var hit, Mathf.Infinity, hoverMask))
            {
                var go = hit.transform.gameObject;

                if (go.TryGetComponent<ShipView>(out var shipView))
                {
                    _hoverShip = shipView;
                }
                else if (go.TryGetComponent<ShipMarker>(out var shipMarker))
                {
                    _hoverEndMarker = shipMarker;
                }
                else if (go.TryGetComponent<MissileView>(out var missileView))
                {
                    _hoverMissile = missileView;
                }
            }

            _uiManager.SetHoverInfo(_hoverShip, _hoverEndMarker, _hoverMissile);
        }

        private void HandleClick()
        {
            if (Input.GetMouseButtonUp(0))
            {
                if (_hoverShip != null) _gameManager.SelectedShip = _hoverShip;
                if (_hoverEndMarker != null) _gameManager.SelectedShip = _hoverEndMarker.shipView;
            } else if (Input.GetMouseButton(1))
            {
                if (_hoverShip != null) _gameManager.LockCameraToShip(_hoverShip);
                if (_hoverEndMarker != null) _gameManager.LockCameraToShipMarker(_hoverEndMarker);
                if (_hoverMissile != null) _gameManager.LockCameraToMissile(_hoverMissile);
            }
        }

        private void SetTargetMarkers()
        {
            RemoveTargetMarkers();

            if (_gameManager.Step != TurnStep.Targeting) return;
            if (!_gameManager.SelectedShip.OwnedByClient) return;

            var fcon = _gameManager.SelectedShip.GetComponent<FireControl>();

            var targetMarkersByShip = new Dictionary<string, TargetMarker>();

            foreach (var potentialTarget in fcon.PotentialTargets)
            {
                var shipId = potentialTarget.Target.uid;

                if (targetMarkersByShip.ContainsKey(shipId))
                {
                    targetMarkersByShip[shipId].targetingContexts.Add(potentialTarget);
                }
                else
                {
                    var targetMarker = Instantiate(targetMarkerPrefab, targetMarkersContainer)
                        .GetComponent<TargetMarker>();

                    targetMarker.fcon = fcon;
                    targetMarker.shipView = GameManager.GetShipById(shipId);
                    targetMarker.targetingContexts = new List<TargetingContext>() {potentialTarget};

                    targetMarker.OnLockTarget += (sender, tuple) =>
                    {
                        foreach (Transform child in targetMarkersContainer)
                        {
                            var otherTargetMarker = child.GetComponent<TargetMarker>();
                            if (otherTargetMarker != targetMarker)
                                otherTargetMarker.UpdateUi();
                        }
                    };

                    targetMarkersByShip.Add(shipId, targetMarker);
                }
            }
        }

        private void RemoveTargetMarkers()
        {
            foreach (Transform child in targetMarkersContainer)
            {
                Destroy(child.gameObject);
            }
        }
    }
}