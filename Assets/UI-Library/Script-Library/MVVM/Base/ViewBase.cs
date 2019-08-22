using System;
using UniRx;
using Zenject;

namespace DB.Library.MVVM
{
    public abstract class View : MonoDisposableBase, IView
    {
        public abstract void TrySetViewModel(IViewModel vm);
        public abstract void InitializeView();
    }

    public abstract class ViewBase<TVm> : View
        where TVm : ViewModelBase
    {
        private readonly MultipleAssignmentDisposable _bindingDisposable
            = new MultipleAssignmentDisposable();

        //----------------------------------------------------------------------
        [Inject] private TVm _viewModel;

        public virtual TVm ViewModel
        {
            get { return _viewModel; }
            set
            {
                DisposeBindings();
                _viewModel = value;
                if (_viewModel == null)
                {
                    return;
                }

                BindSubscriptions(GetBindingDisposer());
            }
        }

        //----------------------------------------------------------------------
        public override void TrySetViewModel(IViewModel vm)
        {
            var castVm = vm as TVm;
            if (castVm == null)
            {
                throw new ArgumentException(
                    $"A view model of type {vm.GetType().Name} could not be cast to {typeof(TVm)}!");
            }

            ViewModel = castVm;
        }

        [Inject]
        public override void InitializeView()
        {
            _bindingDisposable.AddTo(Disposer);
            SetUp();
        }

        //----------------------------------------------------------------------
        protected virtual void SetUp()
        {
            BindSubscriptions(GetBindingDisposer());
        }

        //----------------------------------------------------------------------
        protected abstract void BindSubscriptions(CompositeDisposable disposer);

        //----------------------------------------------------------------------
        private void DisposeBindings()
        {
            _bindingDisposable.Disposable?.Dispose();
        }

        //----------------------------------------------------------------------
        private CompositeDisposable GetBindingDisposer()
        {
            _bindingDisposable.Disposable = new CompositeDisposable();
            return (CompositeDisposable) _bindingDisposable.Disposable;
        }

        //----------------------------------------------------------------------
        protected virtual void OnDestroy()
        {
            Disposer?.Dispose();
        }

        //----------------------------------------------------------------------
    }
}