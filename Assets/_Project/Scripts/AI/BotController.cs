using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using AnimalMagicRoyale.Components;
using AnimalMagicRoyale.Core;

namespace AnimalMagicRoyale.AI
{
    [RequireComponent(typeof(NavMeshAgent))]
    [RequireComponent(typeof(AISensorSystem))]
    public class BotController : BasicController
    {
        public float attackRange = 10f;
        
        public NavMeshAgent Agent { get; private set; }
        public AISensorSystem Sensor { get; private set; }

        private FuzzyController fuzzyController;
        private BTNode behaviorTree;
        private BotContext context;
        private float logTimer = 0f;

        protected override void Awake()
        {
            base.Awake();
            Agent = GetComponent<NavMeshAgent>();
            Sensor = GetComponent<AISensorSystem>();

            fuzzyController = new FuzzyController();
            context = new BotContext { Bot = this, Sensor = Sensor };
            
            BuildBehaviorTree();
        }

        protected override void Start()
        {
            base.Start();
            
            if (Inventory != null && Inventory.GetActiveSpell() == null)
            {
                Debug.LogWarning($"[BotController] {gameObject.name} no tiene un hechizo activo asignado en su SpellInventory!");
            }
            else if (Inventory == null)
            {
                Debug.LogWarning($"[BotController] {gameObject.name} no tiene el componente SpellInventory!");
            }
        }

        private void BuildBehaviorTree()
        {
            // Flee Sequence
            var fleeSequence = new BTSequence(new List<BTNode>
            {
                BotConditions.ShouldFlee(),
                BotActions.FleeFromNearestEnemy()
            });

            // Combat Sequence
            var combatSequence = new BTSequence(new List<BTNode>
            {
                BotConditions.ShouldAttack(),
                BotActions.PursueAndAttack()
            });

            // Collect Sequence
            var collectSequence = new BTSequence(new List<BTNode>
            {
                BotConditions.ShouldCollect(),
                BotActions.MoveToLootBox()
            });

            // Zone Survival Sequence (Highest priority in selector)
            var zoneSequence = new BTSequence(new List<BTNode>
            {
                BotConditions.IsOutsideZone(),
                BotActions.MoveToZoneCenter()
            });

            // Root Selector
            behaviorTree = new BTSelector(new List<BTNode>
            {
                zoneSequence,
                fleeSequence,
                combatSequence,
                collectSequence,
                BotActions.PatrolRandomPoint() // Fallback
            });
        }

        private void Update()
        {
            if (!Health.IsAlive)
            {
                Agent.isStopped = true;
                return;
            }

            // Ensure agent is never left in a stopped state from a previous action
            if (Agent.isStopped)
            {
                Agent.isStopped = false;
            }

            UpdateContext();
            
            // Fuzzy Logic Step
            float healthPerc = (Health.CurrentHealth / Health.maxHealth) * 100f;
            float enemyDist = context.NearestEnemy.HasValue ? context.NearestEnemy.Value.distance : 50f;
            
            // Calculate Threat (0-1). Being outside zone is max threat.
            float threat = 0f;
            if (ZoneManager.Instance != null && !ZoneManager.Instance.IsInsideZone(transform.position))
            {
                threat = 1f;
            }
            else if (context.NearestEnemy.HasValue && enemyDist < 5f)
            {
                threat = 0.8f;
            }

            context.FuzzyResult = fuzzyController.Evaluate(healthPerc, enemyDist, threat);

            // Behavior Tree Step
            behaviorTree.Tick(context);

            logTimer += Time.deltaTime;
            if (logTimer >= 2f)
            {
                logTimer = 0f;
                int enemyCount = 0;
                if (Sensor != null)
                {
                    foreach(var t in Sensor.VisibleTargets) if(t.type == TargetType.Enemy) enemyCount++;
                }
                string activeSpell = (Inventory != null && Inventory.GetActiveSpell() != null) ? Inventory.GetActiveSpell().spellName : "NONE";
                Debug.Log($"[BotController] {gameObject.name}: Enemies={enemyCount}, Fuzzy(A={context.FuzzyResult.attackScore:F2}, F={context.FuzzyResult.fleeScore:F2}, C={context.FuzzyResult.collectScore:F2}), Spell={activeSpell}");
            }

            if (AnimHandler != null)
            {
                float botSpeed = Agent.velocity.magnitude;
                bool botRunning = botSpeed > 6.5f; // Umbral para correr (Walk=5, Run=8)
                AnimHandler.UpdateLocomotion(botSpeed, botRunning);
            }
        }

        private void UpdateContext()
        {
            context.NearestEnemy = null;
            context.NearestLootBox = null;
            
            if (Sensor == null) return;

            float minEnemyDist = float.MaxValue;
            float minLootDist = float.MaxValue;

            foreach (var target in Sensor.VisibleTargets)
            {
                if (target.type == TargetType.Enemy && target.distance < minEnemyDist)
                {
                    minEnemyDist = target.distance;
                    context.NearestEnemy = target;
                }
                else if (target.type == TargetType.LootBox && target.distance < minLootDist)
                {
                    minLootDist = target.distance;
                    context.NearestLootBox = target;
                }
            }
        }
    }
}
