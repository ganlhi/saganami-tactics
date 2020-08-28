using Michsky.UI.Shift;
using UnityEngine;

namespace ST.Main_Menu
{
    public class SplashScreenManager : MonoBehaviour
    {
        [Header("RESOURCES")] public GameObject splashScreen;
        public GameObject mainPanels;

        private Animator _splashScreenAnimator;
        private Animator _mainPanelsAnimator;
        private ConnectToServerEvent _ssConnectToServerEvent;

        [Header("SETTINGS")] public bool disableSplashScreen;

        public bool enablePressAnyKeyScreen;

        private MainPanelManager _mpm;

        private void Start()
        {
            _splashScreenAnimator = splashScreen.GetComponent<Animator>();
            _ssConnectToServerEvent = splashScreen.GetComponent<ConnectToServerEvent>();
            _mainPanelsAnimator = mainPanels.GetComponent<Animator>();
            _mpm = gameObject.GetComponent<MainPanelManager>();

            if (disableSplashScreen)
            {
                splashScreen.SetActive(false);
                mainPanels.SetActive(true);

                _mainPanelsAnimator.Play("Start");
                _mpm.OpenFirstTab();
            }
            else if (enablePressAnyKeyScreen)
            {
                splashScreen.SetActive(true);
                _mainPanelsAnimator.Play("Invisible");
            }
            else
            {
                splashScreen.SetActive(true);
                _mainPanelsAnimator.Play("Invisible");
                _splashScreenAnimator.Play("Loading");
                _ssConnectToServerEvent.StartIEnumerator();
            }
        }
    }
}