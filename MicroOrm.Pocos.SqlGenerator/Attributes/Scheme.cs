using System;

namespace MicroOrm.Pocos.SqlGenerator.Attributes
{
    /// <summary>
    /// 
    /// </summary>
    public class Scheme : Attribute
    {
        /// <summary>
        /// 
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        public Scheme(string value)
        {
            this.Value = value;
        }
    }
}
