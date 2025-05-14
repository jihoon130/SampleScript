
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UniRx;
using Michsky.UI.Heat;

namespace FPS.MVP
{
    public class InfomationPopupView : ViewBase
    {
        public ModalWindowManager modalWindowManager;

        public void UpdateUI(string title, string desc, InfomationPopupIconType icon)
        {
            modalWindowManager.titleText = title;
            modalWindowManager.descriptionText = desc;
            modalWindowManager.UpdateUI();
        }
    }
}
