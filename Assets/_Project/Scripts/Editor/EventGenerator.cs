using UnityEditor;
using UnityEngine;
using System.IO;
using AnimalMagicRoyale.Core;

namespace AnimalMagicRoyale.Editor
{
    public class EventGenerator
    {
        [MenuItem("Animal Magic Royale/Generar Eventos Automáticamente")]
        public static void GenerateEvents()
        {
            string targetFolder = "Assets/_Project/Core/Data/Resources/Events";
            
            // Crear las carpetas si no existen
            if (!Directory.Exists(targetFolder))
            {
                Directory.CreateDirectory(targetFolder);
                AssetDatabase.Refresh();
            }

            // Generar todos los eventos
            CreateEventAsset<ZoneShrinkEvent>(targetFolder, "ZoneShrinkEvent");
            CreateEventAsset<StringEvent>(targetFolder, "StringEvent");
            CreateEventAsset<PlayerEliminatedEvent>(targetFolder, "PlayerEliminatedEvent");
            CreateEventAsset<MatchStartEvent>(targetFolder, "MatchStartEvent");
            CreateEventAsset<LootBoxOpenedEvent>(targetFolder, "LootBoxOpenedEvent");
            CreateEventAsset<IntEvent>(targetFolder, "IntEvent");
            CreateEventAsset<HealthChangedEvent>(targetFolder, "HealthChangedEvent");
            CreateEventAsset<GameStateEvent>(targetFolder, "GameStateEvent");
            CreateEventAsset<DeathEvent>(targetFolder, "DeathEvent");

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            // Debug.Log($"<color=green><b>[Éxito]</b></color> Todos los eventos han sido generados correctamente en: {targetFolder}");
        }

        private static void CreateEventAsset<T>(string folder, string name) where T : ScriptableObject
        {
            string path = $"{folder}/{name}.asset";
            // Solo lo creamos si no existe ya
            if (AssetDatabase.LoadAssetAtPath<T>(path) == null)
            {
                T asset = ScriptableObject.CreateInstance<T>();
                AssetDatabase.CreateAsset(asset, path);
            }
        }
    }
}
