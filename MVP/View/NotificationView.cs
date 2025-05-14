
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UniRx;
using Michsky.UI.Heat;

namespace FPS.MVP
{
    public class NotificationView : ViewBase
    {
        public NotificationManager notificationManager;

        public void UpdateUI(string title)
        {
            notificationManager.notificationText = title;
            notificationManager.onDestroy.AddListener(() => { PopupManager.Instance.Return(typeof(NotificationPresenter)); });
            notificationManager.UpdateUI();
        }
    }
}
