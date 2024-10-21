using UnityEditor.Build;
using UnityEditor.Build.Reporting;

namespace RGN.Modules.Telegram.Editor
{
    public class BuildPostProcessor : IPostprocessBuildWithReport
    {
        public int callbackOrder => 0;

        public void OnPostprocessBuild(BuildReport report)
        {

        }
    }
}
