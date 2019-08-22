using System.Linq;
using BPC.Bubbles;
using DB.Library.MVVM;
using Extensions;
using UniRx;
using UnityEngine;

namespace BPC.Input
{
    public class TargetViewModel : ViewModelBase
    {
        private readonly InputViewModel _inputViewModel;
        private readonly NextBubblesViewModel _nextBubblesViewModel;

        public IReadOnlyReactiveProperty<Vector2Int> TargetGridPosition => _inputViewModel.TargetGridPos;

        public IReadOnlyReactiveProperty<bool> HasValidTarget => _inputViewModel.HasValidTarget;

        private readonly ReactiveProperty<int> _exponent;
        public IReadOnlyReactiveProperty<int> Exponent => _exponent;

        public TargetViewModel(InputViewModel inputViewModel, NextBubblesViewModel nextBubblesViewModel)
        {
            _inputViewModel = inputViewModel;
            _nextBubblesViewModel = nextBubblesViewModel;

            _exponent = new ReactiveProperty<int>(1).AddTo(Disposer);

            _nextBubblesViewModel.NextBubbles
                .ObserveAnyChange()
                .OncePerFrame()
                .Subscribe(_ => OnNextBubbleChanged())
                .AddTo(Disposer);
            
            OnNextBubbleChanged();
        }

        private void OnNextBubbleChanged()
        {
            _exponent.Value = _nextBubblesViewModel.NextBubbles.First().Exponent.Value;
        }
    }
}