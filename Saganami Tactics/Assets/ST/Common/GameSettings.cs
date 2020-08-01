namespace ST.Common
{
    public static class GameSettings
    {
        public static readonly float MoveDuration = 2f;
        public static readonly float BeamDuration = 1f;
        
        #region Custom properties keys
        public static readonly string ColorIndexProp = "cid";
        public static readonly string ReadyProp = "rdy";
        public static readonly string MaxPointsProp = "pts";
        #endregion

        #region Scenes names
        public static readonly string SceneLauncher = "Launcher";
        public static readonly string SceneInRoom = "InRoom";
        public static readonly string SceneSetup = "Setup";
        public static readonly string SceneDeploy = "Deploy";
        public static readonly string ScenePlay = "Play";
        #endregion
    }
}