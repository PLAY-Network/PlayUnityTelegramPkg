using UnityEngine;

namespace RGN.Modules.Telegram
{
    public class TelegramMessageReceiver : MonoBehaviour
    {
        public event System.Action OnFullscreenChanged;
        public event System.Action<string> OnFullscreenFailed;

        private void Start()
        {
            Debug.Log("[TelegramMessageReceiver] Start");
        }

        private void FullscreenChangedMessage()
        {
            OnFullscreenChanged?.Invoke();
        }

        private void FullscreenFailedMessage(string error)
        {
            OnFullscreenFailed?.Invoke(error);
        }
    }
}
