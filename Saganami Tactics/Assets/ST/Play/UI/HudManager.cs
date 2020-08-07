using System;
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
#pragma warning restore 649

        private void Awake()
        {
            _gameManager = GetComponent<GameManager>();
            _uiManager = GetComponent<UiManager>();
            _camera = Camera.main;
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
            var ray = _camera.ScreenPointToRay(Input.mousePosition);

            _hoverShip = null; 
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
            if (Input.GetMouseButtonUp(0) && _hoverShip != null)
            {
                _gameManager.SelectedShip = _hoverShip;
            }
        }
    }
}