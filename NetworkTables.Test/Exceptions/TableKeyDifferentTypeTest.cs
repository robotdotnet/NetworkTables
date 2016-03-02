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
    public class TableKeyDifferentTypeTest
    {
        [Test]
        public void TableKeyDifferentTypeNoKey()
        {
            TableKeyDifferentTypeException ex = new TableKeyDifferentTypeException(NtType.Boolean, NtType.Double);
            Assert.That(ex.RequestedKey, Is.EqualTo(string.Empty));
            Assert.That(ex.RequestedType, Is.EqualTo(NtType.Boolean));
            Assert.That(ex.TypeInTable, Is.EqualTo(NtType.Double));
            Assert.That(ex.Message, Is.EqualTo($"Requested Type {NtType.Boolean} does not match actual Type {NtType.Double}."));
            Assert.That(ex.ThrownByValueGet, Is.True);
            Assert.That(ex is InvalidOperationException);
        }

        [Test]
        public void TableKeyDifferentTypeKey()
        {
            const string key = "Test";
            TableKeyDifferentTypeException ex = new TableKeyDifferentTypeException(key, NtType.Boolean, NtType.Double);
            Assert.That(ex.RequestedKey, Is.EqualTo(key));
            Assert.That(ex.RequestedType, Is.EqualTo(NtType.Boolean));
            Assert.That(ex.TypeInTable, Is.EqualTo(NtType.Double));
            Assert.That(ex.Message, Is.EqualTo($"Key: {key}, Requested Type: {NtType.Boolean}, Type in Table: {NtType.Double}"));
            Assert.That(ex.ThrownByValueGet, Is.False);
            Assert.That(ex is InvalidOperationException);
        }
    }
}
