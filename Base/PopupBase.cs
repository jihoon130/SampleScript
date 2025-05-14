using Cysharp.Threading.Tasks;
using FPS.InterFace;
using UnityEngine;
using UnityEngine.InputSystem.XInput;

namespace FPS.Base
{
    public class PopupPresenterBase : IPopupCreate, IPopupDestroy
    {
        protected virtual Args args { get; set; } = new Args();
        protected UniTaskCompletionSource<bool> completionSource = new UniTaskCompletionSource<bool>();
        private ViewBase view;
        private bool IsCompletion;

        public virtual void onCreate(object args)
        {
            ModelContainer.InjectDependencies(this);
        }

        public virtual void InjectView(ViewBase viewBase) 
        {
            view = viewBase;
        }

        public virtual void onDestroy()
        {
            completionSource?.TrySetResult(IsCompletion);
            ModelContainer.ReleaseModel(this);

            UnityEngine.GameObject.Destroy(view.gameObject);
        }
        public void Close(bool result)
        {
            IsCompletion = result;
            PopupManager.Instance.Return(this.GetType());
        }

        public UniTask<bool> GetResultAsync() => completionSource.Task;

        public class Args { };
    }
}