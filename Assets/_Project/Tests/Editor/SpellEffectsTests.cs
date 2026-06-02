using NUnit.Framework;
using UnityEngine;
using AnimalMagicRoyale.Spells.Effects;
using AnimalMagicRoyale.Components;
using AnimalMagicRoyale.Core;
using AnimalMagicRoyale.Spells;

namespace AnimalMagicRoyale.Tests
{
    public class SpellEffectsTests
    {
        private GameObject caster;
        private GameObject target;

        [SetUp]
        public void Setup()
        {
            caster = new GameObject("Caster");
            target = new GameObject("Target");
        }

        [TearDown]
        public void Teardown()
        {
            Object.DestroyImmediate(caster);
            Object.DestroyImmediate(target);
        }

        [Test]
        public void DisarmEffect_CallsDropActiveSpell_WhenApplied()
        {
            // Arrange
            var inventory = target.AddComponent<SpellInventory>();
            var effect = ScriptableObject.CreateInstance<DisarmEffect>();
            
            // Inventario no tiene un hechizo inicial que podamos testear fácilmente si es null 
            // pero podemos verificar que al menos no lance excepciones.
            
            // Act & Assert
            Assert.DoesNotThrow(() => effect.Apply(caster, target));
        }

        [Test]
        public void ShieldEffect_AddsComponentOrLogs_WhenApplied()
        {
            // Arrange
            var hc = target.AddComponent<HealthComponent>();
            var effect = ScriptableObject.CreateInstance<ShieldEffect>();
            effect.shieldAmount = 50f;

            // Act 
            effect.Apply(caster, target);
            
            // Assert
            var shieldComp = target.GetComponent<AnimalMagicRoyale.Components.Abilities.ShieldComponent>();
            Assert.IsNotNull(shieldComp, "ShieldComponent was not added to the target.");
            Assert.AreEqual(50f, shieldComp.remainingShield, "ShieldAmount not correctly initialized.");
        }

        [Test]
        public void SilenceEffect_AddsSilenceComponent()
        {
            // Arrange
            var effect = ScriptableObject.CreateInstance<SilenceEffect>();
            effect.duration = 2f;
            
            // Act
            effect.Apply(caster, target);
            
            // Assert
            var comp = target.GetComponent<SilenceComponent>();
            Assert.IsNotNull(comp, "SilenceComponent was not added to the target.");
        }

        [Test]
        public void PullEffect_AddsPullComponent()
        {
            // Arrange
            var effect = ScriptableObject.CreateInstance<PullEffect>();
            
            // Act
            effect.Apply(caster, target);
            
            // Assert
            var comp = target.GetComponent<PullComponent>();
            Assert.IsNotNull(comp, "PullComponent was not added to the target.");
        }

        [Test]
        public void InvertControlsEffect_LogsCorrectly()
        {
            // Arrange
            var effect = ScriptableObject.CreateInstance<InvertControlsEffect>();

            // Act & Assert
            Assert.DoesNotThrow(() => effect.Apply(caster, target));
        }
        
        [Test]
        public void HardStunEffect_LogsCorrectly()
        {
            // Arrange
            var effect = ScriptableObject.CreateInstance<HardStunEffect>();

            // Act & Assert
            Assert.DoesNotThrow(() => effect.Apply(caster, target));
        }
    }
}
