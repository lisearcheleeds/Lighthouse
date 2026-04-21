using NUnit.Framework;
using Lighthouse.Scene;

namespace Lighthouse.Tests.EditMode
{
    public class ModuleSceneIdTest
    {
        [Test]
        public void Equals_SameId_ReturnsTrue()
        {
            var a = new ModuleSceneId(1, "Module");
            var b = new ModuleSceneId(1, "Different");
            Assert.IsTrue(a.Equals(b));
        }

        [Test]
        public void Equals_DifferentId_ReturnsFalse()
        {
            var a = new ModuleSceneId(1, "Module");
            var b = new ModuleSceneId(2, "Module");
            Assert.IsFalse(a.Equals(b));
        }

        [Test]
        public void Equals_Null_ReturnsFalse()
        {
            var a = new ModuleSceneId(1, "Module");
            Assert.IsFalse(a.Equals(null));
        }

        [Test]
        public void EqualityOperator_SameId_ReturnsTrue()
        {
            var a = new ModuleSceneId(1, "Module");
            var b = new ModuleSceneId(1, "Other");
            Assert.IsTrue(a == b);
        }

        [Test]
        public void InequalityOperator_DifferentId_ReturnsTrue()
        {
            var a = new ModuleSceneId(1, "Module");
            var b = new ModuleSceneId(2, "Module");
            Assert.IsTrue(a != b);
        }

        [Test]
        public void EqualityOperator_BothNull_ReturnsTrue()
        {
            ModuleSceneId a = null;
            ModuleSceneId b = null;
            Assert.IsTrue(a == b);
        }

        [Test]
        public void EqualityOperator_OneNull_ReturnsFalse()
        {
            var a = new ModuleSceneId(1, "Module");
            Assert.IsFalse(a == null);
            Assert.IsFalse(null == a);
        }

        [Test]
        public void GetHashCode_SameId_ReturnsSameHash()
        {
            var a = new ModuleSceneId(7, "A");
            var b = new ModuleSceneId(7, "B");
            Assert.AreEqual(a.GetHashCode(), b.GetHashCode());
        }

        [Test]
        public void ToString_WithName_IncludesNameAndId()
        {
            var id = new ModuleSceneId(2, "Header");
            Assert.AreEqual("Header(2)", id.ToString());
        }

        [Test]
        public void ToString_EmptyName_ReturnsIdOnly()
        {
            var id = new ModuleSceneId(2, "");
            Assert.AreEqual("2", id.ToString());
        }
    }
}
