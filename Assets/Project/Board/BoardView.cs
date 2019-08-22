using BPC.Config;
using BPC.Utils;
using DB.Library.MVVM;
using DG.Tweening;
using UniRx;
using UnityEngine;
using Zenject;

namespace BPC.Board
{
    public class BoardView : ViewBase<BoardViewModel>
    {
        [Inject]
        private GameConfig.BoardSettings _boardSetting;

        [SerializeField] private Transform _bubblesParent;
        public Transform BubblesParent => _bubblesParent;

        [SerializeField] private Transform _bubbleSpawnAnchor;
        public Transform BubbleSpawnAnchor => _bubbleSpawnAnchor;

        [SerializeField] private Transform _nextBubbleAnchor;
        public Transform NextBubbleAnchor => _nextBubbleAnchor;

        protected override void BindSubscriptions(CompositeDisposable disposer)
        {
            ViewModel.CurrentTopGridY
                .Subscribe(OnCurrentTopGridYChanged).AddTo(Disposer);
        }

        private void OnCurrentTopGridYChanged(int topY)
        {
            var displacement = HexGridUtils.HexGridToWorld(Vector2Int.down * topY);
            _bubblesParent.DOLocalMoveY(displacement.y, 0.4f);
        }
    }
}