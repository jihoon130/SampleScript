
using UnityEngine;
using UniRx;
using FPS.Base;
using FPS.Attribute;

namespace FPS.MVP
{
    public class NotificationPresenter : PopupPresenterBase
    {
        public new class Args : PopupPresenterBase.Args
        {
            public string Title { get; set; }
        };

        private new Args args;

        private NotificationView view;



        public override void onCreate(object args)
        {
            this.args = (Args)args;
            base.onCreate(args);

            view.UpdateUI(this.args.Title);
        }

        public override void InjectView(ViewBase viewBase)
        {
            base.InjectView(viewBase);
            view = (NotificationView)viewBase;
        }

        public override void onDestroy()
        {
            base.onDestroy();
        }
    }
}
