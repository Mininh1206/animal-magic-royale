using UnityEngine;

namespace AnimalMagicRoyale.Core
{
    /// <summary>
    /// Máquina de estados finita genérica. Gestiona las transiciones
    /// entre estados y delega las llamadas Update/FixedUpdate al estado activo.
    /// </summary>
    public class StateMachine
    {
        public IState CurrentState { get; private set; }
        public IState PreviousState { get; private set; }

        public void Initialize(IState startingState)
        {
            CurrentState = startingState;
            CurrentState?.Enter();
        }

        public void ChangeState(IState newState)
        {
            if (newState == null || newState == CurrentState) return;

            CurrentState?.Exit();
            PreviousState = CurrentState;
            CurrentState = newState;
            CurrentState.Enter();
        }

        public void Update()
        {
            CurrentState?.Update();
        }

        public void FixedUpdate()
        {
            CurrentState?.FixedUpdate();
        }
    }
}
