namespace Infrustructure.Services.MVPWindows
{
    public interface IWindowPresenterFactory
    {
        bool CanCreate(string windowName);
        IWindowPresenter Create(string windowName, IWindowView view, IWindowsManager windowsManager);
    }
}
