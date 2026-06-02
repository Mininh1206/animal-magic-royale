using UnityEngine;
using AnimalMagicRoyale.Components.UI;

namespace AnimalMagicRoyale.Core
{
    public static class UIStateManager
    {
        public static bool IsAnyMenuOpen()
        {
            bool isSettingsOpen = SettingsManager.Instance != null && SettingsManager.Instance.IsOpen;
            bool isInventoryOpen = InventoryUI.Instance != null && InventoryUI.Instance.IsOpen;
            return isSettingsOpen || isInventoryOpen;
        }
    }
}
