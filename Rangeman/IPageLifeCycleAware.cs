namespace Rangeman
{
    public interface IPageLifeCycleAware
    {
        void OnAppearing();
        void OnDisappearing();
    }
}