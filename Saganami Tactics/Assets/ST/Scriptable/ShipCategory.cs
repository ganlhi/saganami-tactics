using UnityEngine;

namespace ST.Scriptable
{
    [CreateAssetMenu(menuName = "ST/Ship Category")]
    public class ShipCategory : ScriptableObject
    {
        public string Name;
        public string Code;
        public int DisplayOrder;
    }
}