using BPC.Config;
using BPC.Utils;
using DB.Library.MVVM;
using TMPro;
using UniRx;
using UnityEngine;
using Zenject;

namespace BPC.Bubbles
{
    public class BubbleView : ViewBase<BubbleViewModel>
    {
        [Inject]
        private GameConfig.BubbleSettings _bubbleSetting;

        [SerializeField] private TextMeshProUGUI _valueText;
        [SerializeField] private TextMeshProUGUI _valueChangedText;
        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private Collider2D _collider;
        [SerializeField] private TextMeshProUGUI _debugText;

        protected override void SetUp()
        {
            _debugText.gameObject.SetActive(_bubbleSetting.ShowDebugText);
            base.SetUp();
        }

        protected override void BindSubscriptions(CompositeDisposable disposer)
        {
            Observable.Merge(
                    ViewModel.IsConnectedToCeiling.AsUnitObservable(),
                    ViewModel.GridPosition.AsUnitObservable())
                .Subscribe(_ => UpdateDebugText()).AddTo(Disposer);

            ViewModel.GridPosition
                .Where(_ => ViewModel.IsOnGrid.Value)
                .Subscribe(OnGridPositionChanged).AddTo(disposer);

            ViewModel.Exponent.Subscribe(OnExponentChanged).AddTo(disposer);

            ViewModel.IsOnGrid
                .Subscribe(isOnGrid => _collider.enabled = isOnGrid).AddTo(Disposer);
        }

        private void UpdateDebugText()
        {
            var isConnected = ViewModel.IsConnectedToCeiling.Value;
            var pos = ViewModel.GridPosition.Value;
            _debugText.text = $"{isConnected}\n({pos.x},{pos.y})";
        }

        private void OnGridPositionChanged(Vector2Int pos)
        {
            transform.localPosition = HexGridUtils.HexGridToWorld(pos);
        }

        private void OnExponentChanged(int exponent)
        {
            var valueString = NumberFormatter.FormatBubbleValue((int) Mathf.Pow(2, exponent));
            _valueText.text = valueString;
            _valueChangedText.text = valueString;
            _spriteRenderer.color = _bubbleSetting.GetBubbleColor(exponent);
            _spriteRenderer.sprite = _bubbleSetting.GetBubbleSprite(exponent);
        }

        public class Factory : PlaceholderFactory<int, Vector2Int, bool, BubbleView>
        {
        }
    }
}