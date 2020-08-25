using System;
using System.Collections.Generic;
using System.Linq;
using ST.Scriptable;
using TMPro;
using UnityEngine;

namespace ST.Common.UI
{
    public class SsdWeaponMount : MonoBehaviour
    {
        private WeaponMount _weaponMount;
        public WeaponMount WeaponMount
        {
            set
            {
                _weaponMount = value;
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
                [SerializeField] private TextMeshProUGUI ammoText;
                [SerializeField] private TextMeshProUGUI nameText;
                [SerializeField] private SsdBoxes ssdBoxes;
        #pragma warning restore 649

        private void UpdateUi()
        {
            ammoText.gameObject.SetActive(_weaponMount.model.type == WeaponType.Missile);
            ammoText.text = _weaponMount.ammo.ToString();
            nameText.text = _weaponMount.model.name;

            ssdBoxes.CurrentValue = (int) SsdHelper.GetUndamagedValue(_weaponMount.weapons, _alterations.Count);
            
            var nbDestroyed = _alterations.Count(a => a.destroyed);
            var nbDamaged = _alterations.Count(a => !a.destroyed);
            
            ssdBoxes.CanRepair = nbDamaged > 0; 
            ssdBoxes.Damages = new Tuple<int, int>(nbDestroyed, nbDamaged);
        }
    }
}