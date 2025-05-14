
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UniRx;
using System.Text;
using Michsky.UI.Heat;
using Cysharp.Threading.Tasks;

namespace FPS.MVP
{
    public class InGameHUDView : MonoBehaviour
    {
        [SerializeField] public TextMeshProUGUI Money;
        [SerializeField] public TextMeshProUGUI Round;
        [SerializeField] public RawImage Weapon_Image;
        [SerializeField] public TextMeshProUGUI Weapon_Bullet;

        public StringBuilder stringBuilder = new StringBuilder();

        public void ChageBullet(int currentBullet, int maxBullet)
        {
            stringBuilder.Append(currentBullet);
            stringBuilder.Append(" / ");
            stringBuilder.Append(maxBullet);

            Weapon_Bullet.text = stringBuilder.ToString();
            stringBuilder.Clear();
        }

        public void ChangeMoney(int money)
        {
            stringBuilder.Append("Money : ");
            stringBuilder.Append(money);

            Money.text = stringBuilder.ToString();
            stringBuilder.Clear();
        }

        public void RoundStart(int round)
        {
            stringBuilder.Append("Round : ");
            stringBuilder.Append(round);

            Round.text = stringBuilder.ToString();
            stringBuilder.Clear();
        }

        public void ChangeWeaponImage(WeaponType weaponType)
        {
            UniTask.Void(async () =>
            {
                var icon = await AddressableManager.Instance.GetAsync<Texture2D>("Icon", weaponType.ToString());

                if (icon != null)
                {
                    Sprite sprite = Sprite.Create(
                            icon,
                            new Rect(0, 0, icon.width, icon.height),
                            new Vector2(0.5f, 0.5f));

                    Weapon_Image.texture = icon;
                }
            });

        }
    }
}
