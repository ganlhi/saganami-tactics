using System;
using ST.Common;
using UnityEngine;

namespace ST.Play
{
    [RequireComponent(typeof(ShipView))]
    public class ShipVerticalMarker : MonoBehaviour
    {
        private ShipView shipView;
        private LineRenderer verticalLine;
        private LineRenderer hex;
        private Vector3[] hexLinePositions; 
        [SerializeField] private float widthCoefficient = 0.004f;
        
        private void Awake()
        {
            shipView = GetComponent<ShipView>();
            verticalLine = transform.Find("VerticalLine").GetComponent<LineRenderer>();
            hex = transform.Find("Hex").GetComponent<LineRenderer>();
        }

        private void Start()
        {
            hexLinePositions = new Vector3[hex.positionCount];
            hex.GetPositions(hexLinePositions);
        }

        private void Update()
        {
            var p = transform.position;
            var p0 = new Vector3(p.x, 0, p.z);

            for (var i = 0; i < hex.positionCount; i++)
            {
                hex.SetPosition(i, hexLinePositions[i] + p0);
            }
            
            verticalLine.SetPosition(0, p);
            verticalLine.SetPosition(1, p0);
            
            var color = shipView.ship.team.ToColor();
            hex.startColor = color;
            hex.endColor = color;
            verticalLine.startColor = color;
            verticalLine.endColor = color;
            
            if (Camera.main != null)
            {
                var camPos = Camera.main.transform.position;
                verticalLine.startWidth = camPos.DistanceTo(p) * widthCoefficient;
                verticalLine.endWidth = camPos.DistanceTo(p0) * widthCoefficient;
                
                hex.startWidth = camPos.DistanceTo(p0) * widthCoefficient;
                hex.endWidth = camPos.DistanceTo(p0) * widthCoefficient;
            }
        }
    }
}