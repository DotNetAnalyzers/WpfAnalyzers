namespace WpfAnalyzers.Test
{
    using System.Collections.Generic;
    using System.Linq;
    using NUnit.Framework;

    public class EnumarebleExtTests
    {
        internal class WhenSourceIsEnumerable
        {
            [Test]
            public void TryFirst()
            {
                Assert.AreEqual(true, Enumerable.Range(1, 3).TryFirst(out var result));
                Assert.AreEqual(1, result);
            }

            [Test]
            public void TryFirstFailsWhenEmpty()
            {
                Assert.AreEqual(false, Enumerable.Empty<int>().TryFirst(out var result));
                Assert.AreEqual(0, result);
            }

            [Test]
            public void TryFirstFailsWhenNull()
            {
                Assert.AreEqual(false, ((IEnumerable<int>)null).TryFirst(out var result));
                Assert.AreEqual(0, result);
            }

            [Test]
            public void TryFirstWithPredicate()
            {
                Assert.AreEqual(true, Enumerable.Range(1, 3).TryFirst(x => x == 2, out var result));
                Assert.AreEqual(2, result);
            }

            [Test]
            public void TryFirstWithPredicateFailsWhenEmpty()
            {
                Assert.AreEqual(false, Enumerable.Empty<int>().TryFirst(x => x == 2, out var result));
                Assert.AreEqual(0, result);
            }

            [Test]
            public void TryFirstWithPredicateFailsWhenNull()
            {
                Assert.AreEqual(false, ((IEnumerable<int>)null).TryFirst(x => x == 2, out var result));
                Assert.AreEqual(0, result);
            }

            [Test]
            public void TrySingle()
            {
                Assert.AreEqual(true, Enumerable.Range(1, 1).TrySingle(out var result));
                Assert.AreEqual(1, result);
            }

            [Test]
            public void TrySingleFailsWhenMoreThanOne()
            {
                Assert.AreEqual(false, Enumerable.Range(0, 3).TrySingle(out var result));
                Assert.AreEqual(0, result);
            }

            [Test]
            public void TrySingleFailsWhenEmpty()
            {
                Assert.AreEqual(false, Enumerable.Empty<int>().TrySingle(out var result));
                Assert.AreEqual(0, result);
            }

            [Test]
            public void TrySingleFailsWhenNull()
            {
                Assert.AreEqual(false, ((IEnumerable<int>)null).TrySingle(out var result));
                Assert.AreEqual(0, result);
            }

            [Test]
            public void TrySingleWithPredicate()
            {
                Assert.AreEqual(true, Enumerable.Range(1, 5).TrySingle(x => x == 2, out var result));
                Assert.AreEqual(2, result);
            }

            [Test]
            public void TrySingleWithPredicateFailsWhenMoreThanOne()
            {
                Assert.AreEqual(false, Enumerable.Repeat(2, 3).TrySingle(x => x == 2, out var result));
                Assert.AreEqual(0, result);
            }

            [Test]
            public void TrySingleWithPredicateFailsWhenEmpty()
            {
                Assert.AreEqual(false, Enumerable.Empty<int>().TrySingle(x => x == 2, out var result));
                Assert.AreEqual(0, result);
            }

            [Test]
            public void TrySingleWithPredicateFailsWhenNull()
            {
                Assert.AreEqual(false, ((IEnumerable<int>)null).TrySingle(out var result));
                Assert.AreEqual(0, result);
            }

            [TestCase(0, 1)]
            [TestCase(1, 2)]
            [TestCase(2, 3)]
            public void TryFirst(int index, int expected)
            {
                Assert.AreEqual(true, Enumerable.Range(1, 3).TryElementAt(index, out var result));
                Assert.AreEqual(expected, result);
            }

            [TestCase(5)]
            public void TryFirstFailsWhenOtOfBounds(int index)
            {
                Assert.AreEqual(false, Enumerable.Range(1, 3).TryElementAt(index, out var result));
                Assert.AreEqual(0, result);
            }

            [Test]
            public void TryElementAtFailsWhenEmpty()
            {
                Assert.AreEqual(false, Enumerable.Empty<int>().TryElementAt(0, out var result));
                Assert.AreEqual(0, result);
            }

            [Test]
            public void TryElementAtFailsWhenNull()
            {
                Assert.AreEqual(false, ((IEnumerable<int>)null).TryElementAt(0, out var result));
                Assert.AreEqual(0, result);
            }
        }
    }
}