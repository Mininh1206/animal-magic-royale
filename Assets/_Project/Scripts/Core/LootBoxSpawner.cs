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
            LoadSpellsFromResources();
        }

        private void LoadSpellsFromResources()
        {
            SpellData[] allSpells = Resources.LoadAll<SpellData>("Spells");
            List<SpellData> hormigaList = new List<SpellData>();
            List<SpellData> ornitorrincoList = new List<SpellData>();
            List<SpellData> goatList = new List<SpellData>();

            foreach (var spell in allSpells)
            {
                if (spell.tier == SpellTier.Hormiga) hormigaList.Add(spell);
                else if (spell.tier == SpellTier.Ornitorrinco) ornitorrincoList.Add(spell);
                else if (spell.tier == SpellTier.GOAT) goatList.Add(spell);
            }

            if (hormigaSpells == null || hormigaSpells.Length == 0) hormigaSpells = hormigaList.ToArray();
            if (ornitorrincoSpells == null || ornitorrincoSpells.Length == 0) ornitorrincoSpells = ornitorrincoList.ToArray();
            if (goatSpells == null || goatSpells.Length == 0) goatSpells = goatList.ToArray();

            Debug.Log($"[LootBoxSpawner] Cargados por defecto: {hormigaSpells.Length} Hormiga, {ornitorrincoSpells.Length} Ornitorrinco, {goatSpells.Length} GOAT.");
        }

        private void Start()
        {
            SpawnInitialBoxes();
        }

        private void SpawnInitialBoxes()
        {
            if (spawnPoints == null || spawnPoints.Length == 0) return;

            List<Transform> actualSpawnPoints = new List<Transform>();
            foreach (var sp in spawnPoints)
            {
                if (sp != null && sp.childCount > 0)
                {
                    foreach (Transform child in sp)
                    {
                        actualSpawnPoints.Add(child);
                    }
                }
                else if (sp != null)
                {
                    actualSpawnPoints.Add(sp);
                }
            }

            foreach (Transform point in actualSpawnPoints)
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
