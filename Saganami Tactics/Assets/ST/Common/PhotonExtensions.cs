using System;
using ExitGames.Client.Photon;
using Photon.Realtime;
using UnityEngine;

namespace ST.Common
{
    public static class PlayerExtensions
    {

        public static Team? GetTeam(this Player player)
        {
            try
            {
                Debug.Log("Player color index " + player.GetColorIndex());
                return TeamHelper.FromIndex(player.GetColorIndex());
            }
            catch (ArgumentOutOfRangeException)
            {
                return null;
            }
        }
        
        public static int GetColorIndex(this Player player)
        {
            if (player.CustomProperties.TryGetValue(GameSettings.ColorIndexProp, out var idx))
            {
                return (int)idx;
            }
            else
            {
                return 0;
            }
        }

        public static void CycleColorIndex(this Player player)
        {
            var index = player.GetColorIndex();
            Team team;
            try
            {
                team = TeamHelper.FromIndex(index).Next();
            }
            catch (ArgumentOutOfRangeException)
            {
                team = Team.Blue;
            }

            var newIndex = team.ToIndex();
            Debug.Log("Cycling to team "+team+ " / " + newIndex);
            
            if (!player.SetCustomProperties(new Hashtable()
            {
                {GameSettings.ColorIndexProp, newIndex},
            }))
            {
                Debug.LogError("Failed to cycle color index");
            }
            else
            {
                Debug.Log(player.CustomProperties.ToStringFull());
                Debug.Log("Cycled to index "+player.GetColorIndex());
            }
        }

        public static bool IsReady(this Player player)
        {
            if (player.CustomProperties.TryGetValue(GameSettings.ReadyProp, out var rdy))
            {
                return (bool)rdy;
            }
            else
            {
                return false;
            }
        }

        public static void SetReady(this Player player, bool ready = true)
        {
            player.SetCustomProperties(new Hashtable() {
                { GameSettings.ReadyProp, ready },
            });
            
            Debug.Log("SetReady " + player.CustomProperties.ToStringFull());
        }
    }

    public static class RoomInfoExtensions
    {
        public static int GetMaxPoints(this RoomInfo room)
        {
            if (room.CustomProperties.TryGetValue(GameSettings.MaxPointsProp, out var points))
            {
                return (int)points;
            }
            return 0;
        }
    }

    public static class RoomExtensions
    {
        public static void ResetPlayersReadiness(this Room room)
        {
            foreach (var p in room.Players.Values)
            {
                p.SetReady(false);
            }
        }
    }
}