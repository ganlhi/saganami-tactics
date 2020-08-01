using System;
using System.Collections.Generic;
using UnityEngine;

namespace ST
{
    [Serializable]
    public class GameSetup
    {
        public int nbPlayers;
        public int maxCost;
    }

    [Serializable]
    public struct ShipState
    {
        public string uid;
        public string name;
        public string ssdName;
        public Team team;
        public ShipStatus status;
        public Vector3 position;
        public Quaternion rotation;
        public Vector3 velocity;
        // TODO add damages

        public static ShipState FromShip(Ship ship)
        {
            return new ShipState()
            {
                uid = ship.uid,
                name = ship.name,
                ssdName = ship.ssdName,
                team = ship.team,
                status = ship.Status,
                position = ship.position,
                rotation = ship.rotation,
                velocity = ship.velocity,
                // TODO add damages
            };
        }
        public static Ship ToShip(ShipState state)
        {
            return new Ship(state.name, state.team, state.ssdName)
            {
                uid = state.uid,
                Status = state.status,
                position = state.position,
                rotation = state.rotation,
                velocity = state.velocity,
                // TODO add damages
            };
        }
    }
    
    
    [Serializable]
    public struct GameState
    {
        public int turn;
        public TurnStep step;
        public List<ShipState> ships;
        // public List<MissileState> Missiles;
        public GameSetup setup;
    }
}