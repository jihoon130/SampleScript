
using UnityEngine;
using UniRx;
using FPS.Base;
using FPS.Attribute;

namespace FPS.MVP
{
    public class InGameHUDPresenter : MonoBehaviour
    {
        [SingletonInject] private PlayerModel model;
        [SerializeField] private InGameHUDView view;

        private void Awake()
        {
            ModelContainer.InjectDependencies(this);

            model.CurrentProjectileStream
                .Switch()
                .Subscribe(value =>
                {
                    view.ChageBullet(value, model.HaveProjectileStream.Value.Value);
                }).AddTo(this);

            model.HaveProjectileStream
                .Switch()
                .Subscribe(value =>
                {
                    view.ChageBullet(model.CurrentProjectileStream.Value.Value, value);
                }).AddTo(this);

            model.Money.Subscribe(_ =>
            {
                view.ChangeMoney(_);
            });

            model.EquipType.Subscribe(_ =>
            {
                view.ChangeWeaponImage(_ == EquipType.MainWeapon ? model.MainWeapon.Value : model.SecondaryWeapon.Value);
            });

            MessageBroker.Default.Receive<RoundStartEvent>().Subscribe(_ =>
            {
                view.RoundStart(_.Round);
            });
        }
    }
}
