using System.Linq;
using BPC.Board;
using BPC.Config;
using DB.Library.MVVM;
using Extensions;
using UniRx;
using UnityEngine;

namespace BPC.Bubbles
{
    public class NextBubblesViewModel : ViewModelBase
    {
        private readonly BubbleView.Factory _bubbleViewFactory;
        private readonly BoardView _boardView;
        private readonly BoardViewModel _boardViewModel;
        private readonly GameConfig.BoardSettings _boardSettings;
        private readonly SerialDisposable _serialDisposable;

        private readonly ReactiveCollection<BubbleViewModel> _nextBubbles;
        public IReadOnlyReactiveCollection<BubbleViewModel> NextBubbles => _nextBubbles;

        public NextBubblesViewModel(
            BubbleView.Factory bubbleViewFactory,
            BoardView boardView,
            BoardViewModel boardViewModel,
            GameConfig.BoardSettings boardSettings)
        {
            _bubbleViewFactory = bubbleViewFactory;
            _boardView = boardView;
            _boardViewModel = boardViewModel;
            _boardSettings = boardSettings;
            _nextBubbles = new ReactiveCollection<BubbleViewModel>().AddTo(Disposer);
            _serialDisposable = new SerialDisposable().AddTo(Disposer);

            FillNextBubbles();

            NextBubbles.ObserveAnyChange()
                .OncePerFrame()
                .Subscribe(_ => FillNextBubbles())
                .AddTo(Disposer);
        }

        private void FillNextBubbles()
        {
            while (NextBubbles.Count < 2)
            {
                var nextExponent = GetNextExponent();
                var view = _bubbleViewFactory.Create(nextExponent, Vector2Int.zero, false);
                var transform = view.transform;
                transform.position = _boardView.BubbleSpawnAnchor.position;
                transform.localScale = _boardView.BubbleSpawnAnchor.localScale;
                view.ViewModel.SetIsOnGrid(false);
                _nextBubbles.Add(view.ViewModel);
            }

            NextBubbles.Last().SetNextBubbleIndex(1);
            var nextBubble = NextBubbles.First();
            nextBubble.SetNextBubbleIndex(0);
            _serialDisposable.Disposable =
                nextBubble.NextBubbleIndex
                    .Where(index => index == -1)
                    .Take(1)
                    .Subscribe(_ => RemoveBubble());
        }

        private int GetNextExponent()
        {
            var initialExponent = _boardSettings.InitialMaxSpawnExponent - 3;

            var exponentsOnBoard =
                _boardViewModel.BubbleViewModels
                    .Select(kvp => kvp.Value.Exponent.Value)
                    .OrderBy(exponent => exponent)
                    .ToList();
            var possibleExponents = exponentsOnBoard
                .Where(exponent => exponent < Mathf.Max(exponentsOnBoard.Max() - 2, initialExponent)).ToList();
            return possibleExponents.Count > 0
                ? possibleExponents[Random.Range(0, possibleExponents.Count)]
                : Random.Range(1, initialExponent);
        }

        private void RemoveBubble()
        {
            _nextBubbles.RemoveAt(0);
        }

        public void SwitchBubbles()
        {
            _nextBubbles.Move(0, 1);
        }
    }
}