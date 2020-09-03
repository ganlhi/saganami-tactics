using System;
using ST.Common.UI;
using ST.Play;
using ST.Play.UI;
using UnityEngine;
using UnityEngine.UI;

namespace ST.Deploy
{
    [RequireComponent(typeof(DeployManager))]
    public class DeployUiManager : MonoBehaviour
    {
        private DeployManager _deployManager;

        [Header("Main")]
#pragma warning disable 649
        [SerializeField]
        private CanvasGroup loadingGroup;

        [SerializeField] private FullscreenPanels fullscreenPanels;
#pragma warning restore 649

        #region Ready

        [Header("Ready")]
#pragma warning disable 649
        [SerializeField]
        private GameObject readyPanelContent;

        [SerializeField] private GameObject readyPanelLoading;
        [SerializeField] private Button readyPanelButton;
#pragma warning restore 649

        private void UpdateReadyPanelOnChanges()
        {
            readyPanelButton.onClick.AddListener(() =>
            {
                _deployManager.SetReadyToPlay();
                readyPanelContent.SetActive(false);
                readyPanelLoading.SetActive(true);
            });
        }

        #endregion Ready

        #region Ships

        [Header("Ships")]
#pragma warning disable 649
        [SerializeField]
        private ShipsList shipsList;
#pragma warning restore 649

        private void UpdateShipsListOnChanges()
        {
            _deployManager.OnShipsInit += (sender, args) => { shipsList.Ships = DeployManager.GetPlayerShips(); };
            _deployManager.OnSelectShip += (sender, selection) => { shipsList.SetSelectedShip(selection.Item1); };

            shipsList.OnSelectShip += (sender, shipView) => { _deployManager.SelectedShip = shipView; };
            shipsList.OnFocusCameraOnShip += (sender, shipView) => { _deployManager.LockCameraToShip(shipView); };
        }

        #endregion Ships

        #region Ship info

        [Header("Ship info")]
#pragma warning disable 649
        [SerializeField]
        private ShipInfo shipInfo;
#pragma warning restore 649

        private void UpdateShipInfoOnChanges()
        {
            _deployManager.OnSelectShip += (sender, selection) =>
            {
                var (shipView, _) = selection;
                shipInfo.ship = shipView.ship;
                shipInfo.ssd = shipView.ship.Ssd;
            };
        }

        #endregion Ship info

        #region Engineering

        [Header("Engineering")]
#pragma warning disable 649
        [SerializeField]
        private SsdPanel ssdPanel;
#pragma warning restore 649

        private void UpdateEngineeringPanelOnChanges()
        {
            if (_deployManager.SelectedShip != null)
            {
                SetEngineeringPanel();
            }

            _deployManager.OnSelectShip += (sender, selection) => SetEngineeringPanel();
        }

        private void SetEngineeringPanel()
        {
            if (_deployManager.SelectedShip == null) return;
            var ship = _deployManager.SelectedShip.ship;
            ssdPanel.ShipName = ship.name;
            ssdPanel.Ssd = ship.Ssd;
        }

        #endregion Engineering

        private void Awake()
        {
            loadingGroup.alpha = 1f;
            loadingGroup.blocksRaycasts = true;

            _deployManager = GetComponent<DeployManager>();

            _deployManager.OnShipsInit += (sender, args) =>
            {
                loadingGroup.alpha = 0f;
                loadingGroup.blocksRaycasts = false;
            };
        }

        private void Start()
        {
            UpdateReadyPanelOnChanges();
            UpdateShipsListOnChanges();
            UpdateShipInfoOnChanges();
            UpdateEngineeringPanelOnChanges();
        }

        private void Update()
        {
//            UpdatePlottingPanelEachFrame();
        }
    }
}