using UnityEngine;
using UniRx;
using FPS.Base;
using FPS.Attribute;
using Cysharp.Threading.Tasks;

namespace FPS.MVP
{
    public class ShopSlotPresenter : PresenterBase
    {
        private ShopSlotModel model;
        [SerializeField] private ShopSlotView view;

        public override void Inject(ModelBase modelBase)
        {
            base.Inject(modelBase);
            model = (ShopSlotModel)modelBase;
            view.UpdateUI(model).AutoForget(this);

            MessageBroker.Default.Receive<PurchaseEvent>().Subscribe(_ =>
            {
                model.IsPurchased = model.Name == _.name && model.ShopData.ShopCategory == ShopCategory.Weapon;

                view.UpdateUI(model).AutoForget(this);
            });

            view.ShopButtonManager.onPurchaseClick.AddListener(() =>
            {
                UniTask.Void(async () =>
                {
                    var presenter = await PopupManager.Instance.Get<InfomationPopupPresenter>(
                        new InfomationPopupPresenter.Args
                        {
                            Title = "Purchase",
                            Description = "You Buy?"
                        });

                    bool isConfirmed = await presenter.GetResultAsync();

                    if (isConfirmed)
                    {
                        var notification = await PopupManager.Instance.Get<NotificationPresenter>(new NotificationPresenter.Args
                        {
                            Title = "Purchase Success"
                        });
                        model.rxPurchase.OnNext(default);
                    }
                });
            });
        }
    }
}
