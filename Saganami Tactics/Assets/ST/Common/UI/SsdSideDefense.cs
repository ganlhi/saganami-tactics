using System;
using System.Collections.Generic;
using System.Linq;
using ST.Scriptable;
using TMPro;
using UnityEngine;

namespace ST.Common.UI
{
    public class SsdSideDefense : MonoBehaviour
    {
        public string Name
        {
            set => nameText.text = value;
        }

        private uint[] _boxes;

        public uint[] Boxes
        {
            set
            {
                _boxes = value;
                UpdateUi();
            }
        }

        private List<SsdAlteration> _alterations = new List<SsdAlteration>();

        public List<SsdAlteration> Alterations
        {
            set
            {
                _alterations = value;
                UpdateUi();
            }
        }

#pragma warning disable 649
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private SsdBoxes ssdBoxes;
#pragma warning restore 649

        private void UpdateUi()
        {
            ssdBoxes.gameObject.SetActive(_boxes.Any());
            ssdBoxes.CurrentValue = (int) SsdHelper.GetUndamagedValue(_boxes, _alterations.Count);

            var nbDestroyed = _alterations.Count(a => a.destroyed);
            var nbDamaged = _alterations.Count(a => !a.destroyed);

            ssdBoxes.CanRepair = _boxes.Length > 0 && nbDamaged > 0;
            ssdBoxes.Damages = new Tuple<int, int>(nbDestroyed, nbDamaged);
        }
    }
}