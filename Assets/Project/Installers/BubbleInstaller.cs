using BPC.Bubbles;
using UnityEngine;
using Zenject;

namespace BPC.Installers
{
    public class BubbleInstaller : Installer
    {
        public override void InstallBindings()
        {
            Container.BindFactory<int, Vector2Int, bool, BubbleView, BubbleView.Factory>()
                .FromFactory<BubbleViewFactory>();

            Container.BindFactory<int, Vector3, BubbleBurstView, BubbleBurstView.Factory>()
                .FromPoolableMemoryPool<int, Vector3, BubbleBurstView, BubbleBurstView.MemoryPool>(
                    poolBinder =>
                        poolBinder.WithInitialSize(5)
                            .FromSubContainerResolve()
                            .ByInstaller<BubbleBurstInstaller>()
                );
        }
    }
}