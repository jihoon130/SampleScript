using UnityEngine;
using Michsky.UI.Heat;

namespace FPS.MVP
{
    public class HealthBarView : MonoBehaviour
    {
        [SerializeField] private ProgressBar healthBar;

        public void ChangeHpBar(int hp)
        {
            healthBar.currentValue = hp;
            healthBar.UpdateUI();
        }

        public void ChangeMaxHp(int maxHp)
        {
            healthBar.maxValue = maxHp;
            healthBar.UpdateUI();
        }
    }
}
