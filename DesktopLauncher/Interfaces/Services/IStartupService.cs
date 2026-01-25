namespace DesktopLauncher.Interfaces.Services
{
    /// <summary>
    /// スタートアップ登録サービスのインターフェース
    /// </summary>
    public interface IStartupService
    {
        /// <summary>
        /// スタートアップが有効かどうか
        /// </summary>
        bool IsEnabled { get; }

        /// <summary>
        /// スタートアップの有効/無効を設定する
        /// </summary>
        /// <param name="enable">有効にする場合はtrue</param>
        void SetEnabled(bool enable);
    }
}
