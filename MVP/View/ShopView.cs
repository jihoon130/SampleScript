
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UniRx;
using Michsky.UI.Heat;

namespace FPS.MVP
{
    public class ShopView : ViewBase
    {
        public Transform Weapon;
        public Transform Upgrades;
        public ButtonManager ButtonManager;
        public TextMeshProUGUI Money;
    }
}
