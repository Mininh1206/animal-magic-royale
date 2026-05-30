using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using AnimalMagicRoyale.Core.Data;
using AnimalMagicRoyale.Core;

namespace AnimalMagicRoyale.Components.UI
{
    [RequireComponent(typeof(UIDocument))]
    public class MainMenuManager : MonoBehaviour
    {
        [Header("Data")]
        [SerializeField] private MapData[] availableMaps;
        [SerializeField] private AnimalType[] availableAnimals;
        
        [Header("References")]
        [SerializeField] private SettingsManager settingsManager;
        [SerializeField] private CharacterPreview characterPreview;

        private UIDocument uiDocument;
        private VisualElement root;

        // UI Elements
        private VisualElement lobbyPanel;
        private VisualElement customizationPanel;
        private ScrollView mapSelector;
        private VisualElement characterGrid;
        private ScrollView skinGrid;
        private VisualElement previewArea;

        // Top Bar Toggle
        private Button btnCustomize;
        private VisualElement customizeIcon;
        private Label customizeLabel;
        private bool isInLobby = true;

        // Team Mode Buttons
        private Button btnSolo;
        private Button btnDuos;
        private Button btnSquads;

        // Play Button
        private Button btnPlay;
        private VisualElement iconPlay;
        private Label playBtnText;

        // State Tracking
        private List<VisualElement> generatedMapCards = new List<VisualElement>();
        private AnimalType currentAnimal;

        private void OnEnable()
        {
            uiDocument = GetComponent<UIDocument>();
            root = uiDocument.rootVisualElement;

            if (root == null) return;

            lobbyPanel = root.Q<VisualElement>("LobbyPanel");
            customizationPanel = root.Q<VisualElement>("CustomizationPanel");

            btnCustomize = root.Q<Button>("CustomizeButton");
            if (btnCustomize != null)
            {
                customizeIcon = btnCustomize.Q<VisualElement>("CustomizeIcon");
                customizeLabel = btnCustomize.Q<Label>("CustomizeLabel");
                btnCustomize.clicked += ToggleCustomization;
            }

            Button btnSettings = root.Q<Button>("SettingsButton");
            if (btnSettings != null) btnSettings.clicked += OpenSettings;

            mapSelector = root.Q<ScrollView>("MapSelector");
            
            btnSolo = root.Q<Button>("BtnSolo");
            btnDuos = root.Q<Button>("BtnDuos");
            btnSquads = root.Q<Button>("BtnSquads");

            if (btnSolo != null) btnSolo.clicked += () => SelectTeamMode(TeamMode.Solo, btnSolo);
            if (btnDuos != null) btnDuos.clicked += () => SelectTeamMode(TeamMode.Duos, btnDuos);
            if (btnSquads != null) btnSquads.clicked += () => SelectTeamMode(TeamMode.Squads, btnSquads);

            btnPlay = root.Q<Button>("PlayButton");
            if (btnPlay != null)
            {
                iconPlay = btnPlay.Q<VisualElement>(className: "icon-play");
                playBtnText = btnPlay.Q<Label>(className: "play-btn-text");
                btnPlay.clicked += OnPlayButtonClicked;
            }

            characterGrid = root.Q<VisualElement>("AnimalGrid");
            skinGrid = root.Q<ScrollView>("SkinSelector");
            previewArea = root.Q<VisualElement>("PreviewArea");

            if (characterPreview != null && previewArea != null)
            {
                characterPreview.BindToUI(previewArea);
            }

            InitializeData();
            SwitchToLobby();
        }

        private void InitializeData()
        {
            generatedMapCards.Clear();
            if (mapSelector != null) mapSelector.Clear();

            // Populate Maps
            if (mapSelector != null && availableMaps != null && availableMaps.Length > 0)
            {
                foreach (var map in availableMaps)
                {
                    VisualElement card = UIElementBuilders.BuildMapCard(map, SelectMap);
                    mapSelector.Add(card);
                    generatedMapCards.Add(card);
                }
                // Force select first map
                SelectMap(availableMaps[0], generatedMapCards[0]);
            }
            else
            {
                // No maps available, disable play button
                PlayerSetupData.SelectedMap = null;
                UpdatePlayButtonState();
            }

            // Populate Animals
            if (characterGrid != null && availableAnimals != null && availableAnimals.Length > 0)
            {
                characterGrid.Clear();
                foreach (var animal in availableAnimals)
                {
                    characterGrid.Add(UIElementBuilders.BuildAnimalCard(animal, SelectAnimal));
                }
                SelectAnimal(availableAnimals[0]);
            }

            // Default Team Mode (Solo)
            SelectTeamMode(TeamMode.Solo, btnSolo);
        }

        private void ToggleCustomization()
        {
            if (isInLobby)
            {
                SwitchToCustomization();
            }
            else
            {
                SwitchToLobby();
            }
        }

        public void SwitchToLobby()
        {
            isInLobby = true;
            if (lobbyPanel != null) lobbyPanel.style.display = DisplayStyle.Flex;
            if (customizationPanel != null) customizationPanel.style.display = DisplayStyle.None;

            if (customizeLabel != null) customizeLabel.text = "CUSTOMIZE";
            if (customizeIcon != null) customizeIcon.style.display = DisplayStyle.Flex;
        }

        public void SwitchToCustomization()
        {
            isInLobby = false;
            if (lobbyPanel != null) lobbyPanel.style.display = DisplayStyle.None;
            if (customizationPanel != null) customizationPanel.style.display = DisplayStyle.Flex;

            if (customizeLabel != null) customizeLabel.text = "VOLVER";
            if (customizeIcon != null) customizeIcon.style.display = DisplayStyle.None;
        }

        private void OpenSettings()
        {
            if (settingsManager != null)
            {
                settingsManager.ShowSettings();
            }
        }

        private void SelectMap(MapData map, VisualElement clickedCard)
        {
            PlayerSetupData.SelectedMap = map;
            
            // Remove highlight from all cards
            foreach (var card in generatedMapCards)
            {
                card.RemoveFromClassList("map-card-selected");
            }

            // Highlight the selected one
            if (clickedCard != null)
            {
                clickedCard.AddToClassList("map-card-selected");
            }

            UpdatePlayButtonState();
        }

        private void SelectTeamMode(TeamMode mode, Button clickedButton)
        {
            PlayerSetupData.SelectedTeamMode = mode;

            // Remove highlight from all
            if (btnSolo != null) btnSolo.RemoveFromClassList("team-btn-selected");
            if (btnDuos != null) btnDuos.RemoveFromClassList("team-btn-selected");
            if (btnSquads != null) btnSquads.RemoveFromClassList("team-btn-selected");

            // Highlight selected
            if (clickedButton != null)
            {
                clickedButton.AddToClassList("team-btn-selected");
            }
        }

        private void UpdatePlayButtonState()
        {
            if (btnPlay == null) return;

            bool canPlay = PlayerSetupData.SelectedMap != null && SceneLoader.Instance != null;

            if (canPlay)
            {
                btnPlay.RemoveFromClassList("play-btn-disabled");
                if (iconPlay != null) iconPlay.style.unityBackgroundImageTintColor = new StyleColor(new Color(0.36f, 0.19f, 0f)); // #5C3100
                if (playBtnText != null) playBtnText.style.color = new StyleColor(new Color(0.36f, 0.19f, 0f));
            }
            else
            {
                btnPlay.AddToClassList("play-btn-disabled");
                // Mute colors
                if (iconPlay != null) iconPlay.style.unityBackgroundImageTintColor = new StyleColor(new Color(0.3f, 0.3f, 0.3f));
                if (playBtnText != null) playBtnText.style.color = new StyleColor(new Color(0.3f, 0.3f, 0.3f));
            }
        }

        private void SelectAnimal(AnimalType animal)
        {
            currentAnimal = animal;
            PlayerSetupData.SelectedAnimal = animal;
            
            if (skinGrid != null)
            {
                skinGrid.Clear();
                if (animal.availableSkins != null)
                {
                    foreach (var skin in animal.availableSkins)
                    {
                        skinGrid.Add(UIElementBuilders.BuildSkinCard(skin, SelectSkin));
                    }
                }
            }

            if (animal.defaultSkin != null)
            {
                SelectSkin(animal.defaultSkin);
            }
        }

        private void SelectSkin(SkinData skin)
        {
            PlayerSetupData.SelectedSkin = skin;
            if (characterPreview != null)
            {
                characterPreview.ShowPreview(skin);
            }
        }

        public void OnPlayButtonClicked()
        {
            if (btnPlay.ClassListContains("play-btn-disabled"))
            {
                Debug.LogWarning("[MainMenuManager] Cannot play right now. Play button is disabled.");
                return;
            }

            if (PlayerSetupData.SelectedMap != null && SceneLoader.Instance != null)
            {
                SceneLoader.Instance.LoadMap(PlayerSetupData.SelectedMap);
            }
            else
            {
                Debug.LogWarning("[MainMenuManager] Cannot play: Selected Map or SceneLoader is null.");
            }
        }
    }
}
