using System;
using ST.Common;
using UnityEngine;

namespace ST.Play
{
    [RequireComponent(typeof(ShipView))]
    public class ShipVelocityVector : MonoBehaviour
    {
        private ShipView shipView;
        private LineRenderer vectorLine;
        [SerializeField] private float widthCoefficient = 0.004f;
        
        private void Awake()
        {
            shipView = GetComponent<ShipView>();
            vectorLine = transform.Find("VelocityVector").GetComponent<LineRenderer>();
        }

        private void Update()
        {
            vectorLine.SetPosition(0, transform.position);
            vectorLine.SetPosition(1, shipView.EndMarker.transform.position);
            if (Camera.main != null)
            {
                var camPos = Camera.main.transform.position;
                vectorLine.startWidth = camPos.DistanceTo(transform.position) * widthCoefficient;
                vectorLine.endWidth = camPos.DistanceTo(shipView.EndMarker.transform.position) * widthCoefficient;
            }
        }
    }
}