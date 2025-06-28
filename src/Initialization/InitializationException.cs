using System;

namespace KeyOverlayFPS.Initialization
{
    /// <summary>
    /// 初期化プロセスで発生する例外
    /// </summary>
    public class InitializationException : Exception
    {
        /// <summary>
        /// 失敗したステップ名
        /// </summary>
        public string StepName { get; }
        
        public InitializationException(string stepName, Exception innerException) 
            : base($"初期化ステップ '{stepName}' でエラーが発生しました", innerException)
        {
            StepName = stepName;
        }
        
        public InitializationException(string stepName, string message) 
            : base($"初期化ステップ '{stepName}': {message}")
        {
            StepName = stepName;
        }
    }
}