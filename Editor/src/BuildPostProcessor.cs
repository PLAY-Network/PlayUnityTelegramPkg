using System;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace RGN.Modules.Telegram.Editor
{
    public class BuildPostProcessor : IPostprocessBuildWithReport
    {
        public int callbackOrder => 9999999;

        public void OnPostprocessBuild(BuildReport report)
        {
            if (report.summary.platform != BuildTarget.WebGL)
            {
                return;
            }
            
            string indexPath = System.IO.Path.Combine(report.summary.outputPath, "index.html");
            if (!System.IO.File.Exists(indexPath))
            {
                Debug.LogWarning("Failed to find index.html in build output");
                return;
            }

            string indexContent = System.IO.File.ReadAllText(indexPath);
            if (indexContent.Contains("https://telegram.org/js/telegram-web-app.js"))
            {
                Debug.LogWarning("Telegram script already exists in index.html");
                return;
            }
            
            string telegramScript = @"
<!-- Telegram -->
<script src=""https://telegram.org/js/telegram-web-app.js""></script> 
<script> 
  if (window.Telegram && window.Telegram.WebApp) { 
    window.Telegram.WebApp.onEvent(""viewportChanged"", () => window.scrollTo(0, 0)); 
  }
</script>";
            
            System.Text.RegularExpressions.Match bodyTagMatch = System.Text.RegularExpressions.Regex
                .Match(indexContent, @"(\s*)</body>");
            if (!bodyTagMatch.Success)
            {
                Debug.LogError("Failed to find </body> tag in index.html");
                return;
            }
            
            int bodyTagIndex = bodyTagMatch.Index;
            string bodyIndent = bodyTagMatch.Groups[1].Value
                .Replace(Environment.NewLine, string.Empty)
                .Replace("\n", string.Empty)
                .Replace("\r", string.Empty);
            
            string scriptIndent = string.Concat(System.Linq.Enumerable.Repeat(bodyIndent, 2));
            string indentedScript = Environment.NewLine + System.Text.RegularExpressions.Regex
                .Replace(telegramScript.Trim(), "^", scriptIndent, System.Text.RegularExpressions.RegexOptions.Multiline);
            indexContent = indexContent
                .Insert(bodyTagIndex, indentedScript);
            System.IO.File.WriteAllText(indexPath, indexContent);
        }
    }
}
