using UnityEngine;

namespace ST.Scriptable
{
    [CreateAssetMenu(menuName = "ST/Faction")]
    public class Faction : ScriptableObject
    {
        public string Name;
        public Sprite Flag;
    }
}