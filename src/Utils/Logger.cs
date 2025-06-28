using System;
using System.IO;

namespace KeyOverlayFPS.Utils
{
    /// <summary>
    /// デバッグログ出力クラス
    /// </summary>
    public static class Logger
    {
        private static readonly string LogFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "debug.log");
        private static readonly object LogLock = new object();

        /// <summary>
        /// ログファイルを初期化（既存ファイルを削除）
        /// </summary>
        public static void Initialize()
        {
            try
            {
                lock (LogLock)
                {
                    if (File.Exists(LogFilePath))
                    {
                        File.Delete(LogFilePath);
                    }
                    WriteLog("INFO", "ログシステム初期化完了");
                    WriteLog("INFO", $"LogFilePath: {LogFilePath}");
                }
            }
            catch (Exception ex)
            {
                // ログ初期化エラーは無視（ファイルアクセス権限等の問題）
                System.Diagnostics.Debug.WriteLine($"ログ初期化エラー: {ex.Message}");
            }
        }

        /// <summary>
        /// 情報ログ出力
        /// </summary>
        public static void Info(string message)
        {
            WriteLog("INFO", message);
        }

        /// <summary>
        /// エラーログ出力
        /// </summary>
        public static void Error(string message, Exception? exception = null)
        {
            var fullMessage = exception != null 
                ? $"{message} - Exception: {exception.GetType().Name}: {exception.Message}\n{exception.StackTrace}"
                : message;
            WriteLog("ERROR", fullMessage);
        }

        /// <summary>
        /// 警告ログ出力
        /// </summary>
        public static void Warning(string message)
        {
            WriteLog("WARN", message);
        }

        /// <summary>
        /// デバッグログ出力
        /// </summary>
        public static void Debug(string message)
        {
            WriteLog("DEBUG", message);
        }

        /// <summary>
        /// 実際のログ書き込み処理
        /// </summary>
        private static void WriteLog(string level, string message)
        {
            try
            {
                lock (LogLock)
                {
                    var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                    var logEntry = $"[{timestamp}] [{level}] {message}";
                    
                    File.AppendAllText(LogFilePath, logEntry + Environment.NewLine);
                    
                    // デバッグ出力にも同時出力
                    System.Diagnostics.Debug.WriteLine(logEntry);
                }
            }
            catch (Exception ex)
            {
                // ログ出力エラーは無視（無限ループを防ぐ）
                System.Diagnostics.Debug.WriteLine($"ログ出力エラー: {ex.Message}");
            }
        }
    }
}