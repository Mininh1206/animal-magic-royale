using NUnit.Framework;
using UnityEngine;
using AnimalMagicRoyale.Spells.Effects;
using AnimalMagicRoyale.Components;

namespace AnimalMagicRoyale.Tests
{
    public class SpellEffectTests
    {
        [Test]
        public void DamageEffect_AppliesDamageToHealthComponent()
        {
            var target = new GameObject("Target");
            var health = target.AddComponent<HealthComponent>();
            health.maxHealth = 100f;
            health.Awake();

            var dmgEffect = ScriptableObject.CreateInstance<DamageEffect>();
            dmgEffect.damageAmount = 25f;

            dmgEffect.Apply(null, target);

            Assert.AreEqual(75f, health.CurrentHealth);

            Object.DestroyImmediate(target);
            Object.DestroyImmediate(dmgEffect);
        }
        
        [Test]
        public void HealEffect_HealsTarget()
        {
            var target = new GameObject("Target");
            var health = target.AddComponent<HealthComponent>();
            health.maxHealth = 100f;
            health.Awake();
            health.TakeDamage(50f);

            var healEffect = ScriptableObject.CreateInstance<HealEffect>();
            healEffect.healAmount = 30f;

            healEffect.Apply(target, target);

            Assert.AreEqual(80f, health.CurrentHealth);

            Object.DestroyImmediate(target);
            Object.DestroyImmediate(healEffect);
        }
    }
}
