using System;
using UnityEngine;

namespace ST.Play
{
    [RequireComponent(typeof(ShipView))]
    public class ShipVelocityVector : MonoBehaviour
    {
        private ShipView shipView;
        private LineRenderer vectorLine;
        
        private void Awake()
        {
            shipView = GetComponent<ShipView>();
            vectorLine = transform.Find("VelocityVector").GetComponent<LineRenderer>();
        }

        private void Update()
        {
            vectorLine.SetPosition(0, transform.position);
            vectorLine.SetPosition(1, shipView.EndMarker.transform.position);
        }
    }
}