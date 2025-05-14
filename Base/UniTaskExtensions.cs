using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;

public static class UniTaskExtensions
{
    public static void AutoForget(this UniTask task, MonoBehaviour mono)
    {
        task.AttachExternalCancellation(mono.GetCancellationTokenOnDestroy()).Forget();
    }
}
