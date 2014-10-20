using System.Collections.Generic;
using System.Reflection;

namespace MicroOrm.Pocos.SqlGenerator
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    public interface ISqlGenerator<TEntity> where TEntity : new()
    {

        #region Properties

        /// <summary>
        /// 
        /// </summary>
        bool IsIdentity { get; }

        /// <summary>
        /// 
        /// </summary>
        PropertyMetadata IdentityProperty { get; }

        /// <summary>
        /// 
        /// </summary>
        string TableName { get; }

        /// <summary>
        /// 
        /// </summary>
        string Scheme { get; }

        /// <summary>
        /// 
        /// </summary>
        IEnumerable<PropertyMetadata> KeyProperties { get; }

        /// <summary>
        /// 
        /// </summary>
        IEnumerable<PropertyMetadata> BaseProperties { get; }

        /// <summary>
        /// 
        /// </summary>
        PropertyMetadata StatusProperty { get; }

        /// <summary>
        /// 
        /// </summary>
        object LogicalDeleteValue { get; }

        /// <summary>
        /// 
        /// </summary>
        bool LogicalDelete { get; }

        #endregion

        #region Functions

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        string GetSelectAll();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="filters"></param>
        /// <returns></returns>
        string GetSelect(object filters);

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        string GetInsert();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        string GetUpdate();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        string GetDelete();

        #endregion
    }
}
