using DB.Library.MVVM;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace BPC.Bubbles
{
    public class NextBubblesView : ViewBase<NextBubblesViewModel>
    {
        [SerializeField]
        private Button _switchBubblesButton;
        
        protected override void BindSubscriptions(CompositeDisposable disposer)
        {
            _switchBubblesButton.OnClickAsObservable()
                .Subscribe(_ => ViewModel.SwitchBubbles()).AddTo(Disposer);
        }
    }
}