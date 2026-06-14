namespace Infrustructure.Services.MVPWindows
{
    public abstract class BaseWindowModel<TArgs> : IWindowModel where TArgs : WindowArgs
    {
        public TArgs Args { get; private set; }
        public WindowOptions Options { get; private set; }

        public virtual void Initialize(TArgs args, WindowOptions options)
        {
            Args = args;
            Options = options;
        }
    }
}
