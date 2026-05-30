using UnityEngine;

namespace AnimalMagicRoyale.Core.Data
{
    [CreateAssetMenu(fileName = "NewSkinData", menuName = "Animal Magic Royale/Data/Skin Data")]
    public class SkinData : ScriptableObject
    {
        [Header("Skin Info")]
        public string skinName;
        public AnimalTypeId animalType;
        
        [Header("Visuals")]
        public Sprite previewIcon;
        public GameObject modelPrefab;
    }
}
