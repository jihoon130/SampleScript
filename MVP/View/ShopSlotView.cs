
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UniRx;
using Michsky.UI.Heat;
using Cysharp.Threading.Tasks;

namespace FPS.MVP
{
    public class ShopSlotView : ViewBase
    {
        [SerializeField] public ShopButtonManager ShopButtonManager;

        public async UniTask UpdateUI(ShopSlotModel model)
        {
            var icon = await AddressableManager.Instance.GetAsync<Texture2D>("Icon", model.Name);

            if (icon != null)
            {
                Sprite sprite = Sprite.Create(
                        icon,
                        new Rect(0, 0, icon.width, icon.height),
                        new Vector2(0.5f, 0.5f));

                ShopButtonManager.buttonIcon = sprite;
            }
            ShopButtonManager.buttonTitle = model.Name;
            ShopButtonManager.priceText = model.ShopData.Price.ToString();
            ShopButtonManager.SetState(model.IsPurchased ? ShopButtonManager.State.Purchased : ShopButtonManager.State.Default);
            ShopButtonManager.UpdateUI();
        }
    }
}
