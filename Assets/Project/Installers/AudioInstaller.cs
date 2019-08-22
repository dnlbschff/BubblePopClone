using BPC.Audio;
using UnityEngine;
using UnityEngine.Audio;
using Zenject;

namespace BPC.Installers
{
    public class AudioInstaller : Installer
    {
        public override void InstallBindings()
        {
            Container.BindFactory<AudioClip, AudioMixerGroup, float, float, AudioView, AudioView.Factory>()
                .FromPoolableMemoryPool<AudioClip, AudioMixerGroup, float, float, AudioView, AudioView.MemoryPool>(
                    poolBinder => poolBinder.WithInitialSize(5)
                        .FromSubContainerResolve()
                        .ByInstaller<AudioViewInstaller>());

            Container.BindInterfacesAndSelfTo<AudioService>().AsSingle();
        }
    }
}