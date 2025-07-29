using System.Threading;
using System.Threading.Tasks;
using RGN.Modules.SignIn.DeviceFlow;

namespace RGN.Modules.Telegram
{
    [Attributes.GeneratorExclude]
    public class TelegramModule : BaseModule<TelegramModule>, IRGNModule
    {
        public TelegramInitParams InitParams { get; private set; }
        
        public override void Init()
        {
            if (!IsAvailable())
            {
                return;
            }
            
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
        
        public bool IsFullscreenSupported() => TelegramJavascriptBridge.IsFullscreenSupported();

        public void RequestFullscreen() => TelegramJavascriptBridge.RequestFullscreen();

        public void ExitFullscreen() => TelegramJavascriptBridge.ExitFullscreen();

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
