using UnityEngine;

namespace AnimalMagicRoyale.Core
{
    /// <summary>
    /// Clase base abstracta para estados concretos. Proporciona acceso
    /// al StateMachine padre y métodos virtuales vacíos para sobreescribir.
    /// </summary>
    public abstract class State : IState
    {
        protected StateMachine stateMachine;

        public State(StateMachine stateMachine)
        {
            this.stateMachine = stateMachine;
        }

        public virtual void Enter() { }
        public virtual void Update() { }
        public virtual void FixedUpdate() { }
        public virtual void Exit() { }
    }
}
