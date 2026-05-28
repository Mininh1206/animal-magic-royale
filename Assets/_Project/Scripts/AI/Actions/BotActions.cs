using UnityEngine;
using AnimalMagicRoyale.Core;
using AnimalMagicRoyale.Components;

namespace AnimalMagicRoyale.AI
{
    public static class BotActions
    {
        public static BTAction MoveToZoneCenter()
        {
            return new BTAction(ctx =>
            {
                if (ZoneManager.Instance != null)
                {
                    Vector3 center = ZoneManager.Instance.transform.position;
                    // Get slightly random point near center to avoid clustering
                    Vector3 randomOffset = new Vector3(Random.Range(-5f, 5f), 0, Random.Range(-5f, 5f));
                    ctx.Bot.Agent.speed = 8f; // Correr a zona segura
                    ctx.Bot.Agent.SetDestination(center + randomOffset);
                    return NodeStatus.Running; // Always running until condition (IsOutsideZone) becomes false
                }
                return NodeStatus.Failure;
            });
        }

        public static BTAction FleeFromNearestEnemy()
        {
            return new BTAction(ctx =>
            {
                if (!ctx.NearestEnemy.HasValue) return NodeStatus.Failure;
                
                // Si el agente ya tiene un camino activo y no ha llegado, continuar
                if (!ctx.Bot.Agent.pathPending && ctx.Bot.Agent.hasPath && ctx.Bot.Agent.remainingDistance > 2f)
                {
                    return NodeStatus.Running;
                }
                
                // Calcular nuevo destino de huida
                Vector3 enemyPos = ctx.NearestEnemy.Value.transform.position;
                Vector3 fleeDir = (ctx.Bot.transform.position - enemyPos).normalized;
                
                // Calculate destination
                Vector3 fleeDest = ctx.Bot.transform.position + fleeDir * 10f;
                
                // Keep inside zone if possible
                if (ZoneManager.Instance != null && !ZoneManager.Instance.IsInsideZone(fleeDest))
                {
                    // Steer towards zone center if fleeing pushes us out
                    Vector3 centerDir = (ZoneManager.Instance.transform.position - ctx.Bot.transform.position).normalized;
                    fleeDest = ctx.Bot.transform.position + (fleeDir + centerDir).normalized * 10f;
                }

                ctx.Bot.Agent.isStopped = false;
                ctx.Bot.Agent.speed = 8f; // Correr
                ctx.Bot.Agent.SetDestination(fleeDest);
                return NodeStatus.Running;
            });
        }

        public static BTAction MoveTowardsEnemy()
        {
            return new BTAction(ctx =>
            {
                if (!ctx.NearestEnemy.HasValue) return NodeStatus.Failure;
                
                Transform target = ctx.NearestEnemy.Value.transform;
                float dist = Vector3.Distance(ctx.Bot.transform.position, target.position);
                
                // Si está lo suficientemente cerca para atacar, detenerse
                if (dist <= ctx.Bot.attackRange)
                {
                    ctx.Bot.Agent.isStopped = true;
                    return NodeStatus.Success;
                }
                
                // Sino, acercarse
                ctx.Bot.Agent.isStopped = false;
                ctx.Bot.Agent.speed = 5f; // Acercarse andando
                ctx.Bot.Agent.SetDestination(target.position);
                return NodeStatus.Running;
            });
        }

        public static BTAction TryCastSpell()
        {
            return new BTAction(ctx =>
            {
                if (!ctx.NearestEnemy.HasValue) return NodeStatus.Failure;
                
                Transform target = ctx.NearestEnemy.Value.transform;
                Vector3 dirToTarget = (target.position - ctx.Bot.transform.position).normalized;
                dirToTarget.y = 0; // Apuntar horizontalmente por ahora

                // Rotate towards target manually since agent might be stopped
                if (dirToTarget.sqrMagnitude > 0)
                {
                    Quaternion lookRot = Quaternion.LookRotation(dirToTarget);
                    ctx.Bot.transform.rotation = Quaternion.Slerp(ctx.Bot.transform.rotation, lookRot, Time.deltaTime * 10f);
                }

                if (ctx.Bot.Inventory != null)
                {
                    // Buscar un slot que tenga hechizo y no esté en cooldown
                    for (int i = 0; i < ctx.Bot.Inventory.slots.Length; i++)
                    {
                        var slot = ctx.Bot.Inventory.slots[i];
                        if (!slot.IsEmpty && !slot.IsOnCooldown)
                        {
                            ctx.Bot.Inventory.SelectSlot(i);
                            break;
                        }
                    }

                    // Randomize aim slightly based on bot precision could go here
                    // Try to cast the currently active spell
                    if (ctx.Bot.Inventory.TryCast(ctx.Bot.gameObject, dirToTarget))
                    {
                        // Maybe switch spells occasionally if we have multiple
                        if (Random.value < 0.1f)
                        {
                            int newSlot = Random.Range(0, 3);
                            if (!ctx.Bot.Inventory.slots[newSlot].IsEmpty)
                            {
                                ctx.Bot.Inventory.SelectSlot(newSlot);
                            }
                        }
                        return NodeStatus.Success;
                    }
                }
                
                return NodeStatus.Running; // Waiting for cooldown
            });
        }

        public static BTAction MoveToLootBox()
        {
            return new BTAction(ctx =>
            {
                if (!ctx.NearestLootBox.HasValue) return NodeStatus.Failure;
                
                Transform box = ctx.NearestLootBox.Value.transform;
                float dist = Vector3.Distance(ctx.Bot.transform.position, box.position);
                
                if (dist <= 2f) // Interaction range
                {
                    ctx.Bot.Agent.isStopped = true;
                    var lootBox = box.GetComponent<LootBox>();
                    if (lootBox != null)
                    {
                        if (lootBox.TryOpen(ctx.Bot.gameObject))
                        {
                            return NodeStatus.Success; // Opened
                        }
                    }
                    return NodeStatus.Failure; // Couldn't open or already open
                }
                
                ctx.Bot.Agent.isStopped = false;
                ctx.Bot.Agent.speed = 5f; // Andar a caja
                ctx.Bot.Agent.SetDestination(box.position);
                return NodeStatus.Running;
            });
        }

        public static BTAction PatrolRandomPoint()
        {
            return new BTAction(ctx =>
            {
                // Siempre asegurarnos de que el agente no esté detenido si está en modo patrulla
                if (ctx.Bot.Agent.isStopped) 
                {
                    ctx.Bot.Agent.isStopped = false;
                }

                // Si ha llegado a su destino o no tiene un camino pendiente
                if (!ctx.Bot.Agent.pathPending && ctx.Bot.Agent.remainingDistance < 1.5f)
                {
                    Vector3 center = ctx.Bot.transform.position;
                    if (ZoneManager.Instance != null)
                    {
                        center = ZoneManager.Instance.transform.position;
                    }
                    
                    Vector2 rand = Random.insideUnitCircle * 20f;
                    Vector3 dest = center + new Vector3(rand.x, 0, rand.y);
                    
                    // Comprobar que el punto es válido en el NavMesh
                    if (UnityEngine.AI.NavMesh.SamplePosition(dest, out UnityEngine.AI.NavMeshHit hit, 10f, UnityEngine.AI.NavMesh.AllAreas))
                    {
                        ctx.Bot.Agent.speed = 5f; // Andar patrullando
                        ctx.Bot.Agent.SetDestination(hit.position);
                    }
                }
                return NodeStatus.Running;
            });
        }
    }
}
