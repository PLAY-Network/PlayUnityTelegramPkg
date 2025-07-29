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

        private const string TELEGRAM_SCRIPT_URL = "https://telegram.org/js/telegram-web-app.js";
        private const string TELEGRAM_EVENT_HANDLER_ID = "telegram-event-handlers";

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
            
            bool hasTelegramScript = Regex.IsMatch(indexContent, @"<script[^>]*src\s*=\s*[""']" + Regex.Escape(TELEGRAM_SCRIPT_URL) + @"[""'][^>]*>");
            bool hasTelegramEventHandlers = indexContent.Contains(TELEGRAM_EVENT_HANDLER_ID);
            
            Match bodyOpenMatch = Regex.Match(indexContent, "<body[^>]*>");
            Match bodyCloseMatch = Regex.Match(indexContent, @"(\s*)</body>");
            
            if (!bodyOpenMatch.Success || !bodyCloseMatch.Success)
            {
                Debug.LogError("Failed to find body tags in index.html");
                return;
            }
            
            string modifiedContent = indexContent;
            
            if (!hasTelegramScript)
            {
                int insertPos = bodyOpenMatch.Index + bodyOpenMatch.Length;
                string indent = GetIndentation(indexContent, bodyOpenMatch.Index) + "  ";
                string[] codeLines = {
                    "<!-- Telegram Web App Script -->",
                    $"<script src=\"{TELEGRAM_SCRIPT_URL}\"></script>"
                };
                string codeToInsert = FormatLines(codeLines, indent);
                modifiedContent = modifiedContent.Insert(insertPos, codeToInsert);
                bodyCloseMatch = Regex.Match(modifiedContent, @"(\s*)</body>");
            }
            
            if (!hasTelegramEventHandlers)
            {
                int insertPos = bodyCloseMatch.Index;
                string indent = bodyCloseMatch.Groups[1].Value + "  ";
                string[] codeLines = {
                    "<!-- Telegram Event Handlers -->",
                    $"<script id=\"{TELEGRAM_EVENT_HANDLER_ID}\">",
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
                string handlersToInsert = FormatLines(codeLines, indent);
                modifiedContent = modifiedContent.Insert(insertPos, handlersToInsert);
            }
            
            if (modifiedContent != indexContent)
            {
                System.IO.File.WriteAllText(indexPath, modifiedContent);
                Debug.Log("Successfully updated index.html with Telegram integration");
            }
            else
            {
                Debug.Log("Telegram script and event handlers are already present in index.html");
            }
        }
        
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
        
        private string GetIndentation(string content, int position)
        {
            int lineStart = content.LastIndexOf('\n', position);
            if (lineStart < 0)
            {
                lineStart = 0;
            }
            else
            {
                lineStart++;
            }
            
            int lineEnd = content.IndexOf('\n', lineStart);
            if (lineEnd < 0)
            {
                lineEnd = content.Length;
            }
            
            string line = content.Substring(lineStart, Math.Min(position - lineStart, lineEnd - lineStart));
            Match indentMatch = Regex.Match(line, @"^\s*");
            
            return indentMatch.Value;
        }
    }
}
