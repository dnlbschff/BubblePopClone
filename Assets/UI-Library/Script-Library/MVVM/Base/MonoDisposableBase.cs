using System;
using JetBrains.Annotations;
using UniRx;
using UnityEngine;
using Zenject;

namespace DB.Library.MVVM
{
    public class MonoDisposableBase : MonoBehaviour, IDisposable
    {
        protected readonly CompositeDisposable Disposer = new CompositeDisposable();

        [Inject, UsedImplicitly]
        private void SetupDisposer(CompositeDisposable disposer)
        {
            Disposer.AddTo(disposer);
        }

        public virtual void Dispose()
        {
            Disposer.Dispose();
        }

        private void OnDestroy()
        {
            Dispose();
        }
    }
}