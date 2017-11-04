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
            public void TryGetFirst()
            {
                Assert.AreEqual(true, Enumerable.Range(1, 3).TryGetFirst(out var result));
                Assert.AreEqual(1, result);
            }

            [Test]
            public void TryGetFirstFailsWhenEmpty()
            {
                Assert.AreEqual(false, Enumerable.Empty<int>().TryGetFirst(out var result));
                Assert.AreEqual(0, result);
            }

            [Test]
            public void TryGetFirstFailsWhenNull()
            {
                Assert.AreEqual(false, ((IEnumerable<int>)null).TryGetFirst(out var result));
                Assert.AreEqual(0, result);
            }

            [Test]
            public void TryGetFirstWithPredicate()
            {
                Assert.AreEqual(true, Enumerable.Range(1, 3).TryGetFirst(x => x == 2, out var result));
                Assert.AreEqual(2, result);
            }

            [Test]
            public void TryGetFirstWithPredicateFailsWhenEmpty()
            {
                Assert.AreEqual(false, Enumerable.Empty<int>().TryGetFirst(x => x == 2, out var result));
                Assert.AreEqual(0, result);
            }

            [Test]
            public void TryGetFirstWithPredicateFailsWhenNull()
            {
                Assert.AreEqual(false, ((IEnumerable<int>)null).TryGetFirst(x => x == 2, out var result));
                Assert.AreEqual(0, result);
            }

            [Test]
            public void TryGetSingle()
            {
                Assert.AreEqual(true, Enumerable.Range(1, 1).TryGetSingle(out var result));
                Assert.AreEqual(1, result);
            }

            [Test]
            public void TryGetSingleFailsWhenMoreThanOne()
            {
                Assert.AreEqual(false, Enumerable.Range(0, 3).TryGetSingle(out var result));
                Assert.AreEqual(0, result);
            }

            [Test]
            public void TryGetSingleFailsWhenEmpty()
            {
                Assert.AreEqual(false, Enumerable.Empty<int>().TryGetSingle(out var result));
                Assert.AreEqual(0, result);
            }

            [Test]
            public void TryGetSingleFailsWhenNull()
            {
                Assert.AreEqual(false, ((IEnumerable<int>)null).TryGetSingle(out var result));
                Assert.AreEqual(0, result);
            }

            [Test]
            public void TryGetSingleWithPredicate()
            {
                Assert.AreEqual(true, Enumerable.Range(1, 5).TryGetSingle(x => x == 2, out var result));
                Assert.AreEqual(2, result);
            }

            [Test]
            public void TryGetSingleWithPredicateFailsWhenMoreThanOne()
            {
                Assert.AreEqual(false, Enumerable.Repeat(2, 3).TryGetSingle(x => x == 2, out var result));
                Assert.AreEqual(0, result);
            }

            [Test]
            public void TryGetSingleWithPredicateFailsWhenEmpty()
            {
                Assert.AreEqual(false, Enumerable.Empty<int>().TryGetSingle(x => x == 2, out var result));
                Assert.AreEqual(0, result);
            }

            [Test]
            public void TryGetSingleWithPredicateFailsWhenNull()
            {
                Assert.AreEqual(false, ((IEnumerable<int>)null).TryGetSingle(out var result));
                Assert.AreEqual(0, result);
            }

            [TestCase(0, 1)]
            [TestCase(1, 2)]
            [TestCase(2, 3)]
            public void TryGetFirst(int index, int expected)
            {
                Assert.AreEqual(true, Enumerable.Range(1, 3).TryGetAtIndex(index, out var result));
                Assert.AreEqual(expected, result);
            }

            [TestCase(5)]
            public void TryGetFirstFailsWhenOtOfBounds(int index)
            {
                Assert.AreEqual(false, Enumerable.Range(1, 3).TryGetAtIndex(index, out var result));
                Assert.AreEqual(0, result);
            }

            [Test]
            public void TryGetAtIndexFailsWhenEmpty()
            {
                Assert.AreEqual(false, Enumerable.Empty<int>().TryGetAtIndex(0, out var result));
                Assert.AreEqual(0, result);
            }

            [Test]
            public void TryGetAtIndexFailsWhenNull()
            {
                Assert.AreEqual(false, ((IEnumerable<int>)null).TryGetAtIndex(0, out var result));
                Assert.AreEqual(0, result);
            }
        }
    }
}