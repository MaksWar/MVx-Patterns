using UnityEngine;

namespace Infrustructure.Services.UI
{
    public interface IWindowRootProvider
    {
        Transform GetRoot(WindowGuiLayer guiLayer);
    }
}
