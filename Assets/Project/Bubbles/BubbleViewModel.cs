using System;
using BPC.Config;
using DB.Library.MVVM;
using UniRx;
using UnityEngine;
using Zenject;

namespace BPC.Bubbles
{
    public class BubbleViewModel : ViewModelBase
    {
        [Inject]
        private GameConfig.BubbleSettings _bubbleSetting;

        private readonly ReactiveProperty<Vector2Int> _gridPosition;
        public IReadOnlyReactiveProperty<Vector2Int> GridPosition => _gridPosition;

        private readonly ReactiveProperty<int> _exponent;
        public IReadOnlyReactiveProperty<int> Exponent => _exponent;

        private readonly ReactiveProperty<bool> _isOnGrid;
        public IReadOnlyReactiveProperty<bool> IsOnGrid => _isOnGrid;

        private readonly ReactiveProperty<int> _nextBubbleIndex;
        public IReadOnlyReactiveProperty<int> NextBubbleIndex => _nextBubbleIndex;

        private readonly ReactiveProperty<Vector2Int> _mergeTarget;
        public IReadOnlyReactiveProperty<Vector2Int> MergeTarget => _mergeTarget;

        private readonly ReactiveProperty<bool> _isConnectedToCeiling;
        public IReadOnlyReactiveProperty<bool> IsConnectedToCeiling => _isConnectedToCeiling;

        private readonly ReactiveProperty<Vector2Int> _neighborAdded;
        public IReadOnlyReactiveProperty<Vector2Int> NeighborAdded => _neighborAdded;

        private readonly SerialDisposable _serialDisposable;

        [Inject]
        public BubbleViewModel(int initialExponent, Vector2Int gridPosition)
        {
            _gridPosition = new ReactiveProperty<Vector2Int>(gridPosition).AddTo(Disposer);
            _exponent = new ReactiveProperty<int>(initialExponent).AddTo(Disposer);
            _isOnGrid = new ReactiveProperty<bool>(true).AddTo(Disposer);
            _nextBubbleIndex = new ReactiveProperty<int>(-1).AddTo(Disposer);
            _mergeTarget = new ReactiveProperty<Vector2Int>(new Vector2Int(-1, -1)).AddTo(Disposer);
            _serialDisposable = new SerialDisposable().AddTo(Disposer);
            _isConnectedToCeiling = new ReactiveProperty<bool>().AddTo(Disposer);
            _neighborAdded = new ReactiveProperty<Vector2Int>().AddTo(Disposer);
        }


        public void SetNextBubbleIndex(int nextBubbleIndex)
        {
            _nextBubbleIndex.Value = nextBubbleIndex;
        }

        public void SetIsOnGrid(bool isOnGrid)
        {
            _isOnGrid.Value = isOnGrid;
        }

        public void SetGridPosition(Vector2Int gridPos)
        {
            _gridPosition.Value = gridPos;
        }

        public void SetMergeTarget(Vector2Int mergeTarget)
        {
            _isOnGrid.Value = false;
            _mergeTarget.SetValueAndForceNotify(mergeTarget);
        }

        public void SetExponentAfterMerge(int exponent)
        {
            _serialDisposable.Disposable =
                Observable.Timer(TimeSpan.FromSeconds(_bubbleSetting.MergeTime))
                    .Subscribe(_ => _exponent.Value = exponent);
        }

        public void SetIsConnectedToCeiling(bool isConnectedToCeiling)
        {
            _isConnectedToCeiling.Value = isConnectedToCeiling;
        }

        public void SetAddedNeighbor(Vector2Int neighborGridPos)
        {
            _neighborAdded.SetValueAndForceNotify(neighborGridPos);
        }
    }
}