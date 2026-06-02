using UnityEngine;
using AnimalMagicRoyale.Core;
using AnimalMagicRoyale.Components;
using AnimalMagicRoyale.Components.Abilities;

namespace AnimalMagicRoyale.AI
{
    public static class BotActions
    {
        public static BTAction MoveToZoneCenter()
        {
            return new BTAction(ctx =>
            {
                if (ZoneManager.Instance != null && ctx.Bot.Agent.isOnNavMesh && ctx.Bot.Agent.isActiveAndEnabled)
                {
                    Vector3 center = ZoneManager.Instance.ZoneCenter;
                    Vector3 randomOffset = new Vector3(Random.Range(-5f, 5f), 0, Random.Range(-5f, 5f));
                    if (ctx.Bot.Agent.isStopped) ctx.Bot.Agent.isStopped = false;
                    ctx.Bot.Agent.speed = 8f;
                    ctx.Bot.Agent.SetDestination(center + randomOffset);
                    return NodeStatus.Running;
                }
                return NodeStatus.Failure;
            });
        }

        public static BTAction FleeFromNearestEnemy()
        {
            return new BTAction(ctx =>
            {
                if (ctx.NearestEnemy == null) return NodeStatus.Failure;
                
                if (!ctx.Bot.Agent.isOnNavMesh || !ctx.Bot.Agent.isActiveAndEnabled) return NodeStatus.Failure;

                var abilityHolder = ctx.Bot.GetComponent<AbilityHolder>();
                if (abilityHolder != null && abilityHolder.IsReady)
                {
                    abilityHolder.TryActivate();
                    // Debug.Log($"[BotAI] {ctx.Bot.gameObject.name} usó habilidad evasiva para huir.");
                }

                Vector3 enemyPos = ctx.NearestEnemy.position;
                Vector3 fleeDir = (ctx.Bot.transform.position - enemyPos).normalized;
                Vector3 fleeDest = ctx.Bot.transform.position + fleeDir * 15f;
                
                if (ZoneManager.Instance != null && !ZoneManager.Instance.IsInsideZone(fleeDest))
                {
                    Vector3 centerDir = (ZoneManager.Instance.ZoneCenter - ctx.Bot.transform.position).normalized;
                    fleeDest = ctx.Bot.transform.position + (fleeDir + centerDir).normalized * 15f;
                }

                if (UnityEngine.AI.NavMesh.SamplePosition(fleeDest, out UnityEngine.AI.NavMeshHit hit, 5f, UnityEngine.AI.NavMesh.AllAreas))
                {
                    fleeDest = hit.position;
                }

                if (ctx.Bot.Agent.isStopped) ctx.Bot.Agent.isStopped = false;
                ctx.Bot.Agent.speed = 8f;
                ctx.Bot.Agent.SetDestination(fleeDest);

                // Kiting
                if (ctx.Bot.Inventory != null && Time.time >= ctx.NextAttackTime)
                {
                    bool canShoot = false;
                    for (int i = 0; i < ctx.Bot.Inventory.slots.Length; i++)
                    {
                        if (!ctx.Bot.Inventory.slots[i].IsEmpty && !ctx.Bot.Inventory.slots[i].IsOnCooldown)
                        {
                            ctx.Bot.Inventory.SelectSlot(i);
                            canShoot = true;
                            break;
                        }
                    }

                    if (canShoot)
                    {
                        float dist = Vector3.Distance(ctx.Bot.transform.position, enemyPos);
                        if (dist > 8f)
                        {
                            float targetCenterYOffset = 1f;
                            var targetCC = ctx.NearestEnemy.GetComponentInParent<CharacterController>();
                            if (targetCC != null) targetCenterYOffset = targetCC.height * 0.35f;
                            else 
                            {
                                var targetAgent = ctx.NearestEnemy.GetComponentInParent<UnityEngine.AI.NavMeshAgent>();
                                if (targetAgent != null) targetCenterYOffset = targetAgent.height * 0.35f;
                            }

                            Vector3 targetCenter = enemyPos + Vector3.up * targetCenterYOffset;
                            Vector3 firePos = ctx.Bot.Inventory.FirePoint != null 
                                              ? ctx.Bot.Inventory.FirePoint.position 
                                              : ctx.Bot.transform.position + Vector3.up * 1f;

                            Vector3 aimDir = (targetCenter - firePos).normalized;
                            
                            Vector3 lookDir = aimDir;
                            lookDir.y = 0;
                            if (lookDir.sqrMagnitude > 0)
                            {
                                ctx.Bot.transform.rotation = Quaternion.LookRotation(lookDir);
                            }
                            
                            if (ctx.Bot.Inventory.TryCast(ctx.Bot.gameObject, aimDir))
                            {
                                ctx.NextAttackTime = Time.time + 1.5f;
                            }
                        }
                    }
                }

                return NodeStatus.Running;
            });
        }

        public static BTAction PursueAndAttack()
        {
            return new BTAction(ctx =>
            {
                if (ctx.NearestEnemy == null) return NodeStatus.Failure;
                if (!ctx.Bot.Agent.isOnNavMesh || !ctx.Bot.Agent.isActiveAndEnabled) return NodeStatus.Failure;

                Transform target = ctx.NearestEnemy;
                float dist = Vector3.Distance(ctx.Bot.transform.position, target.position);
                
                Vector3 targetVelocity = Vector3.zero;
                float targetCenterYOffset = 1f;
                var targetCC = target.GetComponentInParent<CharacterController>();
                if (targetCC != null) 
                {
                    targetVelocity = targetCC.velocity;
                    // El centro geométrico puede quedar muy alto por la cabeza/cuello. 
                    // Apuntamos al torso (35% de la altura total) para no fallar por encima.
                    targetCenterYOffset = targetCC.height * 0.35f;
                }
                else 
                {
                    var targetAgent = target.GetComponentInParent<UnityEngine.AI.NavMeshAgent>();
                    if (targetAgent != null) 
                    {
                        targetVelocity = targetAgent.velocity;
                        targetCenterYOffset = targetAgent.height * 0.35f;
                    }
                }

                float projSpeed = 20f;
                bool isSelfCast = false;
                bool hasBounces = false;

                var activeSpellData = ctx.Bot.Inventory?.GetActiveSpell();
                if (activeSpellData != null)
                {
                    projSpeed = activeSpellData.projectileSpeed;
                    isSelfCast = activeSpellData.targetType == Spells.TargetType.Self;
                    hasBounces = activeSpellData.maxBounces > 0;
                }

                float timeToTarget = dist / Mathf.Max(projSpeed, 1f);
                Vector3 firePos = ctx.Bot.Inventory != null && ctx.Bot.Inventory.FirePoint != null 
                                  ? ctx.Bot.Inventory.FirePoint.position 
                                  : ctx.Bot.transform.position + Vector3.up * 1f;

                Vector3 predictedPos = target.position + (targetVelocity * timeToTarget);
                // Aim at the target's actual center using its character controller offset
                Vector3 targetCenter = predictedPos + Vector3.up * targetCenterYOffset;

                Vector3 aimDir = (targetCenter - firePos).normalized;

                if (!isSelfCast)
                {
                    Vector3 lookDir = aimDir;
                    lookDir.y = 0;
                    if (lookDir.sqrMagnitude > 0)
                    {
                        Quaternion lookRot = Quaternion.LookRotation(lookDir);
                        ctx.Bot.transform.rotation = Quaternion.Slerp(ctx.Bot.transform.rotation, lookRot, Time.deltaTime * 10f);
                    }
                }

                if (dist > ctx.Bot.attackRange)
                {
                    if (ctx.Bot.Agent.isStopped) ctx.Bot.Agent.isStopped = false;
                    ctx.Bot.Agent.speed = 5f;
                    ctx.Bot.Agent.SetDestination(target.position);
                }
                else
                {
                    if (ctx.Bot.Agent.isStopped) ctx.Bot.Agent.isStopped = false;
                    ctx.Bot.Agent.speed = 5f;
                    
                    if (!ctx.Bot.Agent.pathPending && ctx.Bot.Agent.remainingDistance < 1f)
                    {
                        Vector3 right = Vector3.Cross(aimDir, Vector3.up).normalized;
                        float sign = Random.value > 0.5f ? 1f : -1f;
                        Vector3 strafeDest = ctx.Bot.transform.position + (right * sign * 4f);
                        
                        if (ZoneManager.Instance != null && ZoneManager.Instance.IsInsideZone(strafeDest))
                        {
                            if (UnityEngine.AI.NavMesh.SamplePosition(strafeDest, out UnityEngine.AI.NavMeshHit hit, 5f, UnityEngine.AI.NavMesh.AllAreas))
                            {
                                ctx.Bot.Agent.SetDestination(hit.position);
                            }
                        }
                    }
                }

                if (ctx.Bot.Inventory != null && Time.time >= ctx.NextAttackTime)
                {
                    for (int i = 0; i < ctx.Bot.Inventory.slots.Length; i++)
                    {
                        var slot = ctx.Bot.Inventory.slots[i];
                        if (!slot.IsEmpty && !slot.IsOnCooldown)
                        {
                            ctx.Bot.Inventory.SelectSlot(i);
                            break;
                        }
                    }
                    
                    Vector3 finalAimDir = isSelfCast ? ctx.Bot.transform.forward : aimDir;
                    bool castSuccess = ctx.Bot.Inventory.TryCast(ctx.Bot.gameObject, finalAimDir);
                    if (castSuccess)
                    {
                        ctx.NextAttackTime = Time.time + 1.0f;
                    }
                }

                var abilityHolder = ctx.Bot.GetComponent<AbilityHolder>();
                if (abilityHolder != null && abilityHolder.IsReady && dist < 5f)
                {
                    abilityHolder.TryActivate();
                }

                return NodeStatus.Running;
            });
        }

        public static BTAction MoveToLoot()
        {
            return new BTAction(ctx =>
            {
                // CASO A: Target Físico
                if (ctx.BestPhysicalLoot != null)
                {
                    var lootBox = ctx.BestPhysicalLoot.GetComponentInParent<LootBox>();
                    var pickup = ctx.BestPhysicalLoot.GetComponentInParent<SpellPickup>();
                    
                    Transform targetRoot = lootBox != null ? lootBox.transform : (pickup != null ? pickup.transform : ctx.BestPhysicalLoot);
                    float dist = Vector3.Distance(ctx.Bot.transform.position, targetRoot.position);
                    
                    if (dist <= 3f)
                    {
                        if (ctx.Bot.Agent.isOnNavMesh && ctx.Bot.Agent.isActiveAndEnabled) ctx.Bot.Agent.isStopped = true;
                        
                        // Hacer lo mismo que el jugador: OverlapSphere
                        var colliders = Physics.OverlapSphere(ctx.Bot.transform.position, 3f);
                        bool interacted = false;

                        foreach (var col in colliders)
                        {
                            var lb = col.GetComponent<LootBox>();
                            if (lb != null)
                            {
                                if (lb.TryOpen(ctx.Bot.gameObject))
                                {
                                    interacted = true;
                                    break;
                                }
                            }
                            
                            var p = col.GetComponent<SpellPickup>();
                            if (p != null && p.containedSpell != null)
                            {
                                int targetSlot = -1;
                                int lowestTier = int.MaxValue;
                                
                                for (int i = 0; i < ctx.Bot.Inventory.slots.Length; i++)
                                {
                                    if (ctx.Bot.Inventory.slots[i].IsEmpty)
                                    {
                                        targetSlot = i;
                                        break;
                                    }
                                    else if ((int)ctx.Bot.Inventory.slots[i].spellData.tier < lowestTier)
                                    {
                                        lowestTier = (int)ctx.Bot.Inventory.slots[i].spellData.tier;
                                        targetSlot = i;
                                    }
                                }
                                
                                if (targetSlot != -1)
                                {
                                    ctx.Bot.Inventory.SelectSlot(targetSlot);
                                    if (p.TryPickup(ctx.Bot.gameObject))
                                    {
                                        if (TeamMemorySystem.Instance != null)
#pragma warning disable CS0618
                                            TeamMemorySystem.Instance.RemoveSpell(ctx.TeamId, p.gameObject.GetEntityId());
#pragma warning restore CS0618
                                        interacted = true;
                                        break;
                                    }
                                }
                            }
                        }

                        if (interacted) return NodeStatus.Success;
                        return NodeStatus.Failure;
                    }
                    
                    if (ctx.Bot.Agent.isOnNavMesh && ctx.Bot.Agent.isActiveAndEnabled)
                    {
                        ctx.Bot.Agent.isStopped = false;
                        ctx.Bot.Agent.speed = 5f;
                        ctx.Bot.Agent.SetDestination(targetRoot.position);
                    }
                    return NodeStatus.Running;
                }
                
                // CASO B: Target en Memoria
                if (ctx.BestMemoryLoot.HasValue)
                {
                    Vector3 dest = ctx.BestMemoryLoot.Value.Position;
                    float dist = Vector3.Distance(ctx.Bot.transform.position, dest);
                    
                    if (dist < 2.0f)
                    {
                        // Check if sensor sees the spell here
                        bool spellFound = false;
                        foreach (var target in ctx.Sensor.VisibleTargets)
                        {
                            if (target.type == TargetType.SpellPickup && Vector3.Distance(target.transform.position, dest) < 1f)
                            {
                                spellFound = true;
                                break;
                            }
                        }

                        if (!spellFound)
                        {
                            // Alguien se lo llevó o despawneó
                            if (TeamMemorySystem.Instance != null)
                            {
                                TeamMemorySystem.Instance.RemoveSpell(ctx.TeamId, ctx.BestMemoryLoot.Value.InstanceID);
                            }
                            // Debug.Log($"[BotAI] {ctx.Bot.gameObject.name} llegó a la posición de memoria pero el hechizo no estaba.");
                            return NodeStatus.Failure;
                        }
                    }

                    if (ctx.Bot.Agent.isOnNavMesh && ctx.Bot.Agent.isActiveAndEnabled)
                    {
                        ctx.Bot.Agent.isStopped = false;
                        ctx.Bot.Agent.speed = 5f;
                        ctx.Bot.Agent.SetDestination(dest);
                    }
                    return NodeStatus.Running;
                }

                return NodeStatus.Failure;
            });
        }

        public static BTAction Investigate()
        {
            return new BTAction(ctx =>
            {
                if (!ctx.InvestigationTarget.HasValue) return NodeStatus.Failure;

                Vector3 dest = ctx.InvestigationTarget.Value.LastKnownPosition;
                float dist = Vector3.Distance(ctx.Bot.transform.position, dest);

                if (dist < 2.0f)
                {
                    // Llego y giro un poco
                    ctx.Bot.transform.Rotate(0, 90 * Time.deltaTime, 0);
                    
                    // Como pasará el tiempo, el cleanup del TeamMemory lo borrará.
                    // Pero podemos forzarlo si queremos.
                    return NodeStatus.Running;
                }

                if (ctx.Bot.Agent.isOnNavMesh && ctx.Bot.Agent.isActiveAndEnabled)
                {
                    ctx.Bot.Agent.isStopped = false;
                    ctx.Bot.Agent.speed = 5f;
                    ctx.Bot.Agent.SetDestination(dest);
                }
                return NodeStatus.Running;
            });
        }

        public static BTAction PatrolRandomPoint()
        {
            return new BTAction(ctx =>
            {
                if (!ctx.Bot.Agent.isOnNavMesh || !ctx.Bot.Agent.isActiveAndEnabled) return NodeStatus.Failure;

                if (ctx.Bot.Agent.isStopped) 
                {
                    ctx.Bot.Agent.isStopped = false;
                    ctx.Bot.Agent.ResetPath();
                }

                if (!ctx.Bot.Agent.pathPending && ctx.Bot.Agent.remainingDistance < 1.5f)
                {
                    Vector3 center = ctx.Bot.transform.position;
                    float patrolRadius = 15f;
                    if (ZoneManager.Instance != null && ZoneManager.Instance.IsActive)
                    {
                        center = ZoneManager.Instance.ZoneCenter;
                        patrolRadius = Mathf.Max(10f, (ZoneManager.Instance.CurrentDiameter / 2f) * 0.7f);
                    }
                    
                    Vector2 rand = Random.insideUnitCircle * patrolRadius;
                    Vector3 dest = center + new Vector3(rand.x, 0, rand.y);
                    
                    if (ZoneManager.Instance != null && !ZoneManager.Instance.IsInsideZone(dest))
                    {
                        dest = center; // Ir al centro si random es fuera
                    }

                    if (UnityEngine.AI.NavMesh.SamplePosition(dest, out UnityEngine.AI.NavMeshHit hit, patrolRadius, UnityEngine.AI.NavMesh.AllAreas))
                    {
                        ctx.Bot.Agent.speed = 5f;
                        ctx.Bot.Agent.SetDestination(hit.position);
                    }
                }
                return NodeStatus.Running;
            });
        }
    }
}
