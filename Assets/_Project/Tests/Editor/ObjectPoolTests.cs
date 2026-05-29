using NUnit.Framework;
using UnityEngine;
using AnimalMagicRoyale.Core;

namespace AnimalMagicRoyale.Tests
{
    public class ObjectPoolTests
    {
        private GameObject prefabGO;
        private DummyComponent prefab;
        private ObjectPool<DummyComponent> pool;

        private class DummyComponent : MonoBehaviour { }

        [SetUp]
        public void SetUp()
        {
            prefabGO = new GameObject("Prefab");
            prefab = prefabGO.AddComponent<DummyComponent>();
            
            pool = new ObjectPool<DummyComponent>(prefab, 2);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(prefabGO);
        }

        [Test]
        public void Get_ReturnsActiveObject()
        {
            var obj = pool.Get();
            Assert.IsNotNull(obj);
            Assert.IsTrue(obj.gameObject.activeSelf);
        }

        [Test]
        public void Release_DeactivatesObject()
        {
            var obj = pool.Get();
            pool.Release(obj);
            Assert.IsFalse(obj.gameObject.activeSelf);
        }
    }
}
