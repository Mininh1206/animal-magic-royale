using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace AnimalMagicRoyale.Core
{
    /// <summary>
    /// Sistema centralizado de keybindings rebindable. Singleton que persiste
    /// las asignaciones de teclas del jugador en PlayerPrefs usando el nuevo Input System.
    /// </summary>
    public class KeyBindingManager : MonoBehaviour
    {
        public static KeyBindingManager Instance { get; private set; }

        /// Acciones lógicas del juego.
        public enum GameAction
        {
            SelectSlot1,
            SelectSlot2,
            SelectSlot3,
            Ability,
            Interact,

            // Debug Actions
            HealPlayer,
            LowerHealth
        }

        // Valores predeterminados
        private static readonly Dictionary<GameAction, Key> DEFAULT_BINDINGS = new()
        {
            { GameAction.SelectSlot1, Key.Digit1 },
            { GameAction.SelectSlot2, Key.Digit2 },
            { GameAction.SelectSlot3, Key.Digit3 },
            { GameAction.Ability,     Key.Q },
            { GameAction.Interact,    Key.E },

            // Debug Actions
            { GameAction.HealPlayer,    Key.H },
            { GameAction.LowerHealth,   Key.L }
        };

        private Dictionary<GameAction, Key> _currentBindings = new();

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                LoadBindings();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        /// Comprueba si la acción lógica fue pulsada este frame.
        public bool GetActionDown(GameAction action)
        {
            if (Keyboard.current == null) return false;
            return _currentBindings.TryGetValue(action, out Key key) && Keyboard.current[key].wasPressedThisFrame;
        }

        /// Cambia la tecla de una acción y persiste en PlayerPrefs.
        public void SetBinding(GameAction action, Key newKey)
        {
            _currentBindings[action] = newKey;
            SaveBindings();
        }

        /// Devuelve la tecla actual de una acción.
        public Key GetBinding(GameAction action)
        {
            return _currentBindings.TryGetValue(action, out Key key) ? key : DEFAULT_BINDINGS[action];
        }

        /// Restaura todos los valores predeterminados.
        public void ResetToDefaults()
        {
            _currentBindings = new Dictionary<GameAction, Key>(DEFAULT_BINDINGS);
            SaveBindings();
        }

        private void LoadBindings()
        {
            _currentBindings.Clear();
            foreach (var kvp in DEFAULT_BINDINGS)
            {
                string saved = PlayerPrefs.GetString($"KB_{kvp.Key}", kvp.Value.ToString());
                
                // Conversión de formato antiguo (KeyCode.Alpha1) al nuevo (Key.Digit1) si había datos guardados previos
                if (saved.StartsWith("Alpha")) saved = saved.Replace("Alpha", "Digit");

                if (System.Enum.TryParse<Key>(saved, out Key key))
                    _currentBindings[kvp.Key] = key;
                else
                    _currentBindings[kvp.Key] = kvp.Value;
            }
        }

        private void SaveBindings()
        {
            foreach (var kvp in _currentBindings)
                PlayerPrefs.SetString($"KB_{kvp.Key}", kvp.Value.ToString());
            PlayerPrefs.Save();
        }
    }
}
