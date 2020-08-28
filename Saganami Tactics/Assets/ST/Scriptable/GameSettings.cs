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

        #endregion

        #region Scenes names

        public readonly string SceneMainMenu = "Main Menu";
        public readonly string SceneSetup = "Setup";
        public readonly string SceneDeploy = "Deploy";
        public readonly string ScenePlay = "Play";

        #endregion

        #region Team colors

        public Color BlueTeam = Color.blue;
        public Color YellowTeam = Color.yellow;
        public Color GreenTeam = Color.green;
        public Color MagentaTeam = Color.magenta;

        #endregion Team colors

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
    }
}