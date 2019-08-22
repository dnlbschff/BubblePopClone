using System.Collections.Generic;
using System.Linq;
using BPC.Audio;
using BPC.Bubbles;
using BPC.Config;
using BPC.Utils;
using DB.Library.MVVM;
using Extensions;
using UniRx;
using UnityEngine;

namespace BPC.Board
{
    public class BoardViewModel : ViewModelBase
    {
        private readonly BubbleView.Factory _bubbleViewFactory;
        private readonly AudioService _audioService;

        private readonly Vector2Int _minBounds;
        private readonly Vector2Int _maxBounds;
        private readonly int _maxSpawnExponent;
        private readonly int _minNumberOfBubbles;
        private readonly int _explosionExponent;

        private readonly ReactiveProperty<bool> _isInitialized;
        public IReadOnlyReactiveProperty<bool> IsInitialized => _isInitialized;

        private readonly ReactiveProperty<bool> _isMerging;
        public IReadOnlyReactiveProperty<bool> IsMerging => _isMerging;

        private readonly ReactiveProperty<int> _currentTopGridY;
        public IReadOnlyReactiveProperty<int> CurrentTopGridY => _currentTopGridY;

        private readonly ReactiveDictionary<Vector2Int, BubbleViewModel> _bubbleViewModels;
        public IReadOnlyReactiveDictionary<Vector2Int, BubbleViewModel> BubbleViewModels => _bubbleViewModels;

        private readonly CompositeDisposable _mergeSubscriptionDisposer;
        private readonly SerialDisposable _bubbleAddedSerialDisposable;

        public BoardViewModel(GameConfig.BoardSettings boardSetting, BubbleView.Factory bubbleViewFactory, AudioService audioService)
        {
            _minBounds = boardSetting.Min;
            _maxBounds = boardSetting.Max;
            _maxSpawnExponent = boardSetting.InitialMaxSpawnExponent + 2;
            _minNumberOfBubbles = boardSetting.MinNumberOfBubbles;
            _explosionExponent = boardSetting.ExplosionExponent;
            _bubbleViewFactory = bubbleViewFactory;
            _audioService = audioService;

            _isInitialized = new ReactiveProperty<bool>().AddTo(Disposer);
            _isMerging = new ReactiveProperty<bool>().AddTo(Disposer);
            _currentTopGridY = new ReactiveProperty<int>().AddTo(Disposer);
            _bubbleViewModels = new ReactiveDictionary<Vector2Int, BubbleViewModel>().AddTo(Disposer);
            _mergeSubscriptionDisposer = new CompositeDisposable().AddTo(Disposer);
            _bubbleAddedSerialDisposable = new SerialDisposable().AddTo(Disposer);

            IsInitialized
                .IfTrue()
                .Subscribe(_ => UpdateConnectedToCeiling()).AddTo(Disposer);

            BubbleViewModels
                .ObserveAnyChange()
                .OncePerFrame()
                .Where(_ => IsInitialized.Value)
                .DelayFrame(1)
                .Subscribe(_ => OnBubblesCountChanged()).AddTo(Disposer);

            CurrentTopGridY
                .SkipLatestValueOnSubscribe()
                .Where(_ => IsInitialized.Value)
                .OncePerFrame()
                .Subscribe(OnCurrentTopYChanged).AddTo(Disposer);

            IsMerging
                .SkipLatestValueOnSubscribe()
                .IfFalse()
                .OncePerFrame()
                .Subscribe(_ => TriggerExplosions())
                .AddTo(Disposer);
        }

        private void TriggerExplosions()
        {
            var explodingBubbles = BubbleViewModels
                .Select(kvp => kvp.Value)
                .Where(b => b.Exponent.Value >= _explosionExponent).ToList();

            explodingBubbles.ForEach(b =>
            {
                GetNeighbors(b.GridPosition.Value).ForEach(n =>
                {
                    n.SetMergeTarget(n.GridPosition.Value);
                    RemoveBubble(n);
                });
                b.SetMergeTarget(b.GridPosition.Value);
                RemoveBubble(b);
                _audioService.PlaySfx(SfxId.Explosion);
            });
        }

        private void OnCurrentTopYChanged(int topY)
        {
            var maxColumns = _maxBounds.x - _minBounds.x;
            for (int col = Mathf.Abs(topY % 2); col < maxColumns; col += 2)
            {
                var view = _bubbleViewFactory.Create(Random.Range(1, _maxSpawnExponent), new Vector2Int(col, topY),
                    true);
                view.ViewModel.SetIsOnGrid(true);
                view.ViewModel.SetIsConnectedToCeiling(true);
            }

            UpdateConnectedToCeiling();
        }

        private void OnBubblesCountChanged()
        {
            var count = BubbleViewModels.Count;
            if (count < _minNumberOfBubbles)
            {
                _currentTopGridY.Value--;
                return;
            }

            UpdateConnectedToCeiling();
        }

        public void AddBubble(BubbleViewModel bubbleViewModel, bool processMerges = false)
        {
            _bubbleViewModels.Add(bubbleViewModel.GridPosition.Value, bubbleViewModel);
            if (processMerges)
            {
                _bubbleAddedSerialDisposable.Disposable = bubbleViewModel.IsOnGrid
                    .SkipLatestValueOnSubscribe()
                    .IfTrue()
                    .Subscribe(_ => StartMerging(bubbleViewModel));
            }
        }

        private void RemoveBubble(BubbleViewModel bubbleViewModel)
        {
            _bubbleViewModels.Remove(bubbleViewModel.GridPosition.Value);
        }

        private void StartMerging(BubbleViewModel bubbleViewModel)
        {
            _mergeSubscriptionDisposer.Clear();

            _isMerging.Value = true;
            var mergeGridPos = bubbleViewModel.GridPosition.Value;
            var neighbors = GetNeighbors(mergeGridPos);
            neighbors.ForEach(n => n.SetAddedNeighbor(mergeGridPos));
            ProcessMerge(bubbleViewModel);
        }

        private void ProcessMerge(BubbleViewModel bubbleViewModel)
        {
            var matchingBubbles = GetConnectedMatchingBubbles(bubbleViewModel);

            if (matchingBubbles.Count <= 0)
            {
                _isMerging.Value = false;
                return;
            }

            var resultExponent = bubbleViewModel.Exponent.Value + matchingBubbles.Count;
            matchingBubbles.Add(bubbleViewModel);

            var mergeTarget = GetMergeTarget(matchingBubbles, resultExponent);

            matchingBubbles.Remove(mergeTarget);
            matchingBubbles.ForEach(bubble =>
            {
                bubble.SetMergeTarget(mergeTarget.GridPosition.Value);
                RemoveBubble(bubble);
            });
            mergeTarget.SetExponentAfterMerge(resultExponent);
            mergeTarget.Exponent
                .SkipLatestValueOnSubscribe()
                .Subscribe(_ => ProcessMerge(mergeTarget))
                .AddTo(_mergeSubscriptionDisposer);
        }

        private BubbleViewModel GetMergeTarget(
            List<BubbleViewModel> matchingBubbles,
            int resultExponent)
        {
            var orderedMatches = matchingBubbles
                .Select(bubble => new
                    {Bubble = bubble, SortingInfo = GetNeighborsSortingInfo(bubble, matchingBubbles, resultExponent)})
                .OrderBy(x => x.SortingInfo.MinExponentDiff)
                .ThenByDescending(x => x.SortingInfo.MinExponentDiffMatchCount)
                .Select(x => x.Bubble);

            var mergeTarget = orderedMatches.FirstOrDefault();
            return mergeTarget ?? matchingBubbles.Last();
        }

        private (int MinExponentDiff, int MinExponentDiffMatchCount) GetNeighborsSortingInfo(
            BubbleViewModel bubble,
            List<BubbleViewModel> bubblesToExclude, int resultExponent)
        {
            var neighborDiffs = GetNeighbors(bubble.GridPosition.Value)
                .Where(n => !bubblesToExclude.Contains(n))
                .Select(n => new {Bubble = n, Diff = n.Exponent.Value - resultExponent})
                .Where(tuple => tuple.Diff >= 0)
                .ToList();
            var min = neighborDiffs.Count > 0
                ? neighborDiffs.Min(tuple => tuple.Diff)
                : int.MaxValue;
            var count = neighborDiffs.Count(tuple => tuple.Diff == min);
            return (MinExponentDiff: min, MinExponentDiffMatchCount: count);
        }

        private List<BubbleViewModel> GetConnectedMatchingBubbles(BubbleViewModel bubbleViewModel)
        {
            var connectedBubbles = GetEqualExponentNeighbors(bubbleViewModel);

            for (var idx = 0; idx < connectedBubbles.Count; idx++)
            {
                var neighbor = connectedBubbles[idx];
                var unknownNeighbors =
                    GetEqualExponentNeighbors(neighbor)
                        .Where(n => !connectedBubbles.Contains(n) && n != bubbleViewModel).ToList();
                connectedBubbles.AddRange(unknownNeighbors);
            }

            return connectedBubbles;
        }

        private List<BubbleViewModel> GetEqualExponentNeighbors(BubbleViewModel bubbleViewModel)
        {
            var gridPos = bubbleViewModel.GridPosition.Value;
            var exponent = bubbleViewModel.Exponent.Value;

            var neighbors = GetNeighbors(gridPos)
                .Where(bubble => bubble.Exponent.Value == exponent).ToList();

            return neighbors;
        }

        private BubbleViewModel GetBubble(Vector2Int gridPos)
        {
            return _bubbleViewModels.ContainsKey(gridPos) ? _bubbleViewModels[gridPos] : null;
        }

        private List<BubbleViewModel> GetNeighbors(Vector2Int gridPos)
        {
            var neighbors = new List<BubbleViewModel>(6);
            foreach (var neighborPos in HexGridUtils.GetAllNeighborGridPositions(gridPos))
            {
                var bubble = GetBubble(neighborPos);
                if (bubble != null)
                {
                    neighbors.Add(bubble);
                }
            }

            return neighbors;
        }

        public bool IsWithinBounds(Vector2Int gridPos)
        {
            return gridPos.x >= _minBounds.x
                   && gridPos.x < _maxBounds.x
                   && gridPos.y >= _minBounds.y + _currentTopGridY.Value
                   && gridPos.y < _maxBounds.y + _currentTopGridY.Value;
        }

        public bool IsFree(Vector2Int gridPos)
        {
            return !BubbleViewModels.ContainsKey(gridPos);
        }

        private void UpdateConnectedToCeiling()
        {
            var bubblesToRemove = new List<BubbleViewModel>();
            foreach (var bubble in _bubbleViewModels)
            {
                UpdateConnectedToCeilingForBubble(bubble.Value);
                if (!bubble.Value.IsConnectedToCeiling.Value)
                {
                    bubblesToRemove.Add(bubble.Value);
                }
            }

            bubblesToRemove.ForEach(RemoveBubble);
        }

        private void UpdateConnectedToCeilingForBubble(BubbleViewModel bubbleViewModel)
        {
            var connectedBubbles = GetNeighbors(bubbleViewModel.GridPosition.Value);

            if (bubbleViewModel.GridPosition.Value.y == CurrentTopGridY.Value)
            {
                bubbleViewModel.SetIsConnectedToCeiling(true);
                return;
            }

            for (var idx = 0; idx < connectedBubbles.Count; idx++)
            {
                var neighbor = connectedBubbles[idx];
                var unknownNeighbors =
                    GetNeighbors(neighbor.GridPosition.Value)
                        .Where(n => !connectedBubbles.Contains(n) && n != bubbleViewModel).ToList();
                connectedBubbles.AddRange(unknownNeighbors);
            }

            var isConnectedToCeiling = connectedBubbles.Any(b => b.GridPosition.Value.y == CurrentTopGridY.Value);
            bubbleViewModel.SetIsConnectedToCeiling(isConnectedToCeiling);
        }

        public void SetIsInitialized(bool isInitialized)
        {
            _isInitialized.Value = isInitialized;
        }
    }
}