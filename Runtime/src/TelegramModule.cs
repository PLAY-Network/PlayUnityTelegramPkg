using System.Threading;
using System.Threading.Tasks;
using RGN.ImplDependencies.Engine;
using RGN.Modules.SignIn;
using RGN.Modules.SignIn.DeviceFlow;

namespace RGN.Modules.Telegram
{
    [Attributes.GeneratorExclude]
    public class TelegramModule : BaseModule<TelegramModule>, IRGNModule
    {
        private TelegramInitParams _initParams;
        
        public override void Init()
        {
            if (RGNCore.I.Dependencies.EngineApp is IEngineAppOpenUrlPatcher patcher)
            {
                patcher.PatchOpenUrl(TelegramJavascriptBridge.OpenLink);
            }
            
            EmailSignInModule.I.PatchSignInWithDeviceCodeFunction(SignInWithDeviceCodeAsync);
            
            _initParams = TelegramJavascriptBridge.GetInitParams();
        }

        public async Task<ISignInWithDeviceCodeIntent> SignInWithDeviceCodeAsync(CancellationToken cancellationToken = default)
        {
            SignInWithDeviceCodeIntent signInWithDeviceCodeIntent = new SignInWithDeviceCodeIntent(_rgnCore);
            if (_initParams.AppPlatform == "web" || _initParams.AppPlatform == "weba")
            {
                signInWithDeviceCodeIntent.SetImmediateMode(true);
                await signInWithDeviceCodeIntent.RequestDeviceCodeAsync(cancellationToken);
            }
            return signInWithDeviceCodeIntent;
        }
    }
}
