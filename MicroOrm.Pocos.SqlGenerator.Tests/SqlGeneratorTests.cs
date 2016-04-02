using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MicroOrm.Pocos.SqlGenerator.Attributes;

namespace MicroOrm.Pocos.SqlGenerator.Tests
{
    [TestClass]
    public class SqlGeneratorTests
    {
        [TestMethod]
        public void GetWhere_TestForNulls()
        {
            var sqlGenerator = new SqlGenerator<MyObject>();

            var sql = sqlGenerator.GetSelect(new { Disabled = (DateTime?)null });

            Assert.IsTrue(sql.Contains("WHERE [MyTable].[Disabled] IS NULL"), sql);
        }

        [TestMethod]
        public void GetWhere_TestForValueInNullableColumn()
        {
            var sqlGenerator = new SqlGenerator<MyObject>();

            var sql = sqlGenerator.GetSelect(new { Disabled = new DateTime(2016,02,04,0,0,0) });

            Assert.IsTrue(sql.Contains("WHERE [MyTable].[Disabled] = @Disabled"), sql);
        }
    }

    [StoredAs("MyTable")]
    public class MyObject
    {
        [KeyProperty]
        public int MyObjectId { get; set; }

        public string Description { get; set; }

        public DateTime? Disabled { get; set; }
    }
}
