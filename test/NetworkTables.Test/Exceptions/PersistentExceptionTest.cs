using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetworkTables.Exceptions;
using NUnit.Framework;

namespace NetworkTables.Test.Exceptions
{
    [TestFixture]
    public class PersistentExceptionTest
    {
        [Test]
        public void Persistent()
        {
            const string message = "Test Error Message";
            PersistentException ex = new PersistentException(message);
            Assert.That(ex.Message, Is.EqualTo(message));
            Assert.That(ex is IOException);
        }
    }
}
