using System;
using JetBrains.Annotations;
using UniRx;
using Zenject;

namespace DB.Library.MVVM
{
    public abstract class DisposableBase : IDisposable
    {
        protected readonly CompositeDisposable Disposer = new CompositeDisposable();

        [Inject, UsedImplicitly]
        private void InjectDisposer(CompositeDisposable disposer)
        {
            Disposer.AddTo(disposer);
        }
        
        public virtual void Dispose()
        {
            Disposer.Dispose();
        }
    }
}