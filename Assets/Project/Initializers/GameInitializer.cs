using BPC.Board;
using BPC.Bubbles;
using BPC.Config;
using UnityEngine;
using Zenject;

namespace BPC.Initializers
{
    public class GameInitializer : IInitializable
    {
        [Inject] private GameConfig.BoardSettings _boardSetting;
        [Inject] private BubbleView.Factory _bubbleViewFactory;
        [Inject] private BoardViewModel _boardViewModel;
        [Inject] private BoardView _boardView;

        public void Initialize()
        {
            InitializeBoard();
        }

        private void InitializeBoard()
        {
            var minRow = _boardSetting.Min.y;
            var maxColumns = _boardSetting.Max.x - _boardSetting.Min.x;
            var maxRows = _boardSetting.InitialRows;

            for (var row = minRow; row < maxRows; row++)
            {
                for (var col = row % 2; col < maxColumns; col += 2)
                {
                    if (row == maxRows - 1 && Random.Range(0f, 1f) < 0.3f)
                    {
                        continue;
                    }

                    _bubbleViewFactory.Create(
                        Random.Range(1, _boardSetting.InitialMaxSpawnExponent),
                        new Vector2Int(col, row),
                        true);
                }
            }

            _boardViewModel.SetIsInitialized(true);
        }
    }
}