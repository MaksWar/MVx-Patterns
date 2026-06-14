# MVP Window System

A production-ready, async-first Model-View-Presenter window management system for Unity, built on top of [UniTask](https://github.com/Cysharp/UniTask) and [Zenject (Extenject)](https://github.com/modesttree/Zenject).

---

## Table of Contents

- [Architecture Overview](#architecture-overview)
- [Core Concepts](#core-concepts)
  - [Model](#model)
  - [View](#view)
  - [Presenter](#presenter)
- [Orchestration Layer](#orchestration-layer)
  - [IWindowsManager / WindowsManager](#iwindowsmanager--windowsmanager)
  - [WindowFactory](#windowfactory)
  - [WindowSession](#windowsession)
- [Extension Points](#extension-points)
  - [IWindowPresenterFactory](#iwindowpresenterfactory)
  - [Lifecycle Hooks](#lifecycle-hooks)
  - [WindowOptions & WindowArgs](#windowoptions--windowargs)
  - [WindowGuiLayer](#windowguilayer)
- [Creating a New Window](#creating-a-new-window)
- [Design Decisions](#design-decisions)
- [Dependency Graph](#dependency-graph)

---

## Architecture Overview

The system follows the **Model-View-Presenter** (MVP) pattern, applied specifically to UI windows in a Unity mobile game context. Each window is an independent MVP triad:

```
IWindowsManager
      │
      ├── WindowFactory  ──────────────────► IWindowPresenterFactory
      │         │                                      │
      │         ▼                                      ▼
      │    WindowSession ◄──────────── BaseWindowPresenter<TView, TModel, TArgs>
      │         │                            │                  │
      │         │                      IWindowView        BaseWindowModel<TArgs>
      │         │                  (BaseWindowView)
      └── manages session lifecycle
```

The orchestration layer (`WindowsManager`, `WindowFactory`, `WindowSession`) is completely separate from the MVP triad itself. Each concrete window only needs to define its own `View`, `Model`, `Presenter`, and `PresenterFactory` - the rest is handled automatically.

---

## Core Concepts

### Model

```csharp
public abstract class BaseWindowModel<TArgs> : IWindowModel
    where TArgs : WindowArgs
{
    public TArgs Args { get; private set; }
    public WindowOptions Options { get; private set; }

    public virtual void Initialize(TArgs args, WindowOptions options) { ... }
}
```

The Model is a plain data container. It holds the arguments passed when the window was opened (`TArgs`) and the display options (`WindowOptions`). It contains no Unity-specific code and no references to the View or Presenter.

`WindowArgs` is an empty base class - extend it to pass any data your specific window needs:

```csharp
public class ShopWindowArgs : WindowArgs
{
    public string CategoryId;
    public bool ShowSpecialOffer;
}
```

### View

```csharp
public abstract class BaseWindowView : MonoBehaviour, IWindowView
{
    [SerializeField] private List<Button> closeButtons;
    [SerializeField] private BaseWindowAnimationComponent[] animationComponents;
    [SerializeField] private WindowGuiLayer guiLayer = WindowGuiLayer.GUI;

    public event Action CloseClicked;

    public virtual UniTask PlayOpenAsync() { ... }
    public virtual UniTask PlayCloseAsync() { ... }
    public void SetVisible(bool isVisible) => gameObject.SetActive(isVisible);
}
```

The View is a passive `MonoBehaviour`. Its only responsibilities are:

- Playing open/close animations (driven by pluggable `BaseWindowAnimationComponent` components)
- Surfacing user input via events (e.g. `CloseClicked`)
- Exposing its `GuiLayer` so the manager can place it in the correct canvas hierarchy

The View **never calls into the Presenter directly** and holds no business logic. Animation components are plugged in via the Inspector, keeping the View class clean.

### Presenter

```csharp
public abstract class BaseWindowPresenter<TView, TModel, TArgs> : IWindowPresenter
    where TView : IWindowView
    where TModel : BaseWindowModel<TArgs>
    where TArgs : WindowArgs, new()
{
    protected readonly TView View;
    protected readonly TModel Model;

    public async UniTask OpenAsync(WindowArgs args, WindowOptions options) { ... }
    public async UniTask CloseAsync() { ... }
}
```

The Presenter is the active layer. It:

- Receives typed `TArgs` and `TModel` - no untyped casts
- Calls `View.PlayOpenAsync()` / `View.PlayCloseAsync()` to drive animations
- Listens to `View.CloseClicked` and delegates back to `IWindowsManager`
- Exposes lifecycle hooks for concrete subclasses to inject custom logic

The Presenter has **no `MonoBehaviour` dependency** and can be unit tested in isolation by mocking `TView` and using a real `TModel`.

---

## Orchestration Layer

### IWindowsManager / WindowsManager

```csharp
public interface IWindowsManager
{
    UniTask InitializeAsync();
    UniTask OpenWindowAsync(string windowName, WindowArgs args = null, WindowOptions options = null);
    UniTask CloseWindowAsync(string windowName);
    UniTask CloseAllWindows();

    bool IsWindowOpened();
    bool IsWindowOpened(string windowName);
    bool IsWindowInProcess(string windowName);
    bool IsWindowInProcessOrOpened(string windowName);

    TView GetCurrentWindowView<TView>() where TView : class, IWindowView;
    void Unload();
}
```

`WindowsManager` is the single point of entry for all window operations. It handles:

- **Session caching** - each window is instantiated once and reused; prefabs are not destroyed between opens
- **Sequential open/close** - opening a new window automatically closes the current one before proceeding
- **GUI layer routing** - moves the view's `Transform` to the correct canvas root (`GUI` or `GUIOverHUD`) based on `WindowOptions` or the View's default `GuiLayer`
- **In-process guard** - `_windowInProcess` prevents re-entrant opens for the same window

### WindowFactory

`WindowFactory` is responsible for creating a complete `WindowSession` from a window name:

1. Loads the prefab via `IAssetsProvider` (Addressables) using the key `MVPWindows/{windowName}.prefab`
2. Instantiates it through Zenject's `DiContainer` for automatic injection
3. Retrieves the `IWindowView` component from the root GameObject
4. Delegates Presenter creation to a registered `IWindowPresenterFactory`
5. Calls `view.InitializeAsync()` and `view.ResetTransform()` before returning

If no factory can handle the window name, or the prefab root is missing `IWindowView`, the factory logs an error and returns `null` - nothing crashes silently.

### WindowSession

```csharp
public sealed class WindowSession : IDisposable
{
    public string Name { get; }
    public GameObject GameObject { get; }
    public IWindowView View { get; }
    public IWindowPresenter Presenter { get; }

    public void Dispose() => Presenter.Dispose();
}
```

A lightweight value object that ties together the name, the live `GameObject`, the typed View interface, and the Presenter. `Dispose()` propagates to the Presenter so event subscriptions are cleaned up correctly.

---

## Extension Points

### IWindowPresenterFactory

```csharp
public interface IWindowPresenterFactory
{
    bool CanCreate(string windowName);
    IWindowPresenter Create(string windowName, IWindowView view, IWindowsManager windowsManager);
}
```

This is how new window types are registered without modifying `WindowFactory`. Each window module provides its own factory, bound in Zenject:

```csharp
// In an installer:
Container.Bind<IWindowPresenterFactory>().To<ShopWindowPresenterFactory>().AsSingle();
```

`WindowFactory` holds a `List<IWindowPresenterFactory>` (injected by Zenject) and picks the first factory that returns `true` from `CanCreate`.

### Lifecycle Hooks

`BaseWindowPresenter` exposes six virtual hook methods that concrete presenters can override independently:

| Hook | When it fires |
|------|---------------|
| `BeforeOpen()` | Before `View.PlayOpenAsync()` |
| `BeforeOpenAsync()` | Before `View.PlayOpenAsync()` (async) |
| `AfterOpen()` | After `View.PlayOpenAsync()` completes |
| `AfterOpenAsync()` | After `View.PlayOpenAsync()` completes (async) |
| `BeforeClose()` | Before `View.PlayCloseAsync()` |
| `BeforeCloseAsync()` | Before `View.PlayCloseAsync()` (async) |
| `AfterClose()` | After `View.PlayCloseAsync()` completes |
| `AfterCloseAsync()` | After `View.PlayCloseAsync()` completes (async) |

Override only what you need - the defaults are all no-ops.

### WindowOptions & WindowArgs

`WindowOptions` controls display behaviour per-call:

```csharp
public class WindowOptions
{
    public Transform Parent;        // Override the canvas root for this open
    public WindowGuiLayer? GuiLayer; // Override the default GUI layer
    public Action OnOpened;         // Callback after open animation completes
    public Action OnClosed;         // Callback after close animation completes
}
```

`WindowArgs` is the typed data payload. Subclass it per window - the base class is empty by design so windows with no arguments still compile without boilerplate.

### WindowGuiLayer

```csharp
public enum WindowGuiLayer
{
    GUI = 0,        // Standard canvas root (GUIController)
    GUIOverHUD = 1  // Overlay canvas root (OverHUDController)
}
```

Each `BaseWindowView` declares its preferred layer via a serialized field. `WindowOptions.GuiLayer` overrides it at open time. `WindowRootProvider` resolves the concrete `Transform` root for each layer at runtime.

---

## Creating a New Window

**1. Define the args (if needed)**

```csharp
public class InventoryWindowArgs : WindowArgs
{
    public string FilterCategory;
}
```

**2. Define the model**

```csharp
public class InventoryWindowModel : BaseWindowModel<InventoryWindowArgs>
{
    public List<ItemData> Items { get; private set; }

    public override void Initialize(InventoryWindowArgs args, WindowOptions options)
    {
        base.Initialize(args, options);
        Items = LoadItems(args.FilterCategory);
    }
}
```

**3. Define the view interface and implementation**

```csharp
public interface IInventoryWindowView : IWindowView
{
    void SetItems(IReadOnlyList<ItemData> items);
    event Action<string> ItemClicked;
}

public class InventoryWindowView : BaseWindowView, IInventoryWindowView
{
    public event Action<string> ItemClicked;

    public void SetItems(IReadOnlyList<ItemData> items) { /* populate list */ }
}
```

**4. Define the presenter**

```csharp
public class InventoryWindowPresenter
    : BaseWindowPresenter<IInventoryWindowView, InventoryWindowModel, InventoryWindowArgs>
{
    public InventoryWindowPresenter(
        IInventoryWindowView view,
        InventoryWindowModel model,
        IWindowsManager windowsManager)
        : base(view, model, windowsManager) { }

    protected override void AfterOpen()
    {
        View.SetItems(Model.Items);
        View.ItemClicked += OnItemClicked;
    }

    protected override void AfterClose()
    {
        View.ItemClicked -= OnItemClicked;
    }

    private void OnItemClicked(string itemId) { /* handle */ }
}
```

**5. Register the factory**

```csharp
public class InventoryWindowPresenterFactory : IWindowPresenterFactory
{
    private const string WindowName = "InventoryWindow";

    private readonly DiContainer _container;

    public InventoryWindowPresenterFactory(DiContainer container)
        => _container = container;

    public bool CanCreate(string windowName) => windowName == WindowName;

    public IWindowPresenter Create(string windowName, IWindowView view, IWindowsManager windowsManager)
    {
        var typedView = (IInventoryWindowView)view;
        var model = new InventoryWindowModel();
        return new InventoryWindowPresenter(typedView, model, windowsManager);
    }
}
```

```csharp
// In installer:
Container.Bind<IWindowPresenterFactory>().To<InventoryWindowPresenterFactory>().AsSingle();
```

**6. Place the prefab**

Put the prefab at the Addressables path `MVPWindows/InventoryWindow.prefab`. The root GameObject must have `InventoryWindowView` attached.

**7. Open the window**

```csharp
await _windowsManager.OpenWindowAsync(
    "InventoryWindow",
    new InventoryWindowArgs { FilterCategory = "weapons" },
    new WindowOptions { OnOpened = () => Debug.Log("Inventory open") }
);
```

---

## Design Decisions

**Why UniTask and not coroutines?**
UniTask allows `await`-based sequencing for animations without the fragility of `StartCoroutine` chains. Open and close animations are truly sequential - `PlayOpenAsync` completes before `AfterOpen` fires.

**Why not MVVM with data binding?**
Unity's UI systems (uGUI) don't have a native binding layer. Introducing one adds complexity without clear benefit for typical mobile game UIs where the view is manually driven. The Presenter explicitly pushes data to the View, which is easier to trace and debug.

**Why session caching?**
Window prefabs are instantiated once and hidden rather than destroyed between opens. This avoids repeated asset loading and Addressables overhead for frequently-used windows like inventories or shops.

**Why string-based window names?**
String keys map directly to Addressables paths and allow windows to be opened from any context (including JSON-driven flows or analytics events) without compile-time dependencies on specific presenter types. A constants file or `static readonly` fields per module avoids magic strings at the call site.

---

## Dependency Graph

```
IWindowsManager
    └── WindowsManager
            ├── IWindowFactory
            │       └── WindowFactory
            │               ├── IAssetsProvider        (Addressables)
            │               ├── DiContainer            (Zenject)
            │               └── List<IWindowPresenterFactory>
            │                       └── (one per window module)
            ├── IWindowRootProvider
            │       └── WindowRootProvider
            │               └── GUIController.Instance / OverHUDController.Instance
            └── IUIRootInitializer
                    └── UIRootInitializer
                            ├── IAssetsProvider
                            └── DiContainer
```
