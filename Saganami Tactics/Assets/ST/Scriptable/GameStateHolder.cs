using UnityEngine;

namespace ST.Scriptable
{
    [CreateAssetMenu(menuName = "ST/GameStateHolder")]
    public class GameStateHolder : ScriptableObject
    {
        public GameState state;
    }
}