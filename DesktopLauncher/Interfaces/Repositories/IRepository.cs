using System.Collections.Generic;

namespace DesktopLauncher.Interfaces.Repositories
{
    /// <summary>
    /// 汎用リポジトリインターフェース
    /// </summary>
    /// <typeparam name="T">エンティティ型</typeparam>
    public interface IRepository<T> where T : class
    {
        /// <summary>
        /// 全件取得
        /// </summary>
        IEnumerable<T> GetAll();

        /// <summary>
        /// IDで取得
        /// </summary>
        T? GetById(string id);

        /// <summary>
        /// 追加
        /// </summary>
        void Add(T entity);

        /// <summary>
        /// 更新
        /// </summary>
        void Update(T entity);

        /// <summary>
        /// 削除
        /// </summary>
        void Delete(string id);

        /// <summary>
        /// 変更を保存
        /// </summary>
        void Save();
    }
}
