
using UnityEngine;
using UniRx;
using FPS.Base;
using FPS.Attribute;

namespace FPS.MVP
{
    public class HealthBarPresenter : MonoBehaviour
    {

        [SingletonInject] private PlayerModel playerModel;
        [SerializeField] private HealthBarView view;

        private void Awake()
        {
            ModelContainer.InjectDependencies(this);

            playerModel.Hp.Subscribe(_ =>
            {
                view.ChangeHpBar(_);
            }).AddTo(this);

            playerModel.MaxHp.Subscribe(_ =>
            {
                view.ChangeMaxHp(_);
            }).AddTo(this);
        }
    }
}
