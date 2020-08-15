using System.Collections.Generic;
using UnityEngine;
using VolumetricLines;

namespace ST.Play
{
    public class BeamsLines : MonoBehaviour
    {
#pragma warning disable 649
        [SerializeField] private VolumetricLineBehavior prefab;
#pragma warning restore 649

        private readonly List<VolumetricLineBehavior> _lines = new List<VolumetricLineBehavior>();

        public void AddLine(Vector3 from, Vector3 to)
        {
            var line = Instantiate(prefab, transform).GetComponent<VolumetricLineBehavior>();

            line.StartPos = from;
            line.EndPos = to;
            _lines.Add(line);
        }

        public void RemoveLines()
        {
            _lines.ForEach(l => Destroy(l.gameObject));
            _lines.Clear();
        }
    }
}