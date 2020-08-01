using System;
using UnityEngine;

namespace ST
{
    [Serializable]
    public struct Ship
    {
        public string uid;
        public string name;
        public Team team;
        public string ssdName;
        
        public Vector3 position;
        public Quaternion rotation;
        
        public Vector3 velocity;
        public Vector3 thrust;
        
        public Vector3 endMarkerPosition;
        public Quaternion endMarkerRotation;

        private ShipStatus _status;

        public ShipStatus Status
        {
            get => _status;
            set
            {
                if (Equals(_status, value)) return;
                _status = value;
            }
        }

        public Ship(string name, Team team, string ssdName)
        {
            uid = Utils.GenerateId();
            this.name = name;
            this.team = team;
            this.ssdName = ssdName;
            position = Vector3.zero;
            rotation = Quaternion.identity;
            velocity = Vector3.zero;
            thrust = Vector3.zero;
            endMarkerPosition = Vector3.zero;
            endMarkerRotation = Quaternion.identity;
            _status = ShipStatus.Ok;
        }
        
        public void UpdateFutureMovement()
        {
            velocity += thrust;
            PlaceMarker();
        }

        public void PlaceMarker()
        {
            endMarkerPosition = position + velocity;
            endMarkerRotation = rotation; // TODO apply plotted pivots and rolls;
        }

        public void MoveToMarker()
        {
            position = endMarkerPosition;
            rotation = endMarkerRotation;
        }

        public void ResetThrustAndPlottings()
        {
            thrust = Vector3.zero;
            // TODO reset plotting
        }
    }
}