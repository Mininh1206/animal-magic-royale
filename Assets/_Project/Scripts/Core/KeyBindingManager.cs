using System.Collections.Generic;
using UnityEngine;

namespace AnimalMagicRoyale.Core
{
    /// <summary>
    /// Sistema centralizado de keybindings rebindable. Singleton que persiste
    /// las asignaciones de teclas del jugador en PlayerPrefs.
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
            Interact
        }

        // Valores predeterminados
        private static readonly Dictionary<GameAction, KeyCode> DEFAULT_BINDINGS = new()
        {
            { GameAction.SelectSlot1, KeyCode.Alpha1 },
            { GameAction.SelectSlot2, KeyCode.Alpha2 },
            { GameAction.SelectSlot3, KeyCode.Alpha3 },
            { GameAction.Ability,     KeyCode.Q },
            { GameAction.Interact,    KeyCode.E },
        };

        private Dictionary<GameAction, KeyCode> _currentBindings = new();

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
            return _currentBindings.TryGetValue(action, out KeyCode key) && Input.GetKeyDown(key);
        }

        /// Cambia la tecla de una acción y persiste en PlayerPrefs.
        public void SetBinding(GameAction action, KeyCode newKey)
        {
            _currentBindings[action] = newKey;
            SaveBindings();
        }

        /// Devuelve la tecla actual de una acción.
        public KeyCode GetBinding(GameAction action)
        {
            return _currentBindings.TryGetValue(action, out KeyCode key) ? key : DEFAULT_BINDINGS[action];
        }

        /// Restaura todos los valores predeterminados.
        public void ResetToDefaults()
        {
            _currentBindings = new Dictionary<GameAction, KeyCode>(DEFAULT_BINDINGS);
            SaveBindings();
        }

        private void LoadBindings()
        {
            _currentBindings.Clear();
            foreach (var kvp in DEFAULT_BINDINGS)
            {
                string saved = PlayerPrefs.GetString($"KB_{kvp.Key}", kvp.Value.ToString());
                if (System.Enum.TryParse<KeyCode>(saved, out KeyCode key))
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
