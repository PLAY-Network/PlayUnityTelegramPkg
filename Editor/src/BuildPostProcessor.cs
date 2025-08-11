using System;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace RGN.Modules.Telegram.Editor
{
    public class BuildPostProcessor : IPostprocessBuildWithReport
    {
        public int callbackOrder => 9999999;

        private const string TelegramScriptURL = "https://telegram.org/js/telegram-web-app.js";
        private const string TelegramEventHandlerID = "telegram-event-handlers";

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
            string modifiedContent = indexContent;
            
            Match bodyCloseMatch = Regex.Match(modifiedContent, @"(\s*)</body>");
            if (!bodyCloseMatch.Success)
            {
                Debug.LogError("Failed to find closing body tag in index.html");
                return;
            }
            
            // Step 1: Check if Telegram script exists
            bool hasTelegramScript = Regex.IsMatch(indexContent, @"<script[^>]*src\s*=\s*[""']" + Regex.Escape(TelegramScriptURL) + @"[""'][^>]*>");
            int telegramScriptPosition;
            
            if (hasTelegramScript)
            {
                Match scriptMatch = Regex.Match(modifiedContent, @"<script[^>]*src\s*=\s*[""']" + Regex.Escape(TelegramScriptURL) + @"[""'][^>]*>.*?</script>");
                telegramScriptPosition = scriptMatch.Index + scriptMatch.Length;
            }
            else
            {
                int scriptInsertPos = bodyCloseMatch.Index;
                string scriptIndentation = RemoveCRLF(bodyCloseMatch.Groups[1].Value + "  ");
                string[] scriptLines = {
                    $"<script src=\"{TelegramScriptURL}\"></script>"
                };
                string scriptToInsert = FormatLines(scriptLines, scriptIndentation);
                modifiedContent = modifiedContent.Insert(scriptInsertPos, scriptToInsert);
                telegramScriptPosition = scriptInsertPos + scriptToInsert.LastIndexOf("</script>", StringComparison.Ordinal) + "</script>".Length;
            }
            
            // Step 2: Handle event handlers - remove old and add new one
            Match handlersMatch = Regex.Match(modifiedContent, $@"<script[^>]*id\s*=\s*[""]?{TelegramEventHandlerID}[""]?[^>]*>[\s\S]*?</script>");
            if (handlersMatch.Success)
            {
                modifiedContent = modifiedContent.Remove(handlersMatch.Index, handlersMatch.Length);
                
                if (telegramScriptPosition > handlersMatch.Index)
                {
                    telegramScriptPosition -= handlersMatch.Length;
                }
            }
            
            string handlersIndentation = RemoveCRLF(GetIndentationAtPosition(modifiedContent, telegramScriptPosition));
            string[] handlersLines = {
                $"<script id=\"{TelegramEventHandlerID}\">",
                "  if (window.Telegram && window.Telegram.WebApp) {",
                "    window.Telegram.WebApp.onEvent(\"viewportChanged\", () => window.scrollTo(0, 0));",
                "    window.Telegram.WebApp.onEvent(\"fullscreenChanged\", () => {",
                "      if (unityInstance) {",
                "        unityInstance.SendMessage(\"TelegramMessageReceiver\", \"FullscreenChangedMessage\");",
                "      }",
                "    });",
                "    window.Telegram.WebApp.onEvent(\"fullscreenFailed\", (event) => {",
                "      if (unityInstance) {",
                "        unityInstance.SendMessage(\"TelegramMessageReceiver\", \"FullscreenFailedMessage\", event.error);",
                "      }",
                "    });",
                "  }",
                "</script>"
            };
            string handlersToInsert = FormatLines(handlersLines, handlersIndentation);
            modifiedContent = modifiedContent.Insert(telegramScriptPosition, handlersToInsert);
            
            if (modifiedContent != indexContent)
            {
                System.IO.File.WriteAllText(indexPath, modifiedContent);
                Debug.Log("Successfully updated index.html with Telegram integration");
            }
        }
        
        private string GetIndentationAtPosition(string content, int position)
        {
            position = Math.Min(position, content.Length);
            
            int lineStart = content.LastIndexOf('\n', Math.Max(0, position - 1));
            lineStart = lineStart + 1;
            
            if (lineStart > position)
            {
                lineStart = Math.Max(0, position);
            }
            
            string line = content.Substring(lineStart, position - lineStart);
            return Regex.Match(line, @"^\s*").Value;
        }
        
        private string RemoveCRLF(string indentation) => string.IsNullOrEmpty(indentation) 
            ? string.Empty 
            : Regex.Replace(indentation, @"[\r\n]+", string.Empty);

        private string FormatLines(string[] lines, string indentation)
        {
            if (lines == null || lines.Length == 0)
            {
                return string.Empty;
            }
                
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.Append(Environment.NewLine);
            
            for (int i = 0; i < lines.Length; i++)
            {
                sb.Append(indentation);
                sb.Append(lines[i]);
                
                if (i < lines.Length - 1)
                {
                    sb.Append(Environment.NewLine);
                }
            }
            
            return sb.ToString();
        }
    }
}
