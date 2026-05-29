using NUnit.Framework;
using UnityEngine;
using AnimalMagicRoyale.Spells;

namespace AnimalMagicRoyale.Tests
{
    public class SpellDataTests
    {
        [Test]
        public void SpellData_HasCorrectDefaults()
        {
            var data = ScriptableObject.CreateInstance<SpellData>();
            
            Assert.AreEqual(1, data.projectileCount);
            Assert.AreEqual(0, data.damage);
            Assert.IsNotNull(data.effects);
            
            Object.DestroyImmediate(data);
        }
    }
}
