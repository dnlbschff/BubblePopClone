using JetBrains.Annotations;
using Zenject;

namespace MVVM.Base
{
    public abstract class FactoryBase
    {
        protected DiContainer ParentContainer;
        protected DiContainer Container;

        [Inject, UsedImplicitly]
        private void InjectContainer(DiContainer container)
        {
            ParentContainer = container;
            Container = ParentContainer.CreateSubContainer();
        }

        [Inject, UsedImplicitly]
        protected abstract void Initialize();
    }
}