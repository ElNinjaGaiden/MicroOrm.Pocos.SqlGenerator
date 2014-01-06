using System;

namespace MicroOrm.Pocos.SqlGenerator.Attributes
{
    /// <summary>
    /// 
    /// </summary>
    public class StoredAs : Attribute
    {
        /// <summary>
        /// 
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        public StoredAs(string value)
        {
            this.Value = value;
        }
    }
}
