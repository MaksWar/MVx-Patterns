using System;
using UnityEngine;
using Infrustructure.Services.UI;

namespace Infrustructure.Services.MVPWindows
{
    public class WindowArgs
    {
    }

    [Serializable]
    public class WindowOptions
    {
        public Transform Parent;
        public WindowGuiLayer? GuiLayer;

        public Action OnOpened;
        public Action OnClosed;
    }
}
