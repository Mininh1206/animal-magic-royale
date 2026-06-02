using NUnit.Framework;
using UnityEngine;
using AnimalMagicRoyale.Core;
using AnimalMagicRoyale.Core.Data;
using AnimalMagicRoyale.Components;

namespace AnimalMagicRoyale.Tests
{
    public class SkinManagerTests
    {
        private GameObject playerGo;
        private SkinManager skinManager;
        private SkinData testSkinData;
        private GameObject testModelPrefab;

        [SetUp]
        public void SetUp()
        {
            playerGo = new GameObject("Player");
            playerGo.AddComponent<CharacterAnimationHandler>();
            playerGo.AddComponent<SpellInventory>();
            skinManager = playerGo.AddComponent<SkinManager>();

            // Crear un prefab de modelo falso
            testModelPrefab = new GameObject("TestModelPrefab");
            var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.transform.SetParent(testModelPrefab.transform);
            
            testSkinData = ScriptableObject.CreateInstance<SkinData>();
            testSkinData.skinName = "TestSkin";
            testSkinData.modelPrefab = testModelPrefab;
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(playerGo);
            Object.DestroyImmediate(testModelPrefab);
            Object.DestroyImmediate(testSkinData);
        }

        [Test]
        public void SkinManager_ApplySkin_InstantiatesModel_AndAddsBoxCollider()
        {
            skinManager.ApplySkin(testSkinData);

            // Verificar que se instanció un hijo
            Assert.IsTrue(playerGo.transform.childCount > 0);
            Transform modelContainer = playerGo.transform.Find("ModelContainer") ?? playerGo.transform;
            Assert.IsTrue(modelContainer.childCount > 0);

            // Verificar que el hijo instanciado tiene un BoxCollider
            Transform instantiatedModel = modelContainer.GetChild(0);
            BoxCollider boxCollider = instantiatedModel.GetComponent<BoxCollider>();
            
            Assert.IsNotNull(boxCollider, "El BoxCollider no fue añadido al modelo instanciado.");
            Assert.IsFalse(boxCollider.isTrigger, "El BoxCollider debería tener isTrigger=false para colisiones físicas.");
        }

        [Test]
        public void SkinManager_ApplySkin_AutoGeneratesFirePoint_IfMissing()
        {
            skinManager.ApplySkin(testSkinData);

            Transform modelContainer = playerGo.transform.Find("ModelContainer") ?? playerGo.transform;
            Transform instantiatedModel = modelContainer.GetChild(0);
            
            Transform firePoint = instantiatedModel.Find("FirePoint");
            Assert.IsNotNull(firePoint, "FirePoint no fue generado automáticamente.");
            
            SpellInventory inventory = playerGo.GetComponent<SpellInventory>();
            Assert.AreEqual(firePoint, inventory.FirePoint, "El FirePoint generado no se asignó al SpellInventory.");
        }
    }
}
