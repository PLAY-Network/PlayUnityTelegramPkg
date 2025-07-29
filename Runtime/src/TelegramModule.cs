using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using RGN.Modules.SignIn.DeviceFlow;
using UnityEngine;

namespace RGN.Modules.Telegram
{
    [Attributes.GeneratorExclude]
    public class TelegramModule : BaseModule<TelegramModule>, IRGNModule
    {
        private TelegramMessageReceiver _messageReceiver;
        
        public TelegramInitParams InitParams { get; private set; }
        
        public override void Init()
        {
            if (!IsAvailable())
            {
                return;
            }

            GameObject messageReceiverObj = new GameObject(nameof(TelegramMessageReceiver));
            _messageReceiver = messageReceiverObj.AddComponent<TelegramMessageReceiver>();
            Object.DontDestroyOnLoad(messageReceiverObj);
            
#if UNITY_WEBGL && !UNITY_EDITOR
            if (RGNCore.I.Dependencies.EngineApp is ImplDependencies.Engine.IEngineAppOpenUrlPatcher patcher)
            {
                patcher.PatchOpenUrl(TelegramJavascriptBridge.OpenLink);
            }
            
            RGN.Modules.SignIn.EmailSignInModule.I.PatchSignInWithDeviceCodeFunction(SignInWithDeviceCodeAsync);
#endif
            
            InitParams = TelegramJavascriptBridge.GetInitParams();
        }
        
        public bool IsAvailable() => TelegramJavascriptBridge.IsTelegramAvailable();
        
        public bool IsFullscreen() => TelegramJavascriptBridge.IsFullscreen();

        [SuppressMessage("ReSharper", "AccessToModifiedClosure")]
        public void RequestFullscreen(System.Action<bool> callback = null)
        {
            System.Action onChanged = null;
            System.Action<string> onFailed = null;

            onChanged = () =>
            {
                _messageReceiver.OnFullscreenChanged -= onChanged;
                _messageReceiver.OnFullscreenFailed -= onFailed;
                callback?.Invoke(true);
            };
            onFailed = error =>
            {
                _messageReceiver.OnFullscreenChanged -= onChanged;
                _messageReceiver.OnFullscreenFailed -= onFailed;
                callback?.Invoke(false);
            };

            _messageReceiver.OnFullscreenChanged += onChanged;
            _messageReceiver.OnFullscreenFailed += onFailed;

            TelegramJavascriptBridge.RequestFullscreen();
        }

        [SuppressMessage("ReSharper", "AccessToModifiedClosure")]
        public void ExitFullscreen(System.Action<bool> callback = null)
        {
            System.Action onChanged = null;
            System.Action<string> onFailed = null;

            onChanged = () =>
            {
                _messageReceiver.OnFullscreenChanged -= onChanged;
                _messageReceiver.OnFullscreenFailed -= onFailed;
                callback?.Invoke(true);
            };
            
            onFailed = error =>
            {
                _messageReceiver.OnFullscreenChanged -= onChanged;
                _messageReceiver.OnFullscreenFailed -= onFailed;
                callback?.Invoke(false);
            };

            _messageReceiver.OnFullscreenChanged += onChanged;
            _messageReceiver.OnFullscreenFailed += onFailed;
            
            TelegramJavascriptBridge.ExitFullscreen();
        }

        public async Task<ISignInWithDeviceCodeIntent> SignInWithDeviceCodeAsync(CancellationToken cancellationToken = default)
        {
            SignInWithDeviceCodeIntent signInWithDeviceCodeIntent = new SignInWithDeviceCodeIntent(_rgnCore);
            if (InitParams.AppPlatform == "web" || InitParams.AppPlatform == "weba")
            {
                signInWithDeviceCodeIntent.SetImmediateMode(true);
                await signInWithDeviceCodeIntent.RequestDeviceCodeAsync(cancellationToken);
            }
            return signInWithDeviceCodeIntent;
        }
    }
}
