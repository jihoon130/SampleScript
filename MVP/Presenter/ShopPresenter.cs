using Cysharp.Threading.Tasks;
using UnityEngine;
using UniRx;
using FPS.Base;
using FPS.Attribute;
using System;

namespace FPS.MVP
{
    public class ShopPresenter : PopupPresenterBase
    {
        public new class Args : PopupPresenterBase.Args
        {
            public int Round;
        };

        private new Args args;

        [SingletonInject] private PlayerModel playerModel;
        private ShopView view;

        public override void onCreate(object args)
        {
            this.args = (Args)args;
            base.onCreate(args);

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            view.ButtonManager.onClick.AddListener(() => Close(false));
            playerModel.Money.Subscribe(_ => view.Money.text = "Money :" + _);

            UpdateUI().AutoForget(view);
        }

        private async UniTask UpdateUI()
        {
            foreach (var data in DataContainer.ShopData.Values)
            {
                var slot = await AddressableManager.Instance.InitGameObject("ShopSlot",
                    data.ShopCategory == ShopCategory.Weapon ? view.Weapon : view.Upgrades);

                var model = new ShopSlotModel();
                model.Name = data.Name;
                model.ShopData = data;
                model.IsPurchased = playerModel.MainWeapon.Value.ToString() == data.Name;
                model.rxPurchase.Subscribe(_ =>
                {
                    var isPay = playerModel.ConsumeMoney(data.Price);

                    if (isPay)
                    {
                        Purchase(data);
                    }
                    MessageBroker.Default.Publish(new PurchaseEvent { name = data.Name });
                });

                slot.GetComponent<ShopSlotPresenter>().Inject(model);
            }
        }

        public override void InjectView(ViewBase viewBase)
        {
            base.InjectView(viewBase);
            view = (ShopView)viewBase;
        }

        public override void onDestroy()
        {
            base.onDestroy();

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            PopupManager.Instance.Return(typeof(NotificationPresenter));
            MessageBroker.Default.Publish(new RoundStartEvent { Round = ++args.Round });
        }

        private void Purchase(ShopData shopData)
        {
            switch (shopData.ShopCategory)
            {
                case ShopCategory.Weapon:
                    playerModel.SetWeapon((WeaponType)Enum.Parse(typeof(WeaponType), shopData.Name));
                    break;

                default:
                    break;
            }
        }
    }
}
