using System;
using Cysharp.Threading.Tasks;
using Infrustructure.Services.UI;
using UnityEngine;

namespace Infrustructure.Services.MVPWindows
{
    public interface IWindowView
    {
        event Action CloseClicked;

        GameObject GameObject { get; }
        Transform Transform { get; }
        WindowGuiLayer GuiLayer { get; }

        UniTask InitializeAsync();
        UniTask PlayOpenAsync();
        UniTask PlayCloseAsync();

        void SetVisible(bool isVisible);
        void ResetTransform();
    }
}
