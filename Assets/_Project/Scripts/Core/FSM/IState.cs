using UnityEngine;

namespace AnimalMagicRoyale.Core
{
    /// <summary>
    /// Interfaz base para todos los estados de la FSM.
    /// </summary>
    public interface IState
    {
        void Enter();
        void Update();
        void FixedUpdate();
        void Exit();
    }
}
