using BPC.Bubbles;
using BPC.Config;
using Zenject;

namespace BPC.Installers
{
    public class BubbleBurstInstaller : Installer
    {
        [Inject]
        private GameConfig.BubbleSettings _bubbleSetting;

        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<BubbleBurstViewModel>().AsSingle();
            Container.BindInterfacesAndSelfTo<BubbleBurstView>()
                .FromComponentInNewPrefab(_bubbleSetting.BubbleBurstViewPrefab)
                .UnderTransformGroup("BubbleBursts")
                .AsSingle();
        }
    }
}