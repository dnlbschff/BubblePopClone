using BPC.Board;
using BPC.Config;
using UnityEngine;
using Zenject;

namespace BPC.Bubbles
{
    public class BubbleViewFactory : IFactory<int, Vector2Int, bool, BubbleView>
    {
        [Inject]
        private DiContainer _container;

        [Inject]
        private GameConfig.BubbleSettings _bubbleSetting;

        [Inject]
        private BoardViewModel _boardViewModel;

        public BubbleView Create(int initialExponent, Vector2Int gridPosition, bool addToBoard)
        {
            var sub = _container.CreateSubContainer();
            sub.BindInstance(gridPosition);
            sub.BindInstance(initialExponent);
            sub.BindInterfacesAndSelfTo<BubbleViewModel>().AsSingle();
            sub.BindInterfacesAndSelfTo<BubbleAnimationViewModel>().AsSingle();
            sub.BindInterfacesAndSelfTo<BubbleBurstViewModel>().AsSingle();
            if (addToBoard)
            {
                sub.BindInterfacesAndSelfTo<BubbleView>()
                    .FromComponentInNewPrefab(_bubbleSetting.BubbleViewPrefab)
                    .UnderTransform(context => context.Container.Resolve<BoardView>().BubblesParent)
                    .AsSingle()
                    .OnInstantiated<BubbleView>((context, view) => _boardViewModel.AddBubble(view.ViewModel));
            }
            else
            {
                sub.BindInterfacesAndSelfTo<BubbleView>()
                    .FromComponentInNewPrefab(_bubbleSetting.BubbleViewPrefab)
                    .UnderTransformGroup("Next Bubbles")
                    .AsSingle();
            }

            return sub.Resolve<BubbleView>();
        }
    }
}