using System;
using NetworkTables.Exceptions;
using NUnit.Framework;

namespace NetworkTables.Test.Exceptions
{
    [TestFixture]
    public class TableKeyNotDefinedTest
    {
        [Test]
        public void NotDefinedTest()
        {
            string key = "Test";
            TableKeyNotDefinedException ex = new TableKeyNotDefinedException(key);
            Assert.That(ex.Key, Is.EqualTo(key));
            Assert.That(ex.Message, Is.EqualTo($"Unknown Table Key: {key}"));
            Assert.That(ex is InvalidOperationException);
        }
    }
}
