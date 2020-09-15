using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

namespace ST.Scriptable
{
    [CreateAssetMenu(menuName = "ST/Settings")]
    public class GameSettings : ScriptableObject
    {
        public float MoveDuration = 2f;
        public float BeamDuration = 1f;
        public float MissilesMovementPerSecond = 8f;
        public int MissileShortRange = 6;

        #region Custom properties keys

        public readonly string ColorIndexProp = "cid";
        public readonly string ReadyProp = "rdy";
        public readonly string MaxPointsProp = "pts";
        public readonly string GameStartedProp = "gst";
        public readonly string BluePlayerProp = "bpl";
        public readonly string YellowPlayerProp = "ypl";
        public readonly string GreenPlayerProp = "gpl";
        public readonly string MagentaPlayerProp = "mpl";

        #endregion

        #region Scenes names

        public readonly string SceneMainMenu = "Main Menu";
        public readonly string SceneSetup = "Setup";
        public readonly string SceneDeploy = "Deploy";
        public readonly string ScenePlay = "Play";
        public readonly string SceneEndGame = "EndGame";

        #endregion

        #region Team colors

        public Color BlueTeam = Color.blue;
        public Color YellowTeam = Color.yellow;
        public Color GreenTeam = Color.green;
        public Color MagentaTeam = Color.magenta;

        #endregion Team colors

        #region Ships scales

        public List<ShipScale> shipsScales;

        #endregion Ships scales
        
        [CanBeNull] private static GameSettings _default = null;

        public static GameSettings Default
        {
            get
            {
                if (_default == null)
                {
                    _default = Resources.Load<GameSettings>("GameSettings");
                }

                return _default;
            }
        }

        [Serializable]
        public struct ShipScale
        {
            public ShipCategory category;
            public float scale;
        }
    }
}