namespace DesktopLauncher.Interfaces.Services.Ui
{
    /// <summary>
    /// ダイアログサービスのインターフェース
    /// </summary>
    public interface IDialogService
    {
        /// <summary>
        /// ファイル選択ダイアログを表示する
        /// </summary>
        /// <param name="filter">ファイルフィルタ</param>
        /// <returns>選択されたファイルパス（キャンセル時はnull）</returns>
        string? ShowOpenFileDialog(string filter = "すべてのファイル|*.*");

        /// <summary>
        /// フォルダ選択ダイアログを表示する
        /// </summary>
        /// <returns>選択されたフォルダパス（キャンセル時はnull）</returns>
        string? ShowFolderBrowserDialog();

        /// <summary>
        /// ファイルまたはフォルダ選択ダイアログを表示する
        /// </summary>
        /// <param name="initialDirectory">初期ディレクトリ（省略可能）</param>
        /// <returns>選択されたパス（キャンセル時はnull）</returns>
        string? ShowFileOrFolderDialog(string? initialDirectory = null);

        /// <summary>
        /// 画像選択ダイアログを表示する
        /// </summary>
        /// <returns>選択された画像ファイルパス（キャンセル時はnull）</returns>
        string? ShowImageFileDialog();

        /// <summary>
        /// 確認ダイアログを表示する
        /// </summary>
        /// <param name="message">メッセージ</param>
        /// <param name="title">タイトル</param>
        /// <returns>はいが選択されたかどうか</returns>
        bool ShowConfirmDialog(string message, string title = "確認");

        /// <summary>
        /// メッセージダイアログを表示する
        /// </summary>
        /// <param name="message">メッセージ</param>
        /// <param name="title">タイトル</param>
        void ShowMessage(string message, string title = "情報");

        /// <summary>
        /// エラーダイアログを表示する
        /// </summary>
        /// <param name="message">メッセージ</param>
        /// <param name="title">タイトル</param>
        void ShowError(string message, string title = "エラー");

        /// <summary>
        /// テキスト入力ダイアログを表示する
        /// </summary>
        /// <param name="prompt">プロンプトメッセージ</param>
        /// <param name="title">タイトル</param>
        /// <param name="defaultValue">デフォルト値</param>
        /// <returns>入力されたテキスト（キャンセル時はnull）</returns>
        string? ShowInputDialog(string prompt, string title = "入力", string defaultValue = "");

        string? ShowOpenFileDialog(string title, string filter);

        string? ShowSaveFileDialog(string title, string filter, string defaultFileName = "");
    }
}
