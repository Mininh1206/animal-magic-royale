using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AnimalMagicRoyale.Spells;
using AnimalMagicRoyale.Components;

namespace AnimalMagicRoyale.Core
{
    public class LootBoxSpawner : MonoBehaviour
    {
        [Header("Prefabs")]
        public GameObject lootBoxPrefab;
        
        [Header("Spawn Points")]
        public Transform[] spawnPoints;

        [Header("Spell Pools")]
        public SpellData[] hormigaSpells;
        public SpellData[] ornitorrincoSpells;
        public SpellData[] goatSpells;

        [Header("Settings")]
        public float respawnInterval = 30f;
        [Range(0f, 1f)] public float commonChance = 0.6f;
        [Range(0f, 1f)] public float rareChance = 0.3f;
        // legendaryChance is implicitly 1 - (commonChance + rareChance)

        [Header("Events")]
        public LootBoxOpenedEvent onLootBoxOpened;

        private void Awake()
        {
            if (onLootBoxOpened == null) onLootBoxOpened = Resources.Load<LootBoxOpenedEvent>("Events/LootBoxOpenedEvent");
        }

        private void Start()
        {
            SpawnInitialBoxes();
        }

        private void SpawnInitialBoxes()
        {
            foreach (Transform point in spawnPoints)
            {
                SpawnBox(point);
            }
        }

        private void SpawnBox(Transform point)
        {
            if (lootBoxPrefab == null) return;

            SpellTier tier = DetermineRandomTier();
            SpellData spell = GetRandomSpellForTier(tier);

            if (spell != null)
            {
                GameObject boxObj = Instantiate(lootBoxPrefab, point.position, point.rotation, transform);
                LootBox box = boxObj.GetComponent<LootBox>();
                if (box != null)
                {
                    box.Spawner = this;
                    box.SpawnPoint = point;
                    box.Initialize(spell, tier);
                }
            }
        }

        private SpellTier DetermineRandomTier()
        {
            float rand = Random.value;
            if (rand <= commonChance) return SpellTier.Hormiga;
            if (rand <= commonChance + rareChance) return SpellTier.Ornitorrinco;
            return SpellTier.GOAT;
        }

        private SpellData GetRandomSpellForTier(SpellTier tier)
        {
            SpellData[] pool = null;
            switch (tier)
            {
                case SpellTier.Hormiga: pool = hormigaSpells; break;
                case SpellTier.Ornitorrinco: pool = ornitorrincoSpells; break;
                case SpellTier.GOAT: pool = goatSpells; break;
            }

            if (pool != null && pool.Length > 0)
            {
                return pool[Random.Range(0, pool.Length)];
            }
            return null;
        }

        public void OnBoxOpened(LootBox box)
        {
            if (onLootBoxOpened != null)
            {
                onLootBoxOpened.Raise(new LootBoxOpenedPayload
                {
                    // For now we don't have player ref here easily, but we can if we adjust LootBox
                });
            }

            StartCoroutine(RespawnRoutine(box.SpawnPoint));
            Destroy(box.gameObject);
        }

        private IEnumerator RespawnRoutine(Transform point)
        {
            yield return new WaitForSeconds(respawnInterval);
            SpawnBox(point);
        }
    }
}
