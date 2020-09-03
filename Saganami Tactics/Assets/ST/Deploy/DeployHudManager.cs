using JetBrains.Annotations;
using ST.Common;
using ST.Play;
using UnityEngine;

namespace ST.Deploy
{
    [RequireComponent(typeof(DeployManager))]
    public class DeployHudManager : MonoBehaviour
    {
        private DeployManager _deployManager;
        private Camera _camera;
        [CanBeNull] private ShipView _hoverShip;

#pragma warning disable 649
        [SerializeField] private LayerMask hoverMask;
        [SerializeField] private Transform selectionMarker;
#pragma warning restore 649

        private void Awake()
        {
            _deployManager = GetComponent<DeployManager>();
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
            if (_deployManager.SelectedShip != null && (_deployManager.SelectedShip.ship.Status == ShipStatus.Ok ||
                                                      _deployManager.SelectedShip.ship.Status == ShipStatus.Surrendered))
            {
                var position = _deployManager.SelectedShip.transform.position;
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

            if (Physics.Raycast(ray, out var hit, Mathf.Infinity, hoverMask))
            {
                var go = hit.transform.gameObject;

                if (go.TryGetComponent<ShipView>(out var shipView))
                {
                    _hoverShip = shipView;
                }
            }
        }

        private void HandleClick()
        {
            if (Input.GetMouseButtonUp(0) && _hoverShip != null)
            {
                _deployManager.SelectedShip = _hoverShip;
            }
        }
    }
}