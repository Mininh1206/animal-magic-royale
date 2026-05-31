using NUnit.Framework;
using UnityEngine;
using AnimalMagicRoyale.Components;
using AnimalMagicRoyale.Spells.Effects;

namespace AnimalMagicRoyale.Tests
{
    public class CombatSystemTests
    {
        private GameObject rootGo;
        private GameObject childGo;
        private HealthComponent health;
        private DamageEffect damageEffect;
        private GameObject casterGo;

        [SetUp]
        public void SetUp()
        {
            rootGo = new GameObject("PlayerRoot");
            health = rootGo.AddComponent<HealthComponent>();
            health.maxHealth = 100f;
            health.Awake();

            childGo = new GameObject("PlayerModel");
            childGo.transform.SetParent(rootGo.transform);

            casterGo = new GameObject("Caster");

            damageEffect = ScriptableObject.CreateInstance<DamageEffect>();
            damageEffect.damageAmount = 20f;
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(rootGo);
            Object.DestroyImmediate(casterGo);
            Object.DestroyImmediate(damageEffect);
        }

        [Test]
        public void DamageEffect_AppliesDamage_WhenHittingChildModel()
        {
            float initialHealth = health.CurrentHealth;

            // Simulamos que el proyectil golpea al hijo (donde está el BoxCollider)
            damageEffect.Apply(casterGo, childGo);

            Assert.AreEqual(initialHealth - 20f, health.CurrentHealth, "El daño no se aplicó correctamente al golpear el modelo hijo.");
        }

        [Test]
        public void DamageEffect_AppliesDamage_WhenHittingRoot()
        {
            float initialHealth = health.CurrentHealth;

            // Simulamos que el proyectil golpea a la raíz directamente
            damageEffect.Apply(casterGo, rootGo);

            Assert.AreEqual(initialHealth - 20f, health.CurrentHealth, "El daño no se aplicó correctamente al golpear la raíz.");
        }
    }
}
