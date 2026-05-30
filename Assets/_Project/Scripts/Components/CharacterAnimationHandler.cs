using UnityEngine;

namespace AnimalMagicRoyale.Components
{
    /// <summary>
    /// Componente compartido que sincroniza el estado de movimiento con el Animator.
    /// Se usa tanto en el jugador (PlayerController) como en los bots (BotController).
    /// Si no se asigna el Animator manualmente, lo busca en el primer hijo.
    /// </summary>
    public class CharacterAnimationHandler : MonoBehaviour
    {
        [SerializeField] private Animator targetAnimator;
        [SerializeField] private float locomotionBlendSpeed = 10f;
        
        private static readonly int ANIM_IS_MOVING = Animator.StringToHash("isMoving");
        private static readonly int ANIM_IS_RUNNING = Animator.StringToHash("isRunning");
        private static readonly int ANIM_SPEED = Animator.StringToHash("speed");
        private static readonly int ANIM_IS_ATTACKING = Animator.StringToHash("isAttacking");
        
        private float _currentSpeed;
        
        public Animator TargetAnimator => targetAnimator;
        
        private void Awake()
        {
            // Auto-detectar del primer hijo si no está asignado manualmente
            if (targetAnimator == null && transform.childCount > 0)
            {
                targetAnimator = transform.GetChild(0).GetComponentInChildren<Animator>();
            }
            
            if (targetAnimator == null)
            {
                bool isPreview = gameObject.name.Contains("SpawnPoint") || gameObject.name.Contains("Preview");
                if (!isPreview)
                {
                    Debug.LogWarning($"[CharacterAnimationHandler] No se encontró Animator en {gameObject.name}");
                }
            }
        }
        
        /// <summary>
        /// Actualiza animación basándose en velocidad y estado de sprint.
        /// </summary>
        public void UpdateLocomotion(float currentSpeed, bool isSprinting)
        {
            if (targetAnimator == null) return;
            
            bool isMoving = currentSpeed > 0.1f;
            targetAnimator.SetBool(ANIM_IS_MOVING, isMoving);
            targetAnimator.SetBool(ANIM_IS_RUNNING, isMoving && isSprinting);
            
            _currentSpeed = Mathf.Lerp(_currentSpeed, currentSpeed, Time.deltaTime * locomotionBlendSpeed);
            targetAnimator.SetFloat(ANIM_SPEED, _currentSpeed);
        }
        
        public void SetAttacking(bool attacking)
        {
            if (targetAnimator != null)
                targetAnimator.SetBool(ANIM_IS_ATTACKING, attacking);
        }
    }
}
