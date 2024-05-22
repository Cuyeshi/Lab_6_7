using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using ClassLibraryForOOPaP_6_7;
using System.Data.SqlClient;
using NUnit.Framework;

namespace SQLTests
{
    [TestClass]
    public class Tests
    {
        private readonly Repository<object> _repository; // Используем обобщенный тип

        public Tests()
        {
            _repository = new Repository<object>();
        }

        [TestMethod]
        public void GetAll_ReturnsNotNull()
        {
            // Arrange
            string query = "SELECT * FROM Doctors";

            // Act
            var result = _repository.GetAll(query, reader => reader);

            // Assert
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.IsNotNull(result);
        }

        [TestMethod]
        public void GetAll_WithInvalidQuery_ThrowsSqlException()
        {
            // Arrange
            string invalidQuery = "INVALID_QUERY";

            // Act and Assert
            NUnit.Framework.Assert.Throws<System.Data.SqlClient.SqlException>(() => _repository.GetAll(invalidQuery, reader => reader));
        }

        

        [TestMethod]
        public void ExecuteNonQuery_WithInvalidQuery_ThrowsSqlException()
        {
            // Arrange
            string invalidQuery = "INVALID_QUERY";

            // Act and Assert
            NUnit.Framework.Assert.Throws<SqlException>(() => _repository.ExecuteNonQuery(invalidQuery, command => { }));
        }
    }
}
