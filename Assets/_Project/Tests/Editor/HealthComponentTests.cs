using NUnit.Framework;
using UnityEngine;
using AnimalMagicRoyale.Components;

namespace AnimalMagicRoyale.Tests
{
    public class HealthComponentTests
    {
        private GameObject go;
        private HealthComponent health;

        [SetUp]
        public void SetUp()
        {
            go = new GameObject("TestHealth");
            health = go.AddComponent<HealthComponent>();
            health.maxHealth = 100f;
            health.Awake();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(go);
        }

        [Test]
        public void TakeDamage_ReducesHealth()
        {
            health.TakeDamage(20f);
            Assert.AreEqual(80f, health.CurrentHealth);
        }

        [Test]
        public void TakeDamage_ClampsAtZero()
        {
            health.TakeDamage(150f);
            Assert.AreEqual(0f, health.CurrentHealth);
        }

        [Test]
        public void Heal_IncreasesHealth()
        {
            health.TakeDamage(50f);
            health.Heal(20f);
            Assert.AreEqual(70f, health.CurrentHealth);
        }

        [Test]
        public void Heal_ClampsAtMaxHealth()
        {
            health.Heal(50f);
            Assert.AreEqual(100f, health.CurrentHealth);
        }

        [Test]
        public void TakeDamage_AtZero_IsAliveFalse()
        {
            health.TakeDamage(100f);
            Assert.IsFalse(health.IsAlive);
        }
    }
}
