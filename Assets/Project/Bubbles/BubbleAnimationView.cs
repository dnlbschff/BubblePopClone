using System.Collections.Generic;
using BPC.Audio;
using BPC.Board;
using BPC.Config;
using BPC.Utils;
using DB.Library.MVVM;
using DG.Tweening;
using Extensions;
using TMPro;
using UniRx;
using UnityEngine;
using Zenject;

namespace BPC.Bubbles
{
    public class BubbleAnimationView : ViewBase<BubbleAnimationViewModel>
    {
        [Inject]
        private BoardView _boardView;

        [Inject]
        private GameConfig.BubbleSettings _bubbleSetting;

        [Inject]
        private BubbleBurstView.Factory _bubbleBurstViewFactory;

        [Inject]
        private AudioService _audioService;

        [SerializeField] private List<SpriteRenderer> _spriteRenderers;
        [SerializeField] private Canvas _canvas;
        [SerializeField] private RectTransform _valueChangedRt;
        [SerializeField] private TextMeshProUGUI _valueChangedText;
        [SerializeField] private Transform _bubbleParent;
        [SerializeField] private TrailRenderer _trailRenderer;

        private int _backLayerId;
        private int _mainLayerId;
        private int _frontLayerId;
        private SerialDisposable _serialDisposable;

        protected override void SetUp()
        {
            _backLayerId = SortingLayer.NameToID("Bubbles Back");
            _mainLayerId = SortingLayer.NameToID("Bubbles Main");
            _frontLayerId = SortingLayer.NameToID("Bubbles Front");
            _serialDisposable = new SerialDisposable().AddTo(Disposer);
            base.SetUp();
        }

        protected override void BindSubscriptions(CompositeDisposable disposer)
        {
            ViewModel.IsShooting
                .SkipLatestValueOnSubscribe()
                .IfTrue()
                .Subscribe(_ => AnimateShot())
                .AddTo(Disposer);

            ViewModel.NextBubbleIndex
                .SkipLatestValueOnSubscribe()
                .Subscribe(AnimateToNextBubbleAnchor)
                .AddTo(Disposer);

            ViewModel.MergeTarget
                .SkipLatestValueOnSubscribe()
                .Subscribe(AnimateMergeToTarget)
                .AddTo(Disposer);

            ViewModel.IsDropping
                .SkipLatestValueOnSubscribe()
                .IfTrue()
                .Subscribe(_ => AnimateDrop())
                .AddTo(Disposer);

            ViewModel.Exponent
                .SkipLatestValueOnSubscribe()
                .Subscribe(_ => AnimateExponentChange())
                .AddTo(Disposer);

            ViewModel.NeighborAdded
                .SkipLatestValueOnSubscribe()
                .Subscribe(AnimateNeighborAdded)
                .AddTo(Disposer);
        }

        private void AnimateNeighborAdded(Vector2Int neighborGridPos)
        {
            var neighborWorldPos = HexGridUtils.HexGridToWorld(neighborGridPos);
            var directionToNeighbor = neighborWorldPos - transform.localPosition;

            DOTween.Sequence()
                .Append(_bubbleParent.DOLocalMove(-directionToNeighbor.normalized * 0.1f, 0.1f).SetEase(Ease.OutQuint))
                .Append(_bubbleParent.DOLocalMove(Vector3.zero, 0.2f).SetEase(Ease.OutSine));
        }

        private void AnimateToNextBubbleAnchor(int nextBubbleIndex)
        {
            if (nextBubbleIndex < 0) return;

            var targetAnchor = nextBubbleIndex == 0
                ? _boardView.NextBubbleAnchor
                : _boardView.BubbleSpawnAnchor;
            var targetPosition = targetAnchor.position;
            var targetScale = targetAnchor.localScale;

            if (nextBubbleIndex == 1)
            {
                SortToBack();
            }

            transform.DOKill(true);
            transform.DOMove(targetPosition, 0.2f);
            transform.DOScale(targetScale, 0.2f);

            ResetSorting();
        }

        private void AnimateShot()
        {
            _audioService.PlaySfx(SfxId.Shot);
            transform.DOKill(true);
            transform.SetParent(_boardView.BubblesParent, true);
            _trailRenderer.enabled = true;
            SortToFront();

            var sequence = DOTween.Sequence();

            if (ViewModel.TrajectoryPositions.Count > 1)
            {
                var fromPos = transform.localPosition;
                var targetPos = transform.parent.InverseTransformPoint(ViewModel.TrajectoryPositions[0]);
                var animTime = GetAnimationTime(fromPos, targetPos);
                sequence.Append(transform.DOLocalMove(targetPos, animTime).SetEase(Ease.Linear));
                fromPos = targetPos;
                targetPos = ViewModel.TrajectoryPositions[1];
                animTime = GetAnimationTime(fromPos, targetPos);
                sequence.Append(transform.DOLocalMove(targetPos, animTime).SetEase(Ease.Linear));
            }
            else
            {
                var targetPos = ViewModel.TrajectoryPositions[0];
                var fromPos = transform.localPosition;
                var animTime = GetAnimationTime(fromPos, targetPos);
                sequence.Append(transform.DOLocalMove(targetPos, animTime).SetEase(Ease.Linear));
            }

            sequence.AppendCallback(() =>
            {
                ResetSorting();
                _trailRenderer.enabled = false;
                ViewModel.SetAnimationComplete();
            });
        }

        private void AnimateMergeToTarget(Vector2Int target)
        {
            var targetPos = transform.parent
                .InverseTransformPoint(_boardView.BubblesParent.position + HexGridUtils.HexGridToWorld(target));
            _bubbleBurstViewFactory.Create(ViewModel.Exponent.Value, transform.position);

            if (targetPos.magnitude < 0.1f)
            {
                Destroy(gameObject);
                return;
            }

            _audioService.PlaySfx(SfxId.Pop);
            SortToBack();
            transform.DOKill();
            transform.DOLocalMove(targetPos, _bubbleSetting.MergeTime)
                .OnComplete(() => Destroy(gameObject));
        }

        private void AnimateDrop()
        {
            gameObject.transform.SetParent(null);
            var position = transform.position;
            var targetPos = new Vector3(position.x, _boardView.NextBubbleAnchor.position.y, position.y);
            transform.DOKill();
            transform.DOMove(targetPos, 3 * GetAnimationTime(position, targetPos)).SetEase(Ease.InQuad)
                .OnComplete(() =>
                {
                    _bubbleBurstViewFactory.Create(ViewModel.Exponent.Value, transform.position);
                    _audioService.PlaySfx(SfxId.Pop);
                    Destroy(gameObject);
                });
        }

        private void AnimateExponentChange()
        {
            _valueChangedRt.anchoredPosition = Vector2.zero;
            _valueChangedText.color = _valueChangedText.color.WithAlpha(1f);
            _valueChangedRt.gameObject.SetActive(true);
            _serialDisposable.Disposable = DOTween.Sequence()
                .Append(_valueChangedRt.DOAnchorPosY(20, 0.4f))
                .Append(_valueChangedText.DOFade(0f, 0.4f))
                .OnComplete(() => _valueChangedRt.gameObject.SetActive(false))
                .ToDisposable();
        }

        private float GetAnimationTime(Vector3 from, Vector3 to)
        {
            var distance = Vector3.Distance(from, to);
            var animTime = distance / _bubbleSetting.BubbleSpeed;
            return animTime;
        }

        private void SortToFront()
        {
            _spriteRenderers.ForEach(s => s.sortingLayerID = _frontLayerId);
            _canvas.sortingLayerID = _frontLayerId;
        }

        private void SortToBack()
        {
            _spriteRenderers.ForEach(s => s.sortingLayerID = _backLayerId);
            _canvas.sortingLayerID = _backLayerId;
        }

        private void ResetSorting()
        {
            _spriteRenderers.ForEach(s => s.sortingLayerID = _mainLayerId);
            _canvas.sortingLayerID = _mainLayerId;
        }
    }
}