using System.Collections.Generic;
using UnityEngine;
using AnimalMagicRoyale.Core;
using AnimalMagicRoyale.Components;

namespace AnimalMagicRoyale.AI
{
    [RequireComponent(typeof(BotController))]
    [RequireComponent(typeof(AISensorSystem))]
    public class BotContextUpdater : MonoBehaviour
    {
        private BotController bot;
        private AISensorSystem sensor;
        
        private void Awake()
        {
            bot = GetComponent<BotController>();
            sensor = GetComponent<AISensorSystem>();
        }

        public void UpdateBotContext(BotContext context)
        {
            // 1. Limpieza de Targets nulos (Cofres/Hechizos destruidos este frame)
            if (sensor.VisibleTargets != null)
            {
                sensor.VisibleTargets.RemoveAll(t => t.transform == null || !t.transform.gameObject.activeInHierarchy);
            }

            // 2. Variables de Evaluación
            float bestLootScore = -float.MaxValue;
            Transform bestPhysicalLoot = null;
            MemorySpellTarget? bestMemoryLoot = null;

            float minEnemyDist = float.MaxValue;
            Transform nearestEnemy = null;

            // 3. Fusión de Sensores
            foreach (var target in sensor.VisibleTargets)
            {
                // Solo evaluamos si está dentro de la zona
                if (ZoneManager.Instance != null && !ZoneManager.Instance.IsInsideZone(target.transform.position))
                    continue;

                float dist = Vector3.Distance(transform.position, target.transform.position);

                if (target.type == TargetType.Enemy)
                {
                    if (dist < minEnemyDist)
                    {
                        minEnemyDist = dist;
                        nearestEnemy = target.transform;
                    }
                    
                    // Reportar a memoria
                    if (TeamMemorySystem.Instance != null && target.transform != null)
                    {
#pragma warning disable CS0618
                        TeamMemorySystem.Instance.ReportEnemy(context.TeamId, target.transform.gameObject.GetEntityId(), target.transform.position);
#pragma warning restore CS0618
                    }
                }
                else if (target.type == TargetType.LootBox)
                {
                    float score = 1000f - dist; // Alta prioridad
                    if (score > bestLootScore)
                    {
                        bestLootScore = score;
                        bestPhysicalLoot = target.transform;
                        bestMemoryLoot = null;
                    }
                }
                else if (target.type == TargetType.SpellPickup)
                {
                    var pickup = target.transform.GetComponent<SpellPickup>();
                    if (pickup != null && pickup.containedSpell != null)
                    {
                        int currentTier = bot.Inventory != null && bot.Inventory.GetActiveSpell() != null ? (int)bot.Inventory.GetActiveSpell().tier : -1;
                        int pickupTier = (int)pickup.containedSpell.tier;

                        if (pickupTier <= currentTier)
                        {
                            // Ignorar y reportar al equipo por si alguien lo quiere
                            if (TeamMemorySystem.Instance != null)
                            {
#pragma warning disable CS0618
                                TeamMemorySystem.Instance.ReportSpell(context.TeamId, target.transform.gameObject.GetEntityId(), target.transform.position, pickup.containedSpell.tier);
#pragma warning restore CS0618
                            }
                            continue;
                        }

                        float score = (pickupTier * 100f) - dist;
                        if (score > bestLootScore)
                        {
                            bestLootScore = score;
                            bestPhysicalLoot = target.transform;
                            bestMemoryLoot = null;
                        }
                    }
                }
            }

            // 4. Fusión de Memoria Loot (Solo si el físico no es muy superior)
            if (TeamMemorySystem.Instance != null)
            {
                var myTeamSpells = TeamMemorySystem.Instance.GetSpells(context.TeamId);
                foreach (var memSpell in myTeamSpells.Values)
                {
                    if (ZoneManager.Instance != null && !ZoneManager.Instance.IsInsideZone(memSpell.Position))
                        continue;

                    int currentTier = bot.Inventory != null && bot.Inventory.GetActiveSpell() != null ? (int)bot.Inventory.GetActiveSpell().tier : -1;
                    if ((int)memSpell.Tier > currentTier)
                    {
                        float dist = Vector3.Distance(transform.position, memSpell.Position);
                        float score = ((int)memSpell.Tier * 100f) - dist;

                        // Histéresis: Requiere ser un 10% mejor para cambiar de idea si ya íbamos a otro sitio físico
                        if (score > bestLootScore * 1.1f)
                        {
                            bestLootScore = score;
                            bestMemoryLoot = memSpell;
                            bestPhysicalLoot = null;
                        }
                    }
                }
            }

            // 5. Búsqueda de Memoria de Enemigo (Investigation)
            MemoryEnemyTarget? bestInvestigationTarget = null;
            if (nearestEnemy == null && TeamMemorySystem.Instance != null)
            {
                var myTeamEnemies = TeamMemorySystem.Instance.GetEnemies(context.TeamId);
                float minMemEnemyDist = float.MaxValue;
                foreach (var memEnemy in myTeamEnemies.Values)
                {
                    float dist = Vector3.Distance(transform.position, memEnemy.LastKnownPosition);
                    if (dist < minMemEnemyDist)
                    {
                        minMemEnemyDist = dist;
                        bestInvestigationTarget = memEnemy;
                    }
                }
            }

            // 6. Escribir al Contexto
            context.NearestEnemy = nearestEnemy;
            context.BestPhysicalLoot = bestPhysicalLoot;
            context.BestMemoryLoot = bestMemoryLoot;
            context.InvestigationTarget = bestInvestigationTarget;
        }
    }
}
