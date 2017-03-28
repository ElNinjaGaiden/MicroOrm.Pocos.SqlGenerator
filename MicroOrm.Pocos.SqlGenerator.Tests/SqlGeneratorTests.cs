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

        [TestMethod]
        public void GetWhere_Limit1Result()
        {
            var sqlGenerator = new SqlGenerator<MyObject>();

            var sql = sqlGenerator.GetSelect(new { Disabled = new DateTime(2016, 02, 04, 0, 0, 0) }, 1);

            Assert.IsTrue(sql.StartsWith("SELECT TOP 1 "), sql);
        }

        [TestMethod]
        public void GetWhere_NoLimitResult()
        {
            var sqlGenerator = new SqlGenerator<MyObject>();

            var sql = sqlGenerator.GetSelect(new { Disabled = new DateTime(2016, 02, 04, 0, 0, 0) });

            Assert.IsTrue(sql.StartsWith("SELECT [MyTable].[MyObjectId]"), sql);
        }

        [TestMethod]
        public void GetWhere_TestIEnumerableFilter()
        {
            var sqlGenerator = new SqlGenerator<MyObject>();
            var sql = sqlGenerator.GetSelect(new { Description = new[] { "a", "b", "c" } });

            Assert.IsTrue(sql.Contains("WHERE [MyTable].[Description] IN @Description"));
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
