using System;
using System.Linq;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using ST.Scriptable;
using UnityEngine;

namespace ST.Common
{
    public static class PlayerExtensions
    {
        public static Team? GetTeam(this Player player)
        {
            try
            {
                return TeamHelper.FromIndex(player.GetColorIndex());
            }
            catch (ArgumentOutOfRangeException)
            {
                return null;
            }
        }

        public static int GetColorIndex(this Player player)
        {
            if (player?.CustomProperties != null &&
                player.CustomProperties.TryGetValue(GameSettings.Default.ColorIndexProp, out var idx))
            {
                return (int) idx;
            }
            else
            {
                return 0;
            }
        }

        public static void SetColorIndex(this Player player, int colorIndex)
        {
            // Throws exception if index is out of range
            TeamHelper.FromIndex(colorIndex);
            
            // Index is valid
            if (!player.SetCustomProperties(new Hashtable()
            {
                {GameSettings.Default.ColorIndexProp, colorIndex},
            }))
            {
                Debug.LogError("Failed to cycle color index");
            }
        }

        public static bool AssignFirstAvailableColorIndex(this Player player)
        {
            var allPlayers = PhotonNetwork.CurrentRoom.Players.Values.ToList();
            var usedColorIndices = allPlayers.Where(p => !Equals(p, player)).Select(p => p.GetColorIndex()).ToList();
            
            for (var i = 1; i <= 4; i++)
            {
                if (usedColorIndices.Contains(i)) continue;
                player.SetColorIndex(i);
                return true;
            }
            
            Debug.LogError("Unable to assign a color index to player");
            return false;
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

            if (!player.SetCustomProperties(new Hashtable()
            {
                {GameSettings.Default.ColorIndexProp, newIndex},
            }))
            {
                Debug.LogError("Failed to cycle color index");
            }
        }

        public static bool IsReady(this Player player)
        {
            if (player.CustomProperties.TryGetValue(GameSettings.Default.ReadyProp, out var rdy))
            {
                return (bool) rdy;
            }
            else
            {
                return false;
            }
        }

        public static void SetReady(this Player player, bool ready = true)
        {
            player.SetCustomProperties(new Hashtable()
            {
                {GameSettings.Default.ReadyProp, ready},
            });
        }
    }

    public static class RoomInfoExtensions
    {
        public static int GetMaxPoints(this RoomInfo room)
        {
            if (room.CustomProperties.TryGetValue(GameSettings.Default.MaxPointsProp, out var points))
            {
                return (int) points;
            }

            return 0;
        }
        
        public static bool IsGameStarted(this RoomInfo room)
        {
            if (room.CustomProperties.TryGetValue(GameSettings.Default.GameStartedProp, out var started))
            {
                return (bool) started;
            }

            return false;
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