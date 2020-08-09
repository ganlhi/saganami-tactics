using System;
using ST.Common;
using UnityEngine;

namespace ST.Play
{
    [RequireComponent(typeof(ShipView))]
    public class ShipVerticalMarker : MonoBehaviour
    {
        private ShipView _shipView;
        private LineRenderer _verticalLine;
        private LineRenderer _hex;
        private Vector3[] _hexLinePositions;
        private Camera _camera;
        [SerializeField] private float widthCoefficient = 0.004f;

        private void Awake()
        {
            _shipView = GetComponent<ShipView>();
            _verticalLine = transform.Find("VerticalLine").GetComponent<LineRenderer>();
            _hex = transform.Find("Hex").GetComponent<LineRenderer>();
            _camera = Camera.main;
        }

        private void Start()
        {
            _hexLinePositions = new Vector3[_hex.positionCount];
            _hex.GetPositions(_hexLinePositions);
        }

        private void Update()
        {
            var p = transform.position;
            var p0 = new Vector3(p.x, 0, p.z);

            for (var i = 0; i < _hex.positionCount; i++)
            {
                _hex.SetPosition(i, _hexLinePositions[i] + p0);
            }
            
            _verticalLine.SetPosition(0, p);
            _verticalLine.SetPosition(1, p0);
            
            var color = _shipView.ship.team.ToColor();
            _hex.startColor = color;
            _hex.endColor = color;
            _verticalLine.startColor = color;
            _verticalLine.endColor = color;

            if (_camera == null) return;
            
            var camPos = _camera.transform.position;
            _verticalLine.startWidth = camPos.DistanceTo(p) * widthCoefficient;
            _verticalLine.endWidth = camPos.DistanceTo(p0) * widthCoefficient;
                
            _hex.startWidth = camPos.DistanceTo(p0) * widthCoefficient;
            _hex.endWidth = camPos.DistanceTo(p0) * widthCoefficient;
        }
    }
}