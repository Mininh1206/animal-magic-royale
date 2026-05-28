using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using UnityEngine.AI;
using AnimalMagicRoyale.Core;
using AnimalMagicRoyale.Components;
using AnimalMagicRoyale.Spells;
using AnimalMagicRoyale.AI;

namespace AnimalMagicRoyale.Editor
{
    public class M4TestSceneSetup : EditorWindow
    {
        [MenuItem("Animal Magic Royale/Setup M4 Test Scene")]
        public static void ShowWindow()
        {
            GetWindow<M4TestSceneSetup>("M4 AI Test Setup");
        }

        private void OnGUI()
        {
            GUILayout.Label("AI System Test Scene Setup", EditorStyles.boldLabel);
            
            if (GUILayout.Button("Create M4 Test Scene"))
            {
                CreateTestScene();
            }
        }

        private void CreateTestScene()
        {
            // Create a new empty scene
            Scene newScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            newScene.name = "M4_AI_Test";

            // Add lighting
            GameObject dirLight = new GameObject("Directional Light");
            Light light = dirLight.AddComponent<Light>();
            light.type = LightType.Directional;
            dirLight.transform.rotation = Quaternion.Euler(50, -30, 0);

            // Add ground
            GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Ground";
            ground.transform.localScale = new Vector3(10, 1, 10);
            GameObjectUtility.SetStaticEditorFlags(ground, StaticEditorFlags.NavigationStatic);

            // Add GameManager
            GameObject gameManagerObj = new GameObject("GameManager");
            GameManager gm = gameManagerObj.AddComponent<GameManager>();

            // Add ZoneManager
            GameObject zoneManagerObj = new GameObject("ZoneManager");
            ZoneManager zoneManager = zoneManagerObj.AddComponent<ZoneManager>();
            
            GameObject zoneVisual = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            zoneVisual.name = "ZoneVisual";
            zoneVisual.transform.SetParent(zoneManagerObj.transform);
            zoneVisual.transform.localScale = new Vector3(50, 10, 50);
            
            // Hacer el material transparente rojizo
            Renderer zr = zoneVisual.GetComponent<Renderer>();
            zr.sharedMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            zr.sharedMaterial.color = new Color(1, 0, 0, 0.3f);
            
            // Remove cylinder collider so it doesn't block rays
            DestroyImmediate(zoneVisual.GetComponent<Collider>());
            
            zoneManager.zoneVisual = zoneVisual.transform;
            zoneManager.phases = new ZonePhaseData[]
            {
                new ZonePhaseData { startRadius = 50f, endRadius = 25f, shrinkDuration = 10f, waitBeforeShrink = 5f, baseDamage = 5f },
                new ZonePhaseData { startRadius = 25f, endRadius = 5f, shrinkDuration = 10f, waitBeforeShrink = 5f, baseDamage = 10f }
            };

            // Spawner
            GameObject spawnerObj = new GameObject("BotSpawner");
            BotSpawner spawner = spawnerObj.AddComponent<BotSpawner>();
            
            // Create dummy bot prefab in scene for testing
            GameObject bot = CreateDummyBotPrefab();
            spawner.botPrefab = bot;
            spawner.botCount = 3;
            spawner.spawnRadius = 20f;
            
            // Create a Playable Player
            GameObject playablePlayer = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            playablePlayer.name = "PlayablePlayer";
            playablePlayer.transform.position = new Vector3(5, 1, 5);
            playablePlayer.GetComponent<Renderer>().sharedMaterial.color = Color.blue;
            
            playablePlayer.AddComponent<AnimalMagicRoyale.Player.PlayerController>();
            playablePlayer.AddComponent<AnimalMagicRoyale.Player.PlayerInputHandler>();
            playablePlayer.AddComponent<HealthComponent>();
            var playerInv = playablePlayer.AddComponent<SpellInventory>();
            playablePlayer.AddComponent<ZoneDamageTracker>();
            playablePlayer.AddComponent<AbilityHolder>();
            
            // Add Camera
            GameObject camObj = new GameObject("Main Camera");
            UnityEngine.Camera cam = camObj.AddComponent<UnityEngine.Camera>();
            camObj.tag = "MainCamera";
            camObj.transform.SetParent(playablePlayer.transform);
            camObj.transform.localPosition = new Vector3(0, 8, -10);
            camObj.transform.localRotation = Quaternion.Euler(35, 0, 0);

            // Add Projectile Pool
            GameObject poolObj = new GameObject("ProjectilePoolManager");
            poolObj.AddComponent<ProjectilePoolManager>();
            
            // Assign spells
            SpellData basicSpell = AssetDatabase.LoadAssetAtPath<SpellData>("Assets/_Project/Core/Data/Spells/Basic_PaloDeMadera.asset");
            if (basicSpell == null) basicSpell = AssetDatabase.LoadAssetAtPath<SpellData>("Assets/_Project/Core/Data/Abilities/TestSpell.asset");
            
            if (basicSpell != null)
            {
                SerializedObject dummyInvSO = new SerializedObject(playerInv);
                dummyInvSO.FindProperty("basicStickSpell").objectReferenceValue = basicSpell;
                dummyInvSO.ApplyModifiedProperties();
                
                SerializedObject botInvSO = new SerializedObject(bot.GetComponent<SpellInventory>());
                botInvSO.FindProperty("basicStickSpell").objectReferenceValue = basicSpell;
                botInvSO.ApplyModifiedProperties();
            }

            // Bake NavMesh
            UnityEditor.AI.NavMeshBuilder.BuildNavMesh();
            
            Debug.Log("M4 Test Scene generated. NavMesh has been automatically baked!");
        }
        
        private GameObject CreateDummyBotPrefab()
        {
            GameObject bot = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            bot.name = "TestBot";
            bot.transform.position = new Vector3(0, 1, 0);
            bot.GetComponent<Renderer>().sharedMaterial.color = Color.red;
            
            bot.AddComponent<NavMeshAgent>();
            
            var health = bot.AddComponent<HealthComponent>();
            health.maxHealth = 100f;
            
            var inventory = bot.AddComponent<SpellInventory>();
            
            var sensorConfig = ScriptableObject.CreateInstance<AISensorConfig>();
            sensorConfig.targetLayers = -1; // All layers for test
            sensorConfig.obstacleLayers = 0; // No obstacles for test
            
            var sensor = bot.AddComponent<AISensorSystem>();
            // Usar reflection o setter para asignar config si fuera necesario, para el test basta con ponerlo público o via SerializedObject
            SerializedObject so = new SerializedObject(sensor);
            so.FindProperty("config").objectReferenceValue = sensorConfig;
            so.ApplyModifiedProperties();
            
            bot.AddComponent<BotController>();
            
            return bot; // Usually you would make this a real prefab, but for the test scene this works.
        }
    }
}
