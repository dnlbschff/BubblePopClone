using DB.Library.MVVM;
using UniRx;

namespace BPC.Bubbles
{
    public class BubbleBurstViewModel : ViewModelBase
    {
        private readonly ReactiveProperty<int> _exponent;
        public IReadOnlyReactiveProperty<int> Exponent => _exponent;

        public BubbleBurstViewModel()
        {
            _exponent = new ReactiveProperty<int>(1).AddTo(Disposer);
        }

        public void SetExponent(int exponent)
        {
            _exponent.Value = exponent;
        }
    }
}