using UnityEngine;

namespace Infrustructure.Services.Windows
{
    public class Example1WindowController : BaseWindowController<Example1WindowController.ExampleCustomParams>
    {
        public const string PrefabName = "Example1Window";

        public override void AfterOpen()
        {
            base.AfterOpen();
            
            Debug.Log(CustomParams.Shoc);
            Debug.Log(CustomParams.workstationId);
        }

        public class ExampleCustomParams : BaseWindowParams
        {
            public string workstationId;
            public string Shoc;
        }
    }
}