using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using AnimalMagicRoyale.Core.Data;
using AnimalMagicRoyale.Core;
using AnimalMagicRoyale.Spells;

namespace AnimalMagicRoyale.Components.UI
{
    [RequireComponent(typeof(UIDocument))]
    public class MainMenuManager : MonoBehaviour
    {
        [Header("Data")]
        [SerializeField] private MapData[] availableMaps;
        [SerializeField] private AnimalType[] availableAnimals;
        
        [Header("References")]
        [SerializeField] private CharacterPreview characterPreview;

        private UIDocument uiDocument;
        private VisualElement root;

        // UI Elements
        private VisualElement lobbyPanel;
        private VisualElement customizationPanel;
        private VisualElement helpPanel;
        private ScrollView mapSelector;
        private ScrollView animalGrid; // was VisualElement, changed to ScrollView in UXML
        private ScrollView skinGrid;
        private VisualElement previewArea;
        private Label abilityText;

        // Help Panel Elements
        private ScrollView spellCompendiumGrid;
        private UnityEngine.UIElements.Image helpSpellIcon;
        private Label helpSpellName;
        private Label helpSpellTier;
        private Label helpSpellDesc;

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
        private List<VisualElement> generatedAnimalCards = new List<VisualElement>();
        private List<VisualElement> generatedSkinCards = new List<VisualElement>();
        private AnimalType currentAnimal;

        private void OnEnable()
        {
            uiDocument = GetComponent<UIDocument>();
            root = uiDocument.rootVisualElement;

            if (root == null) return;

            lobbyPanel = root.Q<VisualElement>("LobbyPanel");
            customizationPanel = root.Q<VisualElement>("CustomizationPanel");
            helpPanel = root.Q<VisualElement>("HelpPanel");

            btnCustomize = root.Q<Button>("CustomizeButton");
            if (btnCustomize != null)
            {
                customizeIcon = btnCustomize.Q<VisualElement>("CustomizeIcon");
                customizeLabel = btnCustomize.Q<Label>("CustomizeLabel");
                btnCustomize.clicked += ToggleCustomization;
            }

            Button btnSettings = root.Q<Button>("SettingsButton");
            if (btnSettings != null) btnSettings.clicked += OpenSettings;

            Button btnHelp = root.Q<Button>("HelpButton");
            if (btnHelp != null) btnHelp.clicked += ToggleHelpPanel;

            Button btnCloseHelp = root.Q<Button>("CloseHelpButton");
            if (btnCloseHelp != null) btnCloseHelp.clicked += SwitchToLobby;

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

            animalGrid = root.Q<ScrollView>("AnimalGrid");
            skinGrid = root.Q<ScrollView>("SkinSelector");
            previewArea = root.Q<VisualElement>("PreviewArea");
            abilityText = root.Q<Label>("AbilityText");

            spellCompendiumGrid = root.Q<ScrollView>("SpellCompendiumGrid");
            helpSpellIcon = root.Q<UnityEngine.UIElements.Image>("HelpSpellIcon");
            helpSpellName = root.Q<Label>("HelpSpellName");
            helpSpellTier = root.Q<Label>("HelpSpellTier");
            helpSpellDesc = root.Q<Label>("HelpSpellDesc");

            if (characterPreview != null && previewArea != null)
            {
                characterPreview.BindToUI(previewArea);
            }
        }

        private void Start()
        {
            // Ensure cursor is visible and unlocked when returning from a game
            UnityEngine.Cursor.lockState = CursorLockMode.None;
            UnityEngine.Cursor.visible = true;

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
                    if (map != null)
                    {
                        VisualElement card = UIElementBuilders.BuildMapCard(map, SelectMap);
                        mapSelector.Add(card);
                        generatedMapCards.Add(card);
                    }

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
            generatedAnimalCards.Clear();
            if (animalGrid != null && availableAnimals != null && availableAnimals.Length > 0)
            {
                animalGrid.Clear();
                int defaultIndex = 0;
                
                for (int i = 0; i < availableAnimals.Length; i++)
                {
                    var animal = availableAnimals[i];
                    VisualElement card = UIElementBuilders.BuildAnimalCard(animal, SelectAnimal);
                    animalGrid.Add(card);
                    generatedAnimalCards.Add(card);
                    
                    if (PlayerPreferencesManager.Instance != null && 
                        (int)animal.typeId == PlayerPreferencesManager.Instance.currentData.animalTypeId)
                    {
                        defaultIndex = i;
                    }
                    else if (animal.typeId == AnimalTypeId.Pig && PlayerPreferencesManager.Instance == null)
                    {
                        defaultIndex = i;
                    }
                }
                SelectAnimal(availableAnimals[defaultIndex], generatedAnimalCards[defaultIndex]);
            }

            // Populate Spells Compendium
            if (spellCompendiumGrid != null)
            {
                spellCompendiumGrid.Clear();
                // Utilizando flex-direction: row y flex-wrap: wrap en el contentContainer para crear la cuadrícula
                spellCompendiumGrid.contentContainer.style.flexDirection = FlexDirection.Row;
                spellCompendiumGrid.contentContainer.style.flexWrap = Wrap.Wrap;

                SpellData[] allSpells = Resources.LoadAll<SpellData>("Spells");
                System.Array.Sort(allSpells, (a, b) => a.tier.CompareTo(b.tier)); // Ordenar por tier

                foreach (var spell in allSpells)
                {
                    VisualElement spellCard = new VisualElement();
                    spellCard.style.width = 80;
                    spellCard.style.height = 80;
                    spellCard.style.marginRight = 10;
                    spellCard.style.marginBottom = 10;
                    spellCard.style.backgroundColor = new StyleColor(new Color(0.2f, 0.2f, 0.2f));
                    spellCard.style.borderTopLeftRadius = 10;
                    spellCard.style.borderTopRightRadius = 10;
                    spellCard.style.borderBottomLeftRadius = 10;
                    spellCard.style.borderBottomRightRadius = 10;
                    spellCard.style.borderTopWidth = 2;
                    spellCard.style.borderBottomWidth = 2;
                    spellCard.style.borderLeftWidth = 2;
                    spellCard.style.borderRightWidth = 2;

                    // Color del borde según tier
                    Color tierColor = Color.white;
                    if (spell.tier == SpellTier.Hormiga) tierColor = Color.green;
                    else if (spell.tier == SpellTier.Ornitorrinco) tierColor = Color.cyan;
                    else if (spell.tier == SpellTier.GOAT) tierColor = new Color(0.8f, 0f, 1f);
                    
                    spellCard.style.borderTopColor = tierColor;
                    spellCard.style.borderBottomColor = tierColor;
                    spellCard.style.borderLeftColor = tierColor;
                    spellCard.style.borderRightColor = tierColor;

                    UnityEngine.UIElements.Image icon = new UnityEngine.UIElements.Image();
                    icon.sprite = spell.icon;
                    icon.style.width = Length.Percent(80);
                    icon.style.height = Length.Percent(80);
                    icon.style.alignSelf = Align.Center;
                    icon.style.marginTop = Length.Percent(10);
                    
                    spellCard.Add(icon);
                    spellCard.RegisterCallback<ClickEvent>(evt => SelectHelpSpell(spell));
                    spellCompendiumGrid.Add(spellCard);
                }
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
            if (helpPanel != null) helpPanel.style.display = DisplayStyle.None;

            if (customizeLabel != null) customizeLabel.text = "CUSTOMIZE";
            if (customizeIcon != null) customizeIcon.style.display = DisplayStyle.Flex;
        }

        public void SwitchToCustomization()
        {
            isInLobby = false;
            if (lobbyPanel != null) lobbyPanel.style.display = DisplayStyle.None;
            if (customizationPanel != null) customizationPanel.style.display = DisplayStyle.Flex;
            if (helpPanel != null) helpPanel.style.display = DisplayStyle.None;

            if (customizeLabel != null) customizeLabel.text = "VOLVER";
            if (customizeIcon != null) customizeIcon.style.display = DisplayStyle.None;
        }
        
        public void ToggleHelpPanel()
        {
            isInLobby = false;
            if (lobbyPanel != null) lobbyPanel.style.display = DisplayStyle.None;
            if (customizationPanel != null) customizationPanel.style.display = DisplayStyle.None;
            if (helpPanel != null) helpPanel.style.display = DisplayStyle.Flex;
            
            if (customizeLabel != null) customizeLabel.text = "VOLVER";
            if (customizeIcon != null) customizeIcon.style.display = DisplayStyle.None;
        }

        private void SelectHelpSpell(SpellData spell)
        {
            if (helpSpellIcon != null) helpSpellIcon.sprite = spell.icon;
            if (helpSpellName != null) helpSpellName.text = spell.spellName;
            
            if (helpSpellTier != null) 
            {
                helpSpellTier.text = spell.tier.ToString().ToUpper();
                Color tierColor = Color.white;
                if (spell.tier == SpellTier.Hormiga) tierColor = Color.green;
                else if (spell.tier == SpellTier.Ornitorrinco) tierColor = Color.cyan;
                else if (spell.tier == SpellTier.GOAT) tierColor = new Color(0.8f, 0f, 1f);
                helpSpellTier.style.color = new StyleColor(tierColor);
            }
            
            if (helpSpellDesc != null)
            {
                helpSpellDesc.text = $"Daño/CD: {spell.cooldown}s\n\n{spell.description}";
            }
        }

        private void OpenSettings()
        {
            if (SettingsManager.Instance != null)
            {
                SettingsManager.Instance.ToggleSettings();
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

            // Only check SelectedMap. SceneLoader might not be in this scene if testing from MainMenu.
            bool canPlay = PlayerSetupData.SelectedMap != null;

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

        private void SelectAnimal(AnimalType animal, VisualElement clickedCard)
        {
            currentAnimal = animal;
            PlayerSetupData.SelectedAnimal = animal;
            
            if (PlayerPreferencesManager.Instance != null)
            {
                PlayerPreferencesManager.Instance.currentData.animalTypeId = (int)animal.typeId;
                PlayerPreferencesManager.Instance.SavePreferences();
            }
            
            // Highlight logic
            foreach (var card in generatedAnimalCards)
            {
                card.RemoveFromClassList("animal-card-selected");
            }
            if (clickedCard != null)
            {
                clickedCard.AddToClassList("animal-card-selected");
            }

            // Update ability text
            if (abilityText != null)
            {
                abilityText.text = animal.ability != null ? animal.ability.abilityName : "Sin Habilidad";
            }
            
            generatedSkinCards.Clear();
            int defaultSkinIndex = -1;
            if (skinGrid != null)
            {
                skinGrid.Clear();
                if (animal.availableSkins != null)
                {
                    for (int i = 0; i < animal.availableSkins.Count; i++)
                    {
                        var skin = animal.availableSkins[i];
                        VisualElement scard = UIElementBuilders.BuildSkinCard(skin, SelectSkin);
                        skinGrid.Add(scard);
                        generatedSkinCards.Add(scard);
                        
                        if (PlayerPreferencesManager.Instance != null && 
                            skin.skinName == PlayerPreferencesManager.Instance.currentData.skinName)
                        {
                            defaultSkinIndex = i;
                        }
                    }
                }
            }

            if (defaultSkinIndex != -1 && defaultSkinIndex < generatedSkinCards.Count)
            {
                SelectSkin(animal.availableSkins[defaultSkinIndex], generatedSkinCards[defaultSkinIndex]);
            }
            else if (animal.defaultSkin != null && generatedSkinCards.Count > 0)
            {
                SelectSkin(animal.defaultSkin, generatedSkinCards[0]);
            }
            else if (animal.availableSkins != null && animal.availableSkins.Count > 0)
            {
                SelectSkin(animal.availableSkins[0], generatedSkinCards[0]);
            }
        }

        private void SelectSkin(SkinData skin, VisualElement clickedCard)
        {
            PlayerSetupData.SelectedSkin = skin;
            
            if (PlayerPreferencesManager.Instance != null)
            {
                PlayerPreferencesManager.Instance.currentData.skinName = skin.skinName;
                PlayerPreferencesManager.Instance.SavePreferences();
            }
            
            // Highlight logic
            foreach (var card in generatedSkinCards)
            {
                card.RemoveFromClassList("skin-card-selected");
            }
            if (clickedCard != null)
            {
                clickedCard.AddToClassList("skin-card-selected");
            }

            if (characterPreview != null)
            {
                characterPreview.ShowPreview(skin, currentAnimal);
            }
        }

        public void OnPlayButtonClicked()
        {
            if (btnPlay.ClassListContains("play-btn-disabled"))
            {
                Debug.LogWarning("[MainMenuManager] Cannot play right now. Play button is disabled.");
                return;
            }

            if (PlayerSetupData.SelectedMap != null && SceneLoaderManager.Instance != null)
            {
                SceneLoaderManager.Instance.LoadMap(PlayerSetupData.SelectedMap);
            }
            else
            {
                Debug.LogWarning("[MainMenuManager] Cannot play: Selected Map or SceneLoader is null.");
            }
        }
    }
}
