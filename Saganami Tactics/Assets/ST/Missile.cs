using System;
using ST.Common;
using ST.Scriptable;
using UnityEngine;

namespace ST
{
    [Serializable]
    public struct Missile
    {
        public string uid;
        public string attackerId;
        public string targetId;
        public Vector3 launchPoint;
        public int number;
        public Weapon weapon;
        public Vector3 position;
        public Quaternion rotation;
        public MissileStatus status;
        public Vector3 nextMovePosition;
        
        public Missile(Ship launchShip, TargettingContext context)
        {
            uid = Utils.GenerateId();
            attackerId = launchShip.uid;
            targetId = context.Target.uid;
            launchPoint = context.LaunchPoint;
            number = context.Number;
            weapon = context.Mount.model;
            position = context.LaunchPoint;
            rotation = Quaternion.LookRotation(launchPoint.DirectionTo(context.Target.endMarkerPosition));
            status = MissileStatus.Launched;
            nextMovePosition = context.LaunchPoint;
        }
    }
    
    [Serializable]
    public enum MissileStatus
    {
        Launched,
        Accelerating,
        Missed,
        Destroyed,
        Hitting,
    }
}