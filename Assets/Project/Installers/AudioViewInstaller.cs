using BPC.Audio;
using BPC.Config;
using Zenject;

namespace BPC.Installers
{
    public class AudioViewInstaller : Installer
    {
        [Inject]
        private GameConfig.SfxSettings _sfxSetting;

        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<AudioViewModel>().AsSingle();
            Container.BindInterfacesAndSelfTo<AudioView>()
                .FromComponentInNewPrefab(_sfxSetting.AudioViewPrefab)
                .UnderTransformGroup("Audio")
                .AsSingle();
        }
    }
}