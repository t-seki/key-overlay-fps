namespace KeyOverlayFPS.Initialization
{
    /// <summary>
    /// MainWindow初期化ステップのインターフェース
    /// </summary>
    public interface IInitializationStep
    {
        /// <summary>
        /// ステップ名
        /// </summary>
        string Name { get; }
        
        /// <summary>
        /// 初期化ステップを実行
        /// </summary>
        /// <param name="window">初期化対象のMainWindow</param>
        void Execute(MainWindow window);
    }
}