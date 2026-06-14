using UnityEngine;

namespace Infrustructure.Services.Windows
{
    public class OverHUDController : MonoBehaviour
    {
        [SerializeField] private Transform windowsRoot;

        public static OverHUDController Instance { get; private set; }

        public Transform WindowsRoot => windowsRoot;

        private void Awake() =>
            Instance = this;
    }
}
