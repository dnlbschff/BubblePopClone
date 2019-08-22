using System.Collections.Generic;
using BPC.Board;
using BPC.Input;
using BPC.Utils;
using DB.Library.MVVM;
using Extensions;
using UniRx;
using UnityEngine;

namespace BPC.Bubbles
{
    public class BubbleAnimationViewModel : ViewModelBase
    {
        private readonly InputViewModel _inputViewModel;
        private readonly BubbleViewModel _bubbleViewModel;
        private readonly BoardViewModel _boardViewModel;

        private readonly ReactiveProperty<bool> _isShooting;
        public IReadOnlyReactiveProperty<bool> IsShooting => _isShooting;

        private readonly List<Vector3> _trajectoryPositions = new List<Vector3>(2);
        public IReadOnlyList<Vector3> TrajectoryPositions => _trajectoryPositions;

        public IReadOnlyReactiveProperty<Vector2Int> MergeTarget => _bubbleViewModel.MergeTarget;

        public IReadOnlyReactiveProperty<int> NextBubbleIndex => _bubbleViewModel.NextBubbleIndex;

        public readonly IReadOnlyReactiveProperty<bool> IsDropping;

        public IReadOnlyReactiveProperty<int> Exponent => _bubbleViewModel.Exponent;

        public IReadOnlyReactiveProperty<Vector2Int> NeighborAdded => _bubbleViewModel.NeighborAdded;

        public BubbleAnimationViewModel(
            InputViewModel inputViewModel,
            BubbleViewModel bubbleViewModel,
            BoardViewModel boardViewModel)
        {
            _inputViewModel = inputViewModel;
            _bubbleViewModel = bubbleViewModel;
            _boardViewModel = boardViewModel;

            _isShooting = new ReactiveProperty<bool>().AddTo(Disposer);
            IsDropping = _bubbleViewModel.IsOnGrid
                .CombineLatest(_bubbleViewModel.IsConnectedToCeiling,
                    (isOnGrid, isConnected) => isOnGrid & !isConnected)
                .OncePerFrame()
                .ToReadOnlyReactiveProperty();

            _inputViewModel.ShootObservable
                .Where(_ => NextBubbleIndex.Value == 0)
                .Where(_ => _inputViewModel.HasValidTarget.Value)
                .Where(_ => _boardViewModel.IsFree(_inputViewModel.TargetGridPos.Value))
                .Subscribe(_ => UpdateTrajectoryAndShoot())
                .AddTo(Disposer);
        }

        private void UpdateTrajectoryAndShoot()
        {
            _trajectoryPositions.Clear();
            if (_inputViewModel.TrajectoryPoints.Value.Count > 2)
            {
                _trajectoryPositions.Add(_inputViewModel.TrajectoryPoints.Value[1]);
            }

            var gridPos = _inputViewModel.TargetGridPos.Value;
            _bubbleViewModel.SetGridPosition(gridPos);
            _boardViewModel.AddBubble(_bubbleViewModel, true);
            _trajectoryPositions.Add(HexGridUtils.HexGridToWorld(gridPos));
            _isShooting.Value = true;
        }

        public void SetAnimationComplete()
        {
            _bubbleViewModel.SetIsOnGrid(true);
            _bubbleViewModel.SetNextBubbleIndex(-1);
            _isShooting.Value = false;
        }
    }
}