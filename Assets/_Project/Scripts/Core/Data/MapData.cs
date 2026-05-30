using UnityEngine;

namespace AnimalMagicRoyale.Core.Data
{
    [CreateAssetMenu(fileName = "NewMapData", menuName = "Animal Magic Royale/Data/Map Data")]
    public class MapData : ScriptableObject
    {
        [Header("Map Identity")]
        public string mapName;
        public string sceneName; // Exact name of the scene in Build Settings
        
        [Header("UI Visuals")]
        public Sprite previewImage;
        [TextArea] public string description;
        
        [Header("Game Settings")]
        [Tooltip("Maximum allowed players in this map")]
        public int maxPlayers = 20;
    }
}
