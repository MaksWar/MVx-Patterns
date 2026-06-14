using UnityEngine;

namespace Infrustructure.Services.Windows
{
    public class GUIController : MonoBehaviour
    {
        [SerializeField] private Transform windowsRoot;

        public static GUIController Instance { get; private set; }

        public Transform WindowsRoot => windowsRoot;

        private void Awake() =>
            Instance = this;
    }
}
