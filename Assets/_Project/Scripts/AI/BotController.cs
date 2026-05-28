using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using AnimalMagicRoyale.Components;
using AnimalMagicRoyale.Core;

namespace AnimalMagicRoyale.AI
{
    [RequireComponent(typeof(NavMeshAgent))]
    [RequireComponent(typeof(HealthComponent))]
    [RequireComponent(typeof(SpellInventory))]
    [RequireComponent(typeof(AISensorSystem))]
    [RequireComponent(typeof(CharacterAnimationHandler))]
    public class BotController : MonoBehaviour
    {
        public float attackRange = 10f;
        
        public NavMeshAgent Agent { get; private set; }
        public HealthComponent Health { get; private set; }
        public SpellInventory Inventory { get; private set; }
        public AISensorSystem Sensor { get; private set; }

        private FuzzyController fuzzyController;
        private BTNode behaviorTree;
        private BotContext context;
        private CharacterAnimationHandler _animHandler;

        private void Awake()
        {
            Agent = GetComponent<NavMeshAgent>();
            Health = GetComponent<HealthComponent>();
            Inventory = GetComponent<SpellInventory>();
            Sensor = GetComponent<AISensorSystem>();

            fuzzyController = new FuzzyController();
            context = new BotContext { Bot = this, Sensor = Sensor };
            _animHandler = GetComponent<CharacterAnimationHandler>();
            
            BuildBehaviorTree();
        }

        private void Start()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.RegisterPlayer(gameObject);
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

            // Attack Sequence
            var attackSequence = new BTSequence(new List<BTNode>
            {
                BotConditions.ShouldAttack(),
                BotActions.MoveTowardsEnemy(),
                BotActions.TryCastSpell()
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
                attackSequence,
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

            UpdateContext();
            
            // Fuzzy Logic Step
            float healthPerc = (Health.CurrentHealth / Health.maxHealth) * 100f;
            float enemyDist = context.NearestEnemy.HasValue ? context.NearestEnemy.Value.distance : 100f;
            
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

            if (_animHandler != null)
            {
                float botSpeed = Agent.velocity.magnitude;
                bool botRunning = botSpeed > 6.5f; // Umbral para correr (Walk=5, Run=8)
                _animHandler.UpdateLocomotion(botSpeed, botRunning);
            }
        }

        private void UpdateContext()
        {
            context.NearestEnemy = null;
            context.NearestLootBox = null;
            
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
