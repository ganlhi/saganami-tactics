using System;
using ST.Common;
using UnityEngine;

namespace ST.Play
{
    [RequireComponent(typeof(ShipView))]
    public class ShipVelocityVector : MonoBehaviour
    {
        private ShipView _shipView;
        private LineRenderer _vectorLine;
        private Camera _camera;
        [SerializeField] private float widthCoefficient = 0.004f;

        private void Awake()
        {
            _camera = Camera.main;
            _shipView = GetComponent<ShipView>();
            _vectorLine = transform.Find("VelocityVector").GetComponent<LineRenderer>();
        }

        private void Update()
        {
            _vectorLine.SetPosition(0, transform.position);
            _vectorLine.SetPosition(1, _shipView.EndMarker.transform.position);
            
            if (_camera == null) return;
            
            var camPos = _camera.transform.position;
            _vectorLine.startWidth = camPos.DistanceTo(transform.position) * widthCoefficient;
            _vectorLine.endWidth = camPos.DistanceTo(_shipView.EndMarker.transform.position) * widthCoefficient;
        }
    }
}