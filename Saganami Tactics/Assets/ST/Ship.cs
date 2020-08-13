using System;
using System.Collections.Generic;
using System.Linq;
using Photon.Realtime;
using ST.Common;
using ST.Scriptable;
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
        public Quaternion halfRotation;

        public Ssd Ssd
        {
            get
            {
                if (SsdHelper.AvailableSsds.TryGetValue(ssdName, out var ssd))
                    return ssd;
                
                throw new ArgumentOutOfRangeException(nameof(ssdName), ssdName, "Unknown SSD");
            }
        }

        public List<SsdAlteration> alterations;
        
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
                ApplyPlottings();
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
                ApplyPlottings();
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
                ApplyPlottings();
            }
        }

        public int Thrust
        {
            get => _thrust;
            set
            {
                if (_thrust >= 0 && _thrust <= MaxThrust)
                    _thrust = value;
                ApplyPlottings();
            }
        }

        public Vector3 ThrustVector => halfRotation * Vector3.forward * Thrust;

        public int UsedPivots => Math.Abs(Yaw) + Math.Abs(Pitch);
        public int UsedRolls => Math.Abs(Roll);

        public int MaxPivots => (int) SsdHelper.GetMaxPivot(Ssd, alterations);

        public int MaxRolls => (int) SsdHelper.GetMaxRoll(Ssd, alterations);

        public int MaxThrust => (int) SsdHelper.GetMaxThrust(Ssd, alterations);

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
            halfRotation = Quaternion.identity;
            _status = ShipStatus.Ok;
            _yaw = 0;
            _pitch = 0;
            _roll = 0;
            _thrust = 0;
            alterations = new List<SsdAlteration>();
        }

        public void UpdateFutureMovement()
        {
            position = endMarkerPosition;
            rotation = endMarkerRotation;
            velocity += ThrustVector;
            PlaceMarker();
        }

        public void PlaceMarker()
        {
            endMarkerPosition = position + velocity;
            endMarkerRotation = rotation;
        }
        
        private void ApplyPlottings()
        {
            halfRotation = rotation;
            endMarkerRotation = rotation;

            var yaw = Math.Abs(Yaw);
            var yawSign = Math.Sign(Yaw);
            var pitch = Math.Abs(Pitch);
            var pitchSign = Math.Sign(Pitch);
            var roll = Math.Abs(Roll);
            var rollSign = Math.Sign(Roll);
            
            for (var i = 0; i < yaw; i++)
            {
                endMarkerRotation *= Quaternion.AngleAxis(30f * yawSign, Vector3.up);
                halfRotation *= Quaternion.AngleAxis(15f * yawSign, Vector3.up);
            }
            
            for (var i = 0; i < pitch; i++)
            {
                endMarkerRotation *= Quaternion.AngleAxis(30f * pitchSign, Vector3.right);
                halfRotation *= Quaternion.AngleAxis(15f * pitchSign, Vector3.right);
            }
            
            for (var i = 0; i < roll; i++)
            {
                endMarkerRotation *= Quaternion.AngleAxis(30f * rollSign, Vector3.forward);
                halfRotation *= Quaternion.AngleAxis(15f * rollSign, Vector3.forward);
            }
        }

        public void ApplyDisplacement()
        {
            if (UsedPivots == 0 && Thrust > 0)
            {
                endMarkerPosition = position + velocity + ThrustVector.normalized * (Thrust / 2f);
            }
            else
            {
                endMarkerPosition = position + velocity;
            }
        }

        public void ResetThrustAndPlottings()
        {
            Yaw = 0;
            Pitch = 0;
            Roll = 0;
            Thrust = 0;
            PlaceMarker();
        }

        public Tuple<Side, Side> GetBearingTo(Vector3 targetPosition)
        {
            var direction = position.DirectionTo(targetPosition);
            var forward = rotation * Vector3.forward;
            var right = rotation * Vector3.right;
            var up = rotation * Vector3.up;
            
            var angles = new Dictionary<Side, float>()
            {
                {Side.Forward, Vector3.Angle(forward, direction)},
                {Side.Aft, Vector3.Angle(-forward, direction)},
                {Side.Port, Vector3.Angle(-right, direction)},
                {Side.Starboard, Vector3.Angle(right, direction)},
                {Side.Top, Vector3.Angle(up, direction)},
                {Side.Bottom, Vector3.Angle(-up, direction)},
            };
            
            var ordered = angles.OrderBy(p => p.Value).ToList();
            var first = ordered.First();
            var second = ordered.Skip(1).First();
            
            if (first.Key == Side.Top || first.Key == Side.Bottom) 
                return new Tuple<Side, Side>(first.Key, second.Key);
            
            return new Tuple<Side, Side>(first.Key, first.Key); 
        }
    }
}