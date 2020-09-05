using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using ST.Scriptable;
using UnityEngine;

namespace ST
{
    [Serializable]
    public class GameSetup
    {
        public int nbPlayers;
        public int maxCost;
        public string bluePlayer;
        public string yellowPlayer;
        public string greenPlayer;
        public string magentaPlayer;
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
        public List<SsdAlteration> alterations;
        public Dictionary<int, int> consumedAmmo;

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
                alterations = ship.alterations,
                consumedAmmo = ship.consumedAmmo,
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
                alterations = state.alterations ?? new List<SsdAlteration>(),
                consumedAmmo = state.consumedAmmo ?? new Dictionary<int, int>(),
            };
        }
    }
    
    [Serializable]
    public struct MissileState
    {
        public string uid;
        public string attackerId;
        public string targetId;
        public Vector3 launchPoint;
        public bool shortRange;
        public int number;
        public string weaponName;
        public Vector3 position;
        public Quaternion rotation;
        public MissileStatus status;
        public Side hitSide;
        public int attackRange;
        public int updatedAtTurn;

        public static MissileState FromMissile(Missile missile)
        {
            return new MissileState()
            {
                uid = missile.uid,
                attackerId = missile.attackerId,
                targetId = missile.targetId,
                launchPoint = missile.launchPoint,
                shortRange = missile.shortRange,
                number = missile.number,
                weaponName = missile.weapon.name,
                position = missile.position,
                rotation = missile.rotation,
                status = missile.status,
                hitSide = missile.hitSide,
                attackRange = missile.attackRange,
                updatedAtTurn = missile.updatedAtTurn,
            };
        }

        public static Missile ToMissile(MissileState state)
        {
            return new Missile()
            {
                uid = state.uid,
                attackerId = state.attackerId,
                targetId = state.targetId,
                launchPoint = state.launchPoint,
                shortRange = state.shortRange,
                number = state.number,
                weapon = WeaponHelper.GetWeaponByName(state.weaponName),
                position = state.position,
                rotation = state.rotation,
                status = state.status,
                hitSide = state.hitSide,
                attackRange = state.attackRange,
                updatedAtTurn = state.updatedAtTurn,
            };
        }
    }

    [Serializable]
    public class ShipReports
    {
        public string shipUid;
        public Report[] reports;
    }

    [Serializable]
    public struct GameState
    {
        public int turn;
        public TurnStep step;
        public GameSetup setup;
        public List<ShipState> ships;
        public List<MissileState> missiles;
        public List<ShipReports> shipsReports;
    }

    public static class GameStateSaveSystem
    {
        public static void Save(GameState gameState, string gameName)
        {
            var formatter = new BinaryFormatter();
            var path = PathFromGameName(gameName);
            var stream = new FileStream(path, FileMode.Create);

            var data = new GameStateData(gameState);

            formatter.Serialize(stream, data);
            stream.Close();
        }

        public static GameState? Load(string gameName)
        {
            return LoadPath(PathFromGameName(gameName));
        }
        
        private static GameState? LoadPath(string path) 
        {
            if (!File.Exists(path))
            {
                Debug.LogError($"Unable to find save file at path: {path}");
                return null;
            }

            var formatter = new BinaryFormatter();
            var stream = new FileStream(path, FileMode.Open);

            var data = formatter.Deserialize(stream) as GameStateData;
            stream.Close();

            return data?.ToGameState();
        }

        public static List<SaveGameInfo> ListGames()
        {
            var games = new List<SaveGameInfo>();
            
            var paths = Directory.GetFiles(Application.persistentDataPath, "*.stsave", SearchOption.TopDirectoryOnly);

            foreach (var path in paths)
            {
                var gameState = LoadPath(path);
                if (!gameState.HasValue) continue;

                var gameName = Path.GetFileNameWithoutExtension(path);
                
                var info = new SaveGameInfo()
                {
                    GameName = gameName,
                    Date = File.GetLastWriteTime(path),
                    Turn = gameState.Value.turn,
                    NbShips = gameState.Value.ships.Count,
                };
                
                games.Add(info);
            }
            
            games.Sort((a, b) => a.Date.CompareTo(b.Date));
            return games;
        }

        public static void DeleteGame(string gameName)
        {
            var path = PathFromGameName(gameName);
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
        
        public struct SaveGameInfo
        {
            public string GameName;
            public DateTime Date;
            public int Turn;
            public int NbShips;
        }
        
        private static string PathFromGameName(string gameName)
        {
            return Application.persistentDataPath + $"/{gameName}.stsave";
        }

        [Serializable]
        private class GameStateData
        {
            public int turn;
            public TurnStep step;
            public GameSetup setup;
            public ShipStateData[] ships;
            public MissileStateData[] missiles;
            public ShipReports[] shipsReports;
            
            public GameStateData(GameState state)
            {
                turn = state.turn;
                step = state.step;
                setup = state.setup;

                ships = new ShipStateData[state.ships.Count];
                for (var i = 0; i < state.ships.Count; i++)
                {
                    ships[i] = new ShipStateData(state.ships[i]);
                }

                missiles = new MissileStateData[state.missiles.Count];
                for (var i = 0; i < state.missiles.Count; i++)
                {
                    missiles[i] = new MissileStateData(state.missiles[i]);
                }

                shipsReports = state.shipsReports.ToArray();
            }

            public GameState ToGameState()
            {
                return new GameState()
                {
                    turn = this.turn,
                    step = this.step,
                    setup = this.setup,
                    ships = ships.Select(shipStateData => shipStateData.ToShipState()).ToList(),
                    missiles = missiles.Select(missileStateData => missileStateData.ToMissileState()).ToList(),
                    shipsReports = this.shipsReports.ToList(),
                };
            }
        }

        [Serializable]
        private class ShipStateData
        {
            public string uid;
            public string name;
            public string ssdName;
            public Team team;
            public ShipStatus status;
            public float[] position;
            public float[] rotation;
            public float[] velocity;
            public SsdAlteration[] alterations;
            public int[] consumedAmmo;

            public ShipStateData(ShipState state)
            {
                uid = state.uid;
                name = state.name;
                ssdName = state.ssdName;
                team = state.team;
                status = state.status;
                position = new float[3] {state.position.x, state.position.y, state.position.z};
                rotation = new float[4] {state.rotation.x, state.rotation.y, state.rotation.z, state.rotation.w};
                velocity = new float[3] {state.velocity.x, state.velocity.y, state.velocity.z};
                alterations = state.alterations.ToArray();

                consumedAmmo = new int[state.consumedAmmo.Count * 2];
                var i = 0;
                foreach (var kv in state.consumedAmmo)
                {
                    consumedAmmo[i] = kv.Key;
                    consumedAmmo[i + 1] = kv.Value;
                    i += 2;
                }
            }

            public ShipState ToShipState()
            {
                var consumedAmmoDict = new Dictionary<int, int>();
                for (var i = 0; i < consumedAmmo.Length - 1; i += 2)
                {
                    consumedAmmoDict.Add(consumedAmmo[i], consumedAmmo[i + 1]);
                }

                return new ShipState()
                {
                    uid = this.uid,
                    name = this.name,
                    ssdName = this.ssdName,
                    team = this.team,
                    status = this.status,
                    position = new Vector3(this.position[0], this.position[1], this.position[2]),
                    rotation = new Quaternion(this.rotation[0], this.rotation[1], this.rotation[2], this.rotation[3]),
                    velocity = new Vector3(this.velocity[0], this.velocity[1], this.velocity[2]),
                    alterations = this.alterations.ToList(),
                    consumedAmmo = consumedAmmoDict,
                };
            }
        }

        [Serializable]
        private class MissileStateData
        {
            public string uid;
            public string attackerId;
            public string targetId;
            public float[] launchPoint;
            public bool shortRange;
            public int number;
            public string weaponName;
            public float[] position;
            public float[] rotation;
            public MissileStatus status;
            public Side hitSide;
            public int attackRange;
            public int updatedAtTurn;

            public MissileStateData(MissileState state)
            {
                uid = state.uid;
                attackerId = state.attackerId;
                targetId = state.targetId;
                launchPoint = new float[3] {state.launchPoint.x, state.launchPoint.y, state.launchPoint.z};
                shortRange = state.shortRange;
                number = state.number;
                weaponName = state.weaponName;
                position = new float[3] {state.position.x, state.position.y, state.position.z};
                rotation = new float[4] {state.rotation.x, state.rotation.y, state.rotation.z, state.rotation.w};
                status = state.status;
                hitSide = state.hitSide;
                attackRange = state.attackRange;
                updatedAtTurn = state.updatedAtTurn;
            }

            public MissileState ToMissileState()
            {
                return new MissileState()
                {
                    uid = this.uid, 
                    attackerId = this.attackerId, 
                    targetId = this.targetId, 
                    launchPoint = new Vector3(this.launchPoint[0], this.launchPoint[1], this.launchPoint[2]), 
                    shortRange = this.shortRange, 
                    number = this.number, 
                    weaponName = this.weaponName, 
                    position = new Vector3(this.position[0], this.position[1], this.position[2]),
                    rotation = new Quaternion(this.rotation[0], this.rotation[1], this.rotation[2], this.rotation[3]),
                    status = this.status, 
                    hitSide = this.hitSide, 
                    attackRange = this.attackRange, 
                    updatedAtTurn = this.updatedAtTurn, 
                };
            }
        }
    }
}