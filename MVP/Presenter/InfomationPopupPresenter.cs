
using UnityEngine;
using UniRx;
using FPS.Base;
using FPS.Attribute;
using Cysharp.Threading.Tasks;

namespace FPS.MVP
{
    public class InfomationPopupPresenter : PopupPresenterBase
    {
        public new class Args : PopupPresenterBase.Args
        {
            public string Title;
            public string Description;
            public InfomationPopupIconType icon;
        };

        private new Args args;

        private InfomationPopupView view;



        public override void onCreate(object args)
        {
            this.args = (Args)args;
            base.onCreate(args);

            view.modalWindowManager.onClose.AddListener(() => Close(false));
            view.modalWindowManager.onConfirm.AddListener(() => Close(true));

            view.UpdateUI(this.args.Title, this.args.Description, this.args.icon);
        }

        public override void InjectView(ViewBase viewBase)
        {
            base.InjectView(viewBase);
            view = (InfomationPopupView)viewBase;
        }

        public override void onDestroy()
        {
            base.onDestroy();
        }
    }
}
