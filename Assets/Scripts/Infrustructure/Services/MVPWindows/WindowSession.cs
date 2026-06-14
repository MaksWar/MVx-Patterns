using System;
using UnityEngine;

namespace Infrustructure.Services.MVPWindows
{
    public sealed class WindowSession : IDisposable
    {
        public WindowSession(string name, GameObject gameObject, IWindowView view, IWindowPresenter presenter)
        {
            Name = name;
            GameObject = gameObject;
            View = view;
            Presenter = presenter;
        }

        public string Name { get; }
        public GameObject GameObject { get; }
        public IWindowView View { get; }
        public IWindowPresenter Presenter { get; }

        public void Dispose() =>
            Presenter.Dispose();
    }
}
