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

        #region Plotting

        private int _yaw;
        private int _pitch;
        private int _roll;
        private int _thrust;

        public int Yaw
        {
            get => _yaw;
            set
            {
                var prev = _yaw;
                _yaw = value;
                if (UsedPivots > MaxPivots) _yaw = prev;
            }
        }

        public int Pitch
        {
            get => _pitch;
            set
            {
                var prev = _pitch;
                _pitch = value;
                if (UsedPivots > MaxPivots) _pitch = prev;
            }
        }

        public int Roll
        {
            get => _roll;
            set
            {
                var prev = _roll;
                _roll = value;
                if (UsedRolls > MaxRolls) _roll = prev;
            }
        }

        public int Thrust
        {
            get => _thrust;
            set
            {
                if (_thrust >= 0 && _thrust <= MaxThrust)
                    _thrust = value;
            }
        }

        public Vector3 ThrustVector => (rotation * Vector3.forward) * Thrust;

        public int UsedPivots => Math.Abs(Yaw) + Math.Abs(Pitch);
        public int UsedRolls => Math.Abs(Roll);

        public int MaxPivots
        {
            get => 6;
        } // TODO

        public int MaxRolls
        {
            get => 6;
        } // TODO

        public int MaxThrust
        {
            get => 3;
        } // TODO

        #endregion Plotting

        public Ship(string name, Team team, string ssdName)
        {
            uid = Utils.GenerateId();
            this.name = name;
            this.team = team;
            this.ssdName = ssdName;
            position = Vector3.zero;
            rotation = Quaternion.identity;
            velocity = Vector3.zero;
            endMarkerPosition = Vector3.zero;
            endMarkerRotation = Quaternion.identity;
            _status = ShipStatus.Ok;
            _yaw = 0;
            _pitch = 0;
            _roll = 0;
            _thrust = 0;
        }

        public void UpdateFutureMovement()
        {
            velocity += ThrustVector;
            PlaceMarker();
        }

        public void PlaceMarker()
        {
            endMarkerPosition = position + velocity;

            endMarkerRotation = rotation *
                                Quaternion.AngleAxis(30f * Yaw, Vector3.up) *
                                Quaternion.AngleAxis(30f * Pitch, Vector3.right) *
                                Quaternion.AngleAxis(30f * Roll, Vector3.forward);
        }

        public void ApplyDisplacement()
        {
            if (UsedPivots == 0 && Thrust > 0)
            {
                endMarkerPosition = position + velocity + ThrustVector.normalized * (Thrust / 2f);
            }
        }

        public void MoveToMarker()
        {
            position = endMarkerPosition;
            rotation = endMarkerRotation;
        }

        public void ResetThrustAndPlottings()
        {
            Yaw = 0;
            Pitch = 0;
            Roll = 0;
            Thrust = 0;
            PlaceMarker();
        }
    }
}