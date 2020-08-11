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

        // TODO handle missiles
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
            _gameManager.OnSelectShip += (sender, ship) => SetTargetMarkers();
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
            if (_gameManager.SelectedShip != null)
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

            var ray = _camera.ScreenPointToRay(Input.mousePosition);
            // TODO handle missiles

            if (Physics.Raycast(ray, out var hit, Mathf.Infinity, hoverMask))
            {
                var go = hit.transform.gameObject;

                if (go.TryGetComponent<ShipView>(out var shipView))
                {
                    _hoverShip = shipView;
                }

                // TODO
//            else if (go.TryGetComponent<MissilesView>(out var missilesView))
//            {
//                
//            }
            }

            _uiManager.SetHoverShipInfo(_hoverShip);
            // TODO handle missiles
        }

        private void HandleClick()
        {
            // When in targeting mode, no selection via the HUD
            if (_gameManager.Step == TurnStep.Targeting) return;
            
            if (Input.GetMouseButtonUp(0) && _hoverShip != null)
            {
                _gameManager.SelectedShip = _hoverShip;
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
                    targetMarkersByShip[shipId].targettingContexts.Add(potentialTarget);
                }
                else
                {
                    var targetMarker = Instantiate(targetMarkerPrefab, targetMarkersContainer)
                        .GetComponent<TargetMarker>();

                    targetMarker.fcon = fcon;
                    targetMarker.shipView = _gameManager.GetShipById(shipId);
                    targetMarker.targettingContexts = new List<TargettingContext>() {potentialTarget};

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