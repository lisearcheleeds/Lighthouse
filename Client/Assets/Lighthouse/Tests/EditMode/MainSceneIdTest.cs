using NUnit.Framework;
using Lighthouse.Scene;

namespace Lighthouse.Tests.EditMode
{
    public class MainSceneIdTest
    {
        [Test]
        public void Equals_SameId_ReturnsTrue()
        {
            var a = new MainSceneId(1, "Scene");
            var b = new MainSceneId(1, "Different");
            Assert.IsTrue(a.Equals(b));
        }

        [Test]
        public void Equals_DifferentId_ReturnsFalse()
        {
            var a = new MainSceneId(1, "Scene");
            var b = new MainSceneId(2, "Scene");
            Assert.IsFalse(a.Equals(b));
        }

        [Test]
        public void Equals_Null_ReturnsFalse()
        {
            var a = new MainSceneId(1, "Scene");
            Assert.IsFalse(a.Equals(null));
        }

        [Test]
        public void EqualityOperator_SameId_ReturnsTrue()
        {
            var a = new MainSceneId(1, "Scene");
            var b = new MainSceneId(1, "Other");
            Assert.IsTrue(a == b);
        }

        [Test]
        public void InequalityOperator_DifferentId_ReturnsTrue()
        {
            var a = new MainSceneId(1, "Scene");
            var b = new MainSceneId(2, "Scene");
            Assert.IsTrue(a != b);
        }

        [Test]
        public void EqualityOperator_BothNull_ReturnsTrue()
        {
            MainSceneId a = null;
            MainSceneId b = null;
            Assert.IsTrue(a == b);
        }

        [Test]
        public void EqualityOperator_OneNull_ReturnsFalse()
        {
            var a = new MainSceneId(1, "Scene");
            Assert.IsFalse(a == null);
            Assert.IsFalse(null == a);
        }

        [Test]
        public void GetHashCode_SameId_ReturnsSameHash()
        {
            var a = new MainSceneId(5, "A");
            var b = new MainSceneId(5, "B");
            Assert.AreEqual(a.GetHashCode(), b.GetHashCode());
        }

        [Test]
        public void ToString_WithName_IncludesNameAndId()
        {
            var id = new MainSceneId(3, "Home");
            Assert.AreEqual("Home(3)", id.ToString());
        }

        [Test]
        public void ToString_EmptyName_ReturnsIdOnly()
        {
            var id = new MainSceneId(3, "");
            Assert.AreEqual("3", id.ToString());
        }
    }
}
