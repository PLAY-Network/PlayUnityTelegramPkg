using UnityEngine;

namespace RGN.Modules.Telegram
{
    public class TelegramMessageReceiver : MonoBehaviour
    {
        public event System.Action OnFullscreenChanged;
        public event System.Action<string> OnFullscreenFailed;

#if DEBUG
        private void Start()
        {
            Debug.Log("[TelegramMessageReceiver] Start");
        }
#endif

        private void FullscreenChangedMessage()
        {
#if DEBUG
            Debug.Log("[TelegramMessageReceiver] Fullscreen changed");
#endif
            OnFullscreenChanged?.Invoke();
        }

        private void FullscreenFailedMessage(string error)
        {
#if DEBUG
            Debug.Log("[TelegramMessageReceiver] Fullscreen failed, error: " + error);
#endif
            OnFullscreenFailed?.Invoke(error);
        }
    }
}
