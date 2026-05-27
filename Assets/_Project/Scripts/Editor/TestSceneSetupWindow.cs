using UnityEngine;
using UnityEditor;
using AnimalMagicRoyale.Core;
using AnimalMagicRoyale.Components;
using AnimalMagicRoyale.Spells;

namespace AnimalMagicRoyale.EditorScripts
{
    public class TestSceneSetupWindow : EditorWindow
    {
        [MenuItem("Animal Magic Royale/Setup M3 Test Scene")]
        public static void SetupTestScene()
        {
            EnsureDirectories();

            // 1. Crear Managers padre
            GameObject managersGO = GameObject.Find("Managers");
            if (managersGO == null)
            {
                managersGO = new GameObject("Managers");
            }

            // 2. GameManager
            GameObject gmGO = GameObject.Find("GameManager");
            if (gmGO == null)
            {
                gmGO = new GameObject("GameManager");
                gmGO.transform.SetParent(managersGO.transform);
            }
            GameManager gm = gmGO.GetComponent<GameManager>();
            if (gm == null) gm = gmGO.AddComponent<GameManager>();
            
            // Asignar Eventos al GameManager
            gm.onGameStateChanged = CreateOrLoadAsset<GameStateEvent>("Assets/_Project/Core/Data/Events/GameStateEvent.asset");
            gm.onAliveCountChanged = CreateOrLoadAsset<IntEvent>("Assets/_Project/Core/Data/Events/AliveCountEvent.asset");
            gm.onPlayerDeath = CreateOrLoadAsset<DeathEvent>("Assets/_Project/Core/Data/Events/PlayerDeathEvent.asset");
            gm.onMatchStart = CreateOrLoadAsset<MatchStartEvent>("Assets/_Project/Core/Data/Events/MatchStartEvent.asset");
            gm.onPlayerEliminated = CreateOrLoadAsset<PlayerEliminatedEvent>("Assets/_Project/Core/Data/Events/PlayerEliminatedEvent.asset");
            
            // Añadir el script para forzar inicio
            if (gmGO.GetComponent<AnimalMagicRoyale.TestScripts.TestMatchStarter>() == null)
            {
                gmGO.AddComponent<AnimalMagicRoyale.TestScripts.TestMatchStarter>();
            }

            // 2.5. ProjectilePoolManager
            GameObject poolGO = GameObject.Find("ProjectilePoolManager");
            if (poolGO == null)
            {
                poolGO = new GameObject("ProjectilePoolManager");
                poolGO.transform.SetParent(managersGO.transform);
            }
            if (poolGO.GetComponent<AnimalMagicRoyale.Core.ProjectilePoolManager>() == null)
            {
                poolGO.AddComponent<AnimalMagicRoyale.Core.ProjectilePoolManager>();
            }

            // 3. ZoneManager
            GameObject zmGO = GameObject.Find("ZoneManager");
            if (zmGO == null)
            {
                zmGO = new GameObject("ZoneManager");
                zmGO.transform.SetParent(managersGO.transform);
            }
            ZoneManager zm = zmGO.GetComponent<ZoneManager>();
            if (zm == null) zm = zmGO.AddComponent<ZoneManager>();

            zm.onZoneShrink = CreateOrLoadAsset<ZoneShrinkEvent>("Assets/_Project/Core/Data/Events/ZoneShrinkEvent.asset");

            // Crear Visual de la Zona
            Transform zoneVisual = zmGO.transform.Find("ZoneVisual");
            if (zoneVisual == null)
            {
                GameObject cylinder = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                cylinder.name = "ZoneVisual";
                cylinder.transform.SetParent(zmGO.transform);
                cylinder.transform.localPosition = Vector3.zero;
                
                // Eliminar Collider
                DestroyImmediate(cylinder.GetComponent<CapsuleCollider>());

                // Usar el shader de Sprites porque por defecto es transparente y se renderiza por ambas caras (Double Sided) sin problemas de URP
                Material transparentMat = new Material(Shader.Find("Sprites/Default"));
                transparentMat.color = new Color(1f, 0f, 0f, 0.2f); // Rojo semitransparente
                cylinder.GetComponent<MeshRenderer>().sharedMaterial = transparentMat;

                zm.zoneVisual = cylinder.transform;
            }

            // Crear fases de la zona
            ZonePhaseData phase1 = CreateOrLoadAsset<ZonePhaseData>("Assets/_Project/Core/Data/ZonePhases/Phase1.asset");
            phase1.startRadius = 30f; phase1.endRadius = 20f; phase1.waitBeforeShrink = 5f; phase1.shrinkDuration = 10f;
            ZonePhaseData phase2 = CreateOrLoadAsset<ZonePhaseData>("Assets/_Project/Core/Data/ZonePhases/Phase2.asset");
            phase2.startRadius = 20f; phase2.endRadius = 5f; phase2.waitBeforeShrink = 5f; phase2.shrinkDuration = 10f;
            
            zm.phases = new ZonePhaseData[] { phase1, phase2 };

            // 4. LootBoxSpawner
            GameObject spawnerGO = GameObject.Find("LootBoxSpawner");
            if (spawnerGO == null)
            {
                spawnerGO = new GameObject("LootBoxSpawner");
                spawnerGO.transform.SetParent(managersGO.transform);
            }
            LootBoxSpawner spawner = spawnerGO.GetComponent<LootBoxSpawner>();
            if (spawner == null) spawner = spawnerGO.AddComponent<LootBoxSpawner>();

            spawner.onLootBoxOpened = CreateOrLoadAsset<LootBoxOpenedEvent>("Assets/_Project/Core/Data/Events/LootBoxOpenedEvent.asset");

            // Crear Puntos de Spawn
            if (spawnerGO.transform.childCount == 0)
            {
                for(int i = 0; i < 3; i++)
                {
                    GameObject point = new GameObject($"SpawnPoint_{i}");
                    point.transform.SetParent(spawnerGO.transform);
                    point.transform.position = new Vector3(i * 5f, 0f, i * 5f);
                }
            }
            
            spawner.spawnPoints = new Transform[spawnerGO.transform.childCount];
            for (int i = 0; i < spawnerGO.transform.childCount; i++)
            {
                spawner.spawnPoints[i] = spawnerGO.transform.GetChild(i);
            }

            // Cargar los hechizos reales generados si existen
            string spellPath = "Assets/_Project/Core/Data/Spells";
            if (AssetDatabase.IsValidFolder(spellPath))
            {
                string[] guids = AssetDatabase.FindAssets("t:SpellData", new[] { spellPath });
                System.Collections.Generic.List<AnimalMagicRoyale.Spells.SpellData> hormiga = new System.Collections.Generic.List<AnimalMagicRoyale.Spells.SpellData>();
                System.Collections.Generic.List<AnimalMagicRoyale.Spells.SpellData> ornito = new System.Collections.Generic.List<AnimalMagicRoyale.Spells.SpellData>();
                System.Collections.Generic.List<AnimalMagicRoyale.Spells.SpellData> goat = new System.Collections.Generic.List<AnimalMagicRoyale.Spells.SpellData>();

                foreach (var guid in guids)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guid);
                    var sd = AssetDatabase.LoadAssetAtPath<AnimalMagicRoyale.Spells.SpellData>(path);
                    if (sd != null)
                    {
                        if (sd.tier == AnimalMagicRoyale.Spells.SpellTier.Hormiga) hormiga.Add(sd);
                        else if (sd.tier == AnimalMagicRoyale.Spells.SpellTier.Ornitorrinco) ornito.Add(sd);
                        else if (sd.tier == AnimalMagicRoyale.Spells.SpellTier.GOAT) goat.Add(sd);
                    }
                }

                if (hormiga.Count > 0) spawner.hormigaSpells = hormiga.ToArray();
                if (ornito.Count > 0) spawner.ornitorrincoSpells = ornito.ToArray();
                if (goat.Count > 0) spawner.goatSpells = goat.ToArray();
            }
            
            // Fallback: si no hay hechizos generados, usar el de prueba
            if (spawner.hormigaSpells == null || spawner.hormigaSpells.Length == 0)
            {
                AnimalMagicRoyale.Spells.SpellData testSpell = CreateOrLoadAsset<AnimalMagicRoyale.Spells.SpellData>("Assets/_Project/Core/Data/Abilities/TestSpell.asset");
                spawner.hormigaSpells = new AnimalMagicRoyale.Spells.SpellData[] { testSpell };
                spawner.ornitorrincoSpells = new AnimalMagicRoyale.Spells.SpellData[] { testSpell };
                spawner.goatSpells = new AnimalMagicRoyale.Spells.SpellData[] { testSpell };
            }

            // Crear prefab de caja básico
            string prefabPath = "Assets/_Project/Core/Prefabs/TestLootBox.prefab";
            GameObject boxPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (boxPrefab == null)
            {
                EnsureDirectory("Assets/_Project/Core/Prefabs");
                GameObject tempBox = GameObject.CreatePrimitive(PrimitiveType.Cube);
                tempBox.name = "TestLootBox";
                tempBox.GetComponent<BoxCollider>().isTrigger = true;
                tempBox.AddComponent<LootBox>();
                
                Material blueMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                blueMat.SetColor("_BaseColor", Color.blue);
                tempBox.GetComponent<MeshRenderer>().sharedMaterial = blueMat;

                boxPrefab = PrefabUtility.SaveAsPrefabAsset(tempBox, prefabPath);
                DestroyImmediate(tempBox);
            }
            // Crear prefab de Proyectil básico si no existe y asignarlo a todos los hechizos que no tengan
            string projPrefabPath = "Assets/_Project/Core/Prefabs/TestProjectile.prefab";
            GameObject projPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(projPrefabPath);
            if (projPrefab == null)
            {
                GameObject tempProj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                tempProj.name = "TestProjectile";
                tempProj.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
                tempProj.GetComponent<SphereCollider>().isTrigger = true;
                
                Rigidbody rb = tempProj.AddComponent<Rigidbody>();
                rb.useGravity = false;
                rb.isKinematic = false;
                
                tempProj.AddComponent<AnimalMagicRoyale.Spells.Projectile>();

                Material yellowMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                yellowMat.SetColor("_BaseColor", Color.yellow);
                tempProj.GetComponent<MeshRenderer>().sharedMaterial = yellowMat;

                projPrefab = PrefabUtility.SaveAsPrefabAsset(tempProj, projPrefabPath);
                DestroyImmediate(tempProj);
            }

            // Asignar el proyectil a todos los hechizos generados
            if (AssetDatabase.IsValidFolder(spellPath))
            {
                string[] guids = AssetDatabase.FindAssets("t:SpellData", new[] { spellPath });
                foreach (var guid in guids)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guid);
                    var sd = AssetDatabase.LoadAssetAtPath<AnimalMagicRoyale.Spells.SpellData>(path);
                    if (sd != null && sd.projectilePrefab == null)
                    {
                        sd.projectilePrefab = projPrefab;
                        EditorUtility.SetDirty(sd);
                    }
                }
            }

            // 5. Crear Jugador de Prueba
            GameObject playerGO = GameObject.Find("TestPlayer");
            if (playerGO == null)
            {
                playerGO = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                playerGO.name = "TestPlayer";
                playerGO.transform.position = new Vector3(0, 1, 0);

                // Añadir componentes de movimiento y lógica
                if (playerGO.GetComponent<AnimalMagicRoyale.Player.PlayerController>() == null) playerGO.AddComponent<AnimalMagicRoyale.Player.PlayerController>();
                if (playerGO.GetComponent<AnimalMagicRoyale.Player.PlayerInputHandler>() == null) playerGO.AddComponent<AnimalMagicRoyale.Player.PlayerInputHandler>();
                
                // Añadir componentes de sistemas
                if (playerGO.GetComponent<HealthComponent>() == null) playerGO.AddComponent<HealthComponent>();
                if (playerGO.GetComponent<SpellInventory>() == null) playerGO.AddComponent<SpellInventory>();
                if (playerGO.GetComponent<ZoneDamageTracker>() == null) playerGO.AddComponent<ZoneDamageTracker>();
                
                AbilityHolder holder = playerGO.GetComponent<AbilityHolder>();
                if (holder == null) holder = playerGO.AddComponent<AbilityHolder>();

                // Asignar hechizo básico al inventario
                AnimalMagicRoyale.Spells.SpellData basicSpell = CreateOrLoadAsset<AnimalMagicRoyale.Spells.SpellData>("Assets/_Project/Core/Data/Spells/Basic_PaloDeMadera.asset");
                if (basicSpell != null)
                {
                    basicSpell.projectilePrefab = projPrefab;
                    SerializedObject invSO = new SerializedObject(playerGO.GetComponent<SpellInventory>());
                    invSO.FindProperty("basicStickSpell").objectReferenceValue = basicSpell;
                    invSO.ApplyModifiedProperties();
                }

                // Crear y asignar habilidad de prueba
                AnimalMagicRoyale.Components.Abilities.MudShieldAbility shieldAbility = CreateOrLoadAsset<AnimalMagicRoyale.Components.Abilities.MudShieldAbility>("Assets/_Project/Core/Data/Abilities/TestMudShield.asset");
                shieldAbility.shieldAmount = 50f;
                shieldAbility.duration = 5f;
                shieldAbility.cooldown = 10f;
                
                SerializedObject so = new SerializedObject(holder);
                so.FindProperty("ability").objectReferenceValue = shieldAbility;
                so.ApplyModifiedProperties();

                // Asegurar que detecta el suelo
                AnimalMagicRoyale.Player.PlayerController pc = playerGO.GetComponent<AnimalMagicRoyale.Player.PlayerController>();
                SerializedObject pcSO = new SerializedObject(pc);
                pcSO.FindProperty("groundLayers").intValue = 1; // 1 es el Layer "Default"
                pcSO.ApplyModifiedProperties();
            }

            // Registrar al jugador en GameManager
            if (gm != null)
            {
                gm.RegisterPlayer(playerGO);
            }

            // 6. Configurar Cámara
            UnityEngine.Camera mainCam = UnityEngine.Camera.main;
            if (mainCam != null && playerGO != null)
            {
                mainCam.transform.SetParent(playerGO.transform);
                mainCam.transform.localPosition = new Vector3(0, 3f, -6f);
                mainCam.transform.localRotation = Quaternion.Euler(15f, 0, 0);
            }

            // 7. Crear un Suelo para que no se caiga
            GameObject floor = GameObject.Find("TestFloor");
            if (floor == null)
            {
                floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
                floor.name = "TestFloor";
                floor.transform.position = new Vector3(0, -0.5f, 0);
                floor.transform.localScale = new Vector3(100f, 1f, 100f);
                
                Material floorMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                floorMat.SetColor("_BaseColor", Color.gray);
                floor.GetComponent<MeshRenderer>().sharedMaterial = floorMat;
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("<color=green>¡M3 Test Scene configurada correctamente!</color> Managers listos, jugador creado y cajas preparadas.");
        }

        private static void EnsureDirectories()
        {
            EnsureDirectory("Assets/_Project/Core/Data");
            EnsureDirectory("Assets/_Project/Core/Data/Events");
            EnsureDirectory("Assets/_Project/Core/Data/ZonePhases");
            EnsureDirectory("Assets/_Project/Core/Data/Abilities");
            EnsureDirectory("Assets/_Project/Core/Prefabs");
        }

        private static void EnsureDirectory(string path)
        {
            if (!AssetDatabase.IsValidFolder(path))
            {
                string[] parts = path.Split('/');
                string currentPath = parts[0];
                for (int i = 1; i < parts.Length; i++)
                {
                    string nextPath = currentPath + "/" + parts[i];
                    if (!AssetDatabase.IsValidFolder(nextPath))
                    {
                        AssetDatabase.CreateFolder(currentPath, parts[i]);
                    }
                    currentPath = nextPath;
                }
            }
        }

        private static T CreateOrLoadAsset<T>(string path) where T : ScriptableObject
        {
            T asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<T>();
                AssetDatabase.CreateAsset(asset, path);
            }
            EditorUtility.SetDirty(asset);
            return asset;
        }
    }
}
