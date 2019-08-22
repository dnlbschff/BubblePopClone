using BPC.Board;
using BPC.Config;
using Zenject;

namespace BPC.Installers
{
    public class BoardInstaller : Installer
    {
        [Inject]
        private GameConfig.BoardSettings _boardSetting;

        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<BoardViewModel>().AsSingle();
            Container.BindInterfacesAndSelfTo<BoardView>()
                .FromComponentInNewPrefab(_boardSetting.BoardViewPrefab)
                .AsSingle().NonLazy();
        }
    }
}