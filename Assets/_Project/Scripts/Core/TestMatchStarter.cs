using UnityEngine;
using AnimalMagicRoyale.Core;

namespace AnimalMagicRoyale.TestScripts
{
    public class TestMatchStarter : MonoBehaviour
    {
        private void Update()
        {
            if (UnityEngine.InputSystem.Keyboard.current != null && UnityEngine.InputSystem.Keyboard.current.enterKey.wasPressedThisFrame)
            {
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.StartMatch();
                    Debug.Log("Partida iniciada manualmente.");
                }
            }
        }
    }
}
