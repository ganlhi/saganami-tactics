using ST.Scriptable;
using TMPro;
using UnityEngine;

namespace ST.Common.UI
{
    public class SsdWeapon : MonoBehaviour
    {
        public Weapon Weapon
        {
            set => UpdateUi(value);
        }

#pragma warning disable 649
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI typeText;
        [SerializeField] private TextMeshProUGUI evasionText;
        [SerializeField] private TextMeshProUGUI spanText;
        [SerializeField] private Transform rangeBandsContent;
        [SerializeField] private SsdRangeBand rangeBandPrefab;
#pragma warning restore 649


        private void UpdateUi(Weapon weapon)
        {
            nameText.text = weapon.name;
            typeText.text = weapon.type == WeaponType.Missile ? "M" : "L";
            evasionText.text = weapon.evasion != 0 ? $"Evasion: {weapon.evasion}+" : "";
            spanText.text = $"Span: {weapon.span}";

            foreach (Transform child in rangeBandsContent)
            {
                Destroy(child.gameObject);
            }
            
            foreach (var rangeBand in weapon.bands)
            {
                var rb = Instantiate(rangeBandPrefab, rangeBandsContent).GetComponent<SsdRangeBand>();
                rb.RangeBand = rangeBand;
            }
        }
    }
}