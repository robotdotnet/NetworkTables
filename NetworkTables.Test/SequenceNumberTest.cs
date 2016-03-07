using NUnit.Framework;

namespace NetworkTables.Test
{
    [TestFixture]
    public class SequenceNumberTest
    {
        [Test]
        public void ConstructDefault()
        {
            SequenceNumber num = default(SequenceNumber);
            Assert.That(num.Value, Is.EqualTo(0));
        }

        [Test]
        public void ConstructNew()
        {
            SequenceNumber num = new SequenceNumber();
            Assert.That(num.Value, Is.EqualTo(0));
        }

        [Test]
        public void ConstructNewSetValue()
        {
            SequenceNumber num = new SequenceNumber(58);
            Assert.That(num.Value, Is.EqualTo(58));
        }

        [Test]
        public void ConstructCopy()
        {
            SequenceNumber num = new SequenceNumber(58);
            Assert.That(num.Value, Is.EqualTo(58));
            SequenceNumber num2 = new SequenceNumber(num);
            Assert.That(num.Value, Is.EqualTo(58));
        }

        [Test]
        public void OperatorPlusPlusNonOverflow()
        {
            SequenceNumber num = new SequenceNumber(58);
            Assert.That(num.Value, Is.EqualTo(58));
            num++;
            Assert.That(num.Value, Is.EqualTo(59));
        }

        [Test]
        public void OperatorPlusPlusOverflow()
        {
            SequenceNumber num = new SequenceNumber(0xFFFF);
            Assert.That(num.Value, Is.EqualTo(0xFFFF));
            num++;
            Assert.That(num.Value, Is.EqualTo(0));
        }

        [Test]
        public void OperatorEquals()
        {
            SequenceNumber num = new SequenceNumber(58);
            SequenceNumber num2 = new SequenceNumber(58);
            Assert.That(num == num2);
        }

        [Test]
        public void OperatorNotEquals()
        {
            SequenceNumber num = new SequenceNumber(58);
            SequenceNumber num2 = new SequenceNumber(59);
            Assert.That(num != num2);
        }

        [Test]
        public void OperatorLessThenOrEqual()
        {
            SequenceNumber num = new SequenceNumber(58);
            SequenceNumber num2 = new SequenceNumber(59);
            Assert.That(num <= num2);
            num++;
            Assert.That(num <= num2);
            num++;
            Assert.That(num <= num2, Is.False);
        }


        [Test]
        public void OperatorGreaterThenOrEqual()
        {
            SequenceNumber num = new SequenceNumber(58);
            SequenceNumber num2 = new SequenceNumber(59);
            Assert.That(num2 >= num);
            num++;
            Assert.That(num2 >= num);
            num++;
            Assert.That(num2 >= num, Is.False);
        }

        [Test]
        public void OperatorLessThenNotWrapAround()
        {
            SequenceNumber num = new SequenceNumber(58);
            SequenceNumber num2 = new SequenceNumber(59);
            Assert.That(num < num2);
            num++;
            Assert.That(num < num2, Is.False);
            num++;
            Assert.That(num < num2, Is.False);
        }

        [Test]
        public void OperatorGreaterThenNotWrapAround()
        {
            SequenceNumber num = new SequenceNumber(58);
            SequenceNumber num2 = new SequenceNumber(59);
            Assert.That(num2 > num);
            num++;
            Assert.That(num2 > num, Is.False);
            num++;
            Assert.That(num2 > num, Is.False);
        }

        [Test]
        public void ObjectEquals()
        {
            SequenceNumber num = new SequenceNumber(58);
            SequenceNumber num2 = new SequenceNumber(58);
            SequenceNumber num3 = new SequenceNumber(59);

            object oNum = num;

            Assert.That(num.Equals(num2));
            Assert.That(num.Equals(num));
            Assert.That(oNum.Equals(oNum));
            Assert.That(num.Equals(null), Is.False);
            Assert.That(num.Equals(num3), Is.False);
            Assert.That(num.Equals(new object()), Is.False);

        }
    }
}
