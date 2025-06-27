using System;
using System.Windows;

namespace KeyOverlayFPS
{
    /// <summary>
    /// 動的レイアウトシステムの独立テスト用プログラム
    /// </summary>
    public static class TestDynamicLayoutProgram
    {
        /// <summary>
        /// 動的レイアウトテストウィンドウを起動
        /// </summary>
        public static void RunTest()
        {
            try
            {
                var testWindow = new TestDynamicLayoutWindow();
                testWindow.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"テストプログラムでエラーが発生しました: {ex.Message}\n\nスタックトレース:\n{ex.StackTrace}", 
                              "プログラムエラー", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}