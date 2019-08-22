using BPC.Initializers;
using UniRx;
using Zenject;

namespace BPC.Installers
{
    public class GameInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.BindInstance(new CompositeDisposable()).AsSingle();

            Container.Install<BoardInstaller>();
            Container.Install<BubbleInstaller>();
            Container.Install<InputInstaller>();
            Container.Install<AudioInstaller>();

            Container.BindInterfacesAndSelfTo<GameInitializer>().AsSingle().NonLazy();
        }
    }
}