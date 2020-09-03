using System;
using UnityEngine;

namespace ST.Play.UI
{
    public class FullscreenPanels : MonoBehaviour
    {
#pragma warning disable 649
        [SerializeField] private CanvasGroup mainGroup;
        [SerializeField] private CanvasGroup reportsGroup;
        [SerializeField] private CanvasGroup engiGroup;
#pragma warning restore 649

        private void Start()
        {
            Close();
        }

        public void ShowReports()
        {
            //TODO add animation
            mainGroup.alpha = 1;
            mainGroup.interactable = true;
            mainGroup.blocksRaycasts = true;

            if (reportsGroup != null)
            {
                reportsGroup.alpha = 1;
                reportsGroup.interactable = true;
                reportsGroup.blocksRaycasts = true;
            }

            if (engiGroup != null)
            {
                engiGroup.alpha = 0;
                engiGroup.interactable = false;
                engiGroup.blocksRaycasts = false;
            }
        }

        public void ShowEngineeringPanel()
        {
            //TODO add animation
            mainGroup.alpha = 1;
            mainGroup.interactable = true;
            mainGroup.blocksRaycasts = true;

            if (reportsGroup != null)
            {
                reportsGroup.alpha = 0;
                reportsGroup.interactable = false;
                reportsGroup.blocksRaycasts = false;
            }

            if (engiGroup != null)
            {
                engiGroup.alpha = 1;
                engiGroup.interactable = true;
                engiGroup.blocksRaycasts = true;
            }
        }

        public void Close()
        {
            //TODO add animation
            mainGroup.alpha = 0;
            mainGroup.interactable = false;
            mainGroup.blocksRaycasts = false;
        }
    }
}