using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using AnimalMagicRoyale.Components;
using AnimalMagicRoyale.Core;
using AnimalMagicRoyale.Components.Abilities;

namespace AnimalMagicRoyale.AI
{
    [RequireComponent(typeof(NavMeshAgent))]
    [RequireComponent(typeof(AISensorSystem))]
    [RequireComponent(typeof(BotContextUpdater))]
    public class BotController : BasicController
    {
        public float attackRange = 10f;
        
        public NavMeshAgent Agent { get; private set; }
        public AISensorSystem Sensor { get; private set; }
        public BotContextUpdater ContextUpdater { get; private set; }

        public BotContext Context { get; private set; }

        private FuzzyController fuzzyController;
        private BTNode behaviorTree;
        private float stuckTimer = 0f;

        private float takingDamageTimer = 0f;

        protected override void Awake()
        {
            base.Awake();
            Agent = GetComponent<NavMeshAgent>();
            Sensor = GetComponent<AISensorSystem>();
            ContextUpdater = GetComponent<BotContextUpdater>();

            fuzzyController = new FuzzyController();
            Context = new BotContext { Bot = this, Sensor = Sensor };
            
            BuildBehaviorTree();
        }

        protected override void Start()
        {
            base.Start();
            
            if (TeamManager.Instance != null)
            {
                Context.TeamId = TeamManager.Instance.GetTeam(gameObject);
            }

            if (Inventory != null && Inventory.GetActiveSpell() == null)
            {
                Debug.LogWarning($"[BotController] {gameObject.name} no tiene un hechizo activo asignado en su SpellInventory!");
            }
        }

        private void OnEnable()
        {
            if (Health != null && Health.onHealthChanged != null)
            {
                Health.onHealthChanged.RegisterListener(HandleDamage);
            }
        }

        private void OnDisable()
        {
            if (Health != null && Health.onHealthChanged != null)
            {
                Health.onHealthChanged.UnregisterListener(HandleDamage);
            }
        }

        private void HandleDamage(HealthChangedPayload payload)
        {
            if (payload.target == gameObject && payload.delta < 0 && payload.source != null)
            {
                takingDamageTimer = 1.0f; // Activar flag de daño reciente por 1 segundo
                Context.IsTakingDamage = true;
                Context.CurrentRevengeTarget = payload.source.transform;

                // Reportar a la memoria del equipo si es un atacante lejano
                if (TeamMemorySystem.Instance != null)
                {
#pragma warning disable CS0618
                    TeamMemorySystem.Instance.ReportEnemy(Context.TeamId, payload.source.GetEntityId(), payload.source.transform.position);
#pragma warning restore CS0618
                }

                Vector3 dir = payload.source.transform.position - transform.position;
                dir.y = 0; 
                if (dir.sqrMagnitude > 0.01f)
                {
                    transform.rotation = Quaternion.LookRotation(dir);
                }
            }
        }

        private void BuildBehaviorTree()
        {
            var investigateSequence = new BTSequence(new List<BTNode>
            {
                BotConditions.ShouldInvestigate(),
                BotActions.Investigate()
            });

            var fleeSequence = new BTSequence(new List<BTNode>
            {
                BotConditions.ShouldFlee(),
                BotActions.FleeFromNearestEnemy()
            });

            var combatSequence = new BTSequence(new List<BTNode>
            {
                BotConditions.ShouldAttack(),
                BotActions.PursueAndAttack()
            });

            var collectSequence = new BTSequence(new List<BTNode>
            {
                BotConditions.ShouldCollect(),
                BotActions.MoveToLoot()
            });

            var zoneSequence = new BTSequence(new List<BTNode>
            {
                BotConditions.IsOutsideZone(),
                BotActions.MoveToZoneCenter()
            });

            // Prioridades: Zona > Huir > Combate > Investigar > Recolectar > Patrullar
            behaviorTree = new BTSelector(new List<BTNode>
            {
                zoneSequence,
                fleeSequence,
                combatSequence,
                investigateSequence,
                collectSequence,
                BotActions.PatrolRandomPoint()
            });
        }

        private float CalculateThreat(float healthPerc, int enemyCount, float enemyDist)
        {
            float baseThreat = (enemyCount * 0.3f);
            
            if (healthPerc < 40f) 
                baseThreat += 0.5f;
            else if (healthPerc < 70f)
                baseThreat += 0.2f;

            if (Context.IsTakingDamage) 
                baseThreat += 0.2f; // Pico de pánico
            
            if (ZoneManager.Instance != null && !ZoneManager.Instance.IsInsideZone(transform.position))
            {
                return 1.0f; // Máxima amenaza si está fuera de zona
            }

            return Mathf.Clamp01(baseThreat);
        }

        private void Update()
        {
            if (!Health.IsAlive)
            {
                if (Agent.isOnNavMesh && Agent.isActiveAndEnabled) Agent.isStopped = true;
                return;
            }

            if (GameManager.Instance == null || 
                GameManager.Instance.StateMachine.CurrentState is not PlayingState)
            {
                if (Agent.isOnNavMesh && Agent.isActiveAndEnabled) Agent.isStopped = true;
                return;
            }

            if (Agent.isOnNavMesh && Agent.isActiveAndEnabled && Agent.isStopped)
            {
                Agent.isStopped = false;
            }

            // Actualizar timer de daño
            if (takingDamageTimer > 0)
            {
                takingDamageTimer -= Time.deltaTime;
                if (takingDamageTimer <= 0) Context.IsTakingDamage = false;
            }

            ContextUpdater.UpdateBotContext(Context);
            
            // Stuck Prevention
            if (Agent.isOnNavMesh && !Agent.isStopped && Agent.hasPath)
            {
                if (Agent.velocity.sqrMagnitude < 0.1f)
                {
                    stuckTimer += Time.deltaTime;
                    if (stuckTimer > 2f)
                    {
                        if (Agent.isActiveAndEnabled) Agent.ResetPath();
                        
                        var ability = GetComponent<AbilityHolder>();
                        if (ability != null && ability.IsReady) ability.TryActivate();
                        
                        Vector3 right = Vector3.Cross(transform.forward, Vector3.up);
                        Agent.Move(right * (Random.value > 0.5f ? 1f : -1f) * 2f);

                        stuckTimer = 0f;
                        Debug.Log($"[BotController] {gameObject.name} got stuck. Unstucking.");
                    }
                }
                else
                {
                    stuckTimer = 0f;
                }
            }

            // Fuzzy Logic Step
            float healthPerc = (Health.CurrentHealth / Health.maxHealth) * 100f;
            
            int enemyCount = 0;
            if (Sensor != null)
            {
                foreach(var t in Sensor.VisibleTargets) 
                    if(t.type == TargetType.Enemy) enemyCount++;
            }
            
            float enemyDist = Context.NearestEnemy != null ? Vector3.Distance(transform.position, Context.NearestEnemy.position) : 50f;
            float threat = CalculateThreat(healthPerc, enemyCount, enemyDist);

            Context.FuzzyResult = fuzzyController.Evaluate(healthPerc, enemyDist, threat);

            // Behavior Tree Step
            behaviorTree.Tick(Context);

            // Animations and Sounds
            if (AnimHandler != null)
            {
                float botSpeed = Agent.velocity.magnitude;
                bool botRunning = botSpeed > 6.5f; 
                AnimHandler.UpdateLocomotion(botSpeed, botRunning);
                
                if (SFXHandler != null)
                {
                    bool isMoving = botSpeed > 0.1f;
                    SFXHandler.SetFootstepsActive(isMoving);
                }
            }
        }
    }
}
