using NUnit.Framework;
using UnityEngine;
using AnimalMagicRoyale.Components;
using AnimalMagicRoyale.Spells;

namespace AnimalMagicRoyale.Tests
{
    public class SpellInventoryTests
    {
        private GameObject go;
        private SpellInventory inventory;
        private SpellData spell1;
        private SpellData spell2;

        [SetUp]
        public void SetUp()
        {
            go = new GameObject("TestInventory");
            inventory = go.AddComponent<SpellInventory>();
            inventory.Awake();
            
            spell1 = ScriptableObject.CreateInstance<SpellData>();
            spell1.spellName = "Spell 1";
            spell2 = ScriptableObject.CreateInstance<SpellData>();
            spell2.spellName = "Spell 2";
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(go);
            Object.DestroyImmediate(spell1);
            Object.DestroyImmediate(spell2);
        }

        [Test]
        public void StartsWithBasicStickOnly()
        {
            Assert.IsTrue(inventory.slots[1].IsEmpty);
            Assert.IsTrue(inventory.slots[2].IsEmpty);
        }

        [Test]
        public void TryPickup_FillsEmptySlot()
        {
            bool success = inventory.TryPickupSpell(spell1);
            
            Assert.IsTrue(success);
            Assert.AreEqual(spell1, inventory.slots[1].spellData);
        }

        [Test]
        public void TryPickup_SecondSpell_FillsSlot2()
        {
            inventory.TryPickupSpell(spell1);
            bool success = inventory.TryPickupSpell(spell2);
            
            Assert.IsTrue(success);
            Assert.AreEqual(spell1, inventory.slots[1].spellData);
            Assert.AreEqual(spell2, inventory.slots[2].spellData);
        }

        [Test]
        public void SelectSlot_ChangesActive()
        {
            inventory.SelectSlot(1);
            Assert.AreEqual(1, inventory.activeSlotIndex);
        }

        [Test]
        public void SwapSlots_SwapsSpellData()
        {
            inventory.TryPickupSpell(spell1); // slot 1
            inventory.TryPickupSpell(spell2); // slot 2

            inventory.SwapSlots(1, 2);

            Assert.AreEqual(spell2, inventory.slots[1].spellData);
            Assert.AreEqual(spell1, inventory.slots[2].spellData);
        }
    }
}
