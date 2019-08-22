using BPC.Config;
using BPC.Utils;
using DB.Library.MVVM;
using DG.Tweening;
using UniRx;
using UnityEngine;
using Zenject;

namespace BPC.Input
{
    public class TargetBubbleView : ViewBase<TargetViewModel>
    {
        [SerializeField] private SpriteRenderer _spriteRenderer;

        [Inject]
        private GameConfig.BubbleSettings _bubbleSetting;

        [Inject]
        private GameConfig.InputSettings _inputSetting;

        protected override void BindSubscriptions(CompositeDisposable disposer)
        {
            ViewModel.Exponent
                .Subscribe(OnExponentChanged)
                .AddTo(Disposer);

            ViewModel.HasValidTarget
                .Subscribe(hasTarget => gameObject.SetActive(hasTarget)).AddTo(Disposer);

            ViewModel.TargetGridPosition
                .Subscribe(OnTargetGridPositionChanged)
                .AddTo(Disposer);
        }

        private void OnTargetGridPositionChanged(Vector2Int gridPos)
        {
            transform.DOKill();
            transform.localScale = Vector3.zero;
            transform.localPosition = HexGridUtils.HexGridToWorld(gridPos);

            transform.DOScale(1f, 0.2f);
        }

        private void OnExponentChanged(int exponent)
        {
            _spriteRenderer.color =
                _bubbleSetting.GetBubbleColor(exponent)
                    .WithAlpha(_inputSetting.TargetAlpha);

            _spriteRenderer.sprite =
                _bubbleSetting.GetBubbleSprite(exponent);
        }
    }
}