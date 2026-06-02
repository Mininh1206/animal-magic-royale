using System.Collections.Generic;
using UnityEngine;
using AnimalMagicRoyale.Components;

namespace AnimalMagicRoyale.Core.Data
{
    public enum AnimalTypeId 
    { 
        Pig, 
        Rooster, 
        Hen, 
        Duck 
    }

    [CreateAssetMenu(fileName = "NewAnimalType", menuName = "Animal Magic Royale/Data/Animal Type")]
    public class AnimalType : ScriptableObject
    {
        [Header("Identity")]
        public AnimalTypeId typeId;
        public string displayName;
        public Sprite icon;
        
        [Header("Abilities")]
        public SpecialAbility ability;
        
        [Header("Audio")]
        public CharacterAudioData audioData;
        
        [Header("Customization")]
        public SkinData defaultSkin;
        public List<SkinData> availableSkins = new List<SkinData>();
    }
}
