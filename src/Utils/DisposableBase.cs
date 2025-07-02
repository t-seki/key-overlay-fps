using System;

namespace KeyOverlayFPS.Utils
{
    /// <summary>
    /// IDisposableパターンの基底クラス
    /// 標準的なDispose実装の重複を解消
    /// </summary>
    public abstract class DisposableBase : IDisposable
    {
        private bool _disposed = false;

        /// <summary>
        /// リソースが既に解放されているかどうか
        /// </summary>
        protected bool IsDisposed => _disposed;

        /// <summary>
        /// リソースを解放
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// リソースを解放（実装必須）
        /// </summary>
        /// <param name="disposing">マネージリソースを解放するかどうか</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // 派生クラスでのマネージリソース解放
                    DisposeManagedResources();
                }

                // 派生クラスでのアンマネージリソース解放
                DisposeUnmanagedResources();

                _disposed = true;
            }
        }

        /// <summary>
        /// マネージリソースの解放（派生クラスでオーバーライド）
        /// </summary>
        protected virtual void DisposeManagedResources()
        {
            // 派生クラスで実装
        }

        /// <summary>
        /// アンマネージリソースの解放（派生クラスでオーバーライド）
        /// </summary>
        protected virtual void DisposeUnmanagedResources()
        {
            // 派生クラスで実装
        }

        /// <summary>
        /// ファイナライザ
        /// </summary>
        ~DisposableBase()
        {
            Dispose(false);
        }

        /// <summary>
        /// Disposeされているかをチェックし、されていれば例外をスロー
        /// </summary>
        protected void ThrowIfDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(GetType().Name);
            }
        }
    }
}