using NUnit.Framework;

namespace NetworkTables.Test
{
    [TestFixture(true)]
    [TestFixture(false)]
    public class StorageTestPopulateOne : StorageBaseTest
    {
        public StorageTestPopulateOne(bool server)
        {
            m_server = server;
        }

        [SetUp]
        public void Clear()
        {
            tmpEntry = new Storage.Entry("foobar");
            storage?.Dispose();
            storage = new Storage();
            outgoing.Clear();

            HookOutgoing();

            storage.SetEntryTypeValue("foo", Value.MakeBoolean(true));
            outgoing.Clear();
        }

        [Test]
        public void SetDefaultEntryExistsSameType()
        {
            var value = Value.MakeBoolean(true);
            var retVal = storage.SetDefaultEntryValue("foo", value);
            Assert.That(retVal, Is.True);
            Assert.That(!ReferenceEquals(value, GetEntry("foo").Value));

            Assert.That(outgoing, Is.Empty);
        }

        [Test]
        public void SetDefaultEntryExistsDifferentType()
        {
            // existing entry is boolean
            var value = Value.MakeDouble(2.0);
            var retVal = storage.SetDefaultEntryValue("foo", value);
            Assert.That(retVal, Is.False);
            // should not have updated value in table if it already existed
            Assert.That(value, Is.Not.EqualTo(GetEntry("foo").Value));

            Assert.That(outgoing, Is.Empty);
        }

        [Test]
        public void SetEntryTypeValueAssignChangeType()
        {
            var value = Value.MakeDouble(0.0);
            storage.SetEntryTypeValue("foo", value);
            Assert.That(value, Is.EqualTo(GetEntry("foo").Value));

            Assert.That(outgoing, Has.Count.EqualTo(1u));
            Assert.That(outgoing[0].only, Is.Null);
            Assert.That(outgoing[0].except, Is.Null);

            var msg = outgoing[0].msg;

            Assert.That(msg.Type, Is.EqualTo(Message.MsgType.EntryAssign));

            Assert.That(msg.Str, Is.EqualTo("foo"));

            if (m_server)
            {
                Assert.That(msg.Id, Is.EqualTo(0u));
            }
            else
            {
                Assert.That(msg.Id, Is.EqualTo(0xffffu));
            }

            Assert.That(msg.SeqNumUid, Is.EqualTo(2u));
            Assert.That(msg.Val, Is.EqualTo(value));
            Assert.That(msg.Flags, Is.EqualTo(0u));
        }

        [Test]
        public void SetEntryTypeEqualValue()
        {
            var value = Value.MakeBoolean(true);
            storage.SetEntryTypeValue("foo", value);
            Assert.That(value, Is.EqualTo(GetEntry("foo").Value));
            Assert.That(outgoing, Is.Empty);
        }

        [Test]
        public void SetEntryValueAssignTypeChange()
        {
            //Update with different type reuslts in error and no message
            var value = Value.MakeDouble(0.0);
            Assert.That(storage.SetEntryValue("foo", value), Is.False);
            var entry = GetEntry("foo");
            Assert.That(entry.Value, Is.Not.EqualTo(value));
            Assert.That(outgoing, Is.Empty);
        }

        [Test]
        public void SetEntryFlagsEqualValue()
        {
            //update with same value, no update message is issued
            storage.SetEntryFlags("foo", 0);
            var entry = GetEntry("foo");
            Assert.That(entry.Flags, Is.EqualTo(EntryFlags.None));
            Assert.That(outgoing, Is.Empty);
        }

        [Test]
        public void GetEntryFlagsExist()
        {
            storage.SetEntryFlags("foo", EntryFlags.Persistent);
            outgoing.Clear();
            Assert.That(storage.GetEntryFlags("foo"), Is.EqualTo(EntryFlags.Persistent));
            Assert.That(outgoing, Is.Empty);
        }
    }
}
