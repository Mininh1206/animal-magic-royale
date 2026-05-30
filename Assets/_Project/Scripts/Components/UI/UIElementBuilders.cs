using System;
using UnityEngine.UIElements;
using AnimalMagicRoyale.Core.Data;
using UnityEngine;

namespace AnimalMagicRoyale.Components.UI
{
    public static class UIElementBuilders
    {
        public static VisualElement BuildMapCard(MapData data, Action<MapData, VisualElement> onClick)
        {
            Button card = new Button();
            card.AddToClassList("map-card");

            VisualElement previewContainer = new VisualElement();
            previewContainer.AddToClassList("map-card-preview-container");
            
            VisualElement preview = new VisualElement();
            preview.AddToClassList("map-card-preview-image");
            if (data.previewImage != null)
            {
                preview.style.backgroundImage = new StyleBackground(data.previewImage);
            }
            previewContainer.Add(preview);
            
            VisualElement info = new VisualElement();
            info.AddToClassList("map-card-info");

            Label title = new Label(data.mapName);
            title.AddToClassList("map-card-name");

            VisualElement playersContainer = new VisualElement();
            playersContainer.AddToClassList("map-card-players-container");

            VisualElement icon = new VisualElement();
            icon.AddToClassList("map-card-players-icon");

            Label maxPlayers = new Label($"Max {data.maxPlayers}");
            maxPlayers.AddToClassList("map-card-players");

            playersContainer.Add(icon);
            playersContainer.Add(maxPlayers);

            info.Add(title);
            info.Add(playersContainer);

            card.Add(previewContainer);
            card.Add(info);

            // Pass both the data and the visual element to the callback
            card.clicked += () => onClick?.Invoke(data, card);

            return card;
        }

        public static VisualElement BuildAnimalCard(AnimalType data, Action<AnimalType> onClick)
        {
            Button card = new Button();
            card.AddToClassList("animal-card");

            VisualElement icon = new VisualElement();
            icon.AddToClassList("animal-card-icon");
            icon.style.backgroundImage = data.icon != null ? new StyleBackground(data.icon) : new StyleBackground();

            Label name = new Label(data.displayName);
            name.AddToClassList("animal-card-name");

            card.Add(icon);
            card.Add(name);

            card.clicked += () => onClick?.Invoke(data);

            return card;
        }

        public static VisualElement BuildSkinCard(SkinData data, Action<SkinData> onClick)
        {
            Button card = new Button();
            card.AddToClassList("skin-card");

            VisualElement preview = new VisualElement();
            preview.AddToClassList("skin-card-preview");
            preview.style.backgroundImage = data.previewIcon != null ? new StyleBackground(data.previewIcon) : new StyleBackground();

            Label name = new Label(data.skinName);
            name.AddToClassList("skin-card-name");

            card.Add(preview);
            card.Add(name);

            card.clicked += () => onClick?.Invoke(data);

            return card;
        }
    }
}
