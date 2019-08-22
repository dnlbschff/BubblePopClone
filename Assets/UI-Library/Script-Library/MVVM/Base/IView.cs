namespace DB.Library.MVVM
{
    public interface IView
    {
        void TrySetViewModel(IViewModel vm);
        void InitializeView();
    }
}
