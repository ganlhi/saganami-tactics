using UnityEngine;

namespace ST.Play
{
    public class ShipMarker : MonoBehaviour
    {
        #region Editor customization
        #pragma warning disable 0649

        public bool ownedByClient;
        
        [SerializeField]
        private GameObject forLocalPlayer;

        [SerializeField]
        private GameObject forDistantPlayer;

        [SerializeField]
        private MeshRenderer forDistantPlayerMesh;
        
        #pragma warning restore 0649
        #endregion   

        #region Public variables
        #endregion

        #region Unity callbacks
        private void Start()
        {
            forLocalPlayer.SetActive(ownedByClient);
            forDistantPlayer.SetActive(!ownedByClient);
        }
        #endregion
    }
}
