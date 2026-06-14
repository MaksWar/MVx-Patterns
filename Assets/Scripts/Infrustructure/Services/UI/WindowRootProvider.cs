using UnityEngine;

namespace Infrustructure.Services.UI
{
    public sealed class WindowRootProvider : IWindowRootProvider
    {
        public Transform GetRoot(WindowGuiLayer guiLayer)
        {
            Transform windowsRoot = GUIController.Instance.WindowsRoot;

            return guiLayer == WindowGuiLayer.GUIOverHUD
                ? OverHUDController.Instance.WindowsRoot != null ? OverHUDController.Instance.WindowsRoot : windowsRoot
                : windowsRoot;
        }
    }
}
