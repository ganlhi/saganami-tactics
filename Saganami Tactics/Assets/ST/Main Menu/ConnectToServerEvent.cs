using System.Collections;
using Photon.Pun;
using UnityEngine;
using UnityEngine.Events;

namespace ST.Main_Menu
{
    public class ConnectToServerEvent : MonoBehaviour
    {
        public UnityEvent onConnectAction;

        private IEnumerator ConnectToServerEventStart()
        {
            PhotonNetwork.ConnectUsingSettings();

            do
            {
                yield return new WaitForSeconds(1);    
            } while (!PhotonNetwork.IsConnectedAndReady);
            
            onConnectAction.Invoke();
        }

        public void StartIEnumerator ()
        {
            StartCoroutine(nameof(ConnectToServerEventStart));
        }

        public void StopIEnumerator ()
        {
            StopCoroutine(nameof(ConnectToServerEventStart));
        }
    }
}
