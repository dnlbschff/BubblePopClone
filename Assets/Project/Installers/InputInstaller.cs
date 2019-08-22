using BPC.Board;
using BPC.Bubbles;
using BPC.Config;
using BPC.Input;
using Zenject;

namespace BPC.Installers
{
    public class InputInstaller : Installer
    {
        [Inject]
        private GameConfig.InputSettings _inputSetting;

        [Inject]
        private GameConfig.BubbleSettings _bubbleSetting;

        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<InputViewModel>().AsSingle();
            Container.BindInterfacesAndSelfTo<InputView>()
                .FromComponentInNewPrefab(_inputSetting.InputViewPrefab)
                .UnderTransform(context => context.Container.Resolve<BoardView>().NextBubbleAnchor)
                .AsSingle().NonLazy();

            Container.BindInterfacesAndSelfTo<TargetViewModel>().AsSingle();
            Container.BindInterfacesAndSelfTo<TargetBubbleView>()
                .FromComponentInNewPrefab(_inputSetting.TargetViewPrefab)
                .UnderTransform(context => context.Container.Resolve<BoardView>().BubblesParent)
                .AsSingle()
                .OnInstantiated<TargetBubbleView>((context, targetView) => targetView.gameObject.SetActive(false))
                .NonLazy();

            Container.BindInterfacesAndSelfTo<NextBubblesViewModel>().AsSingle();
            Container.BindInterfacesAndSelfTo<NextBubblesView>()
                .FromComponentInNewPrefab(_bubbleSetting.NextBubblesViewPrefab)
                .UnderTransform(context => context.Container.Resolve<BoardView>().BubbleSpawnAnchor)
                .AsSingle().NonLazy();
        }
    }
}