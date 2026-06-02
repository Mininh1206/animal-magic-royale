using UnityEngine;
using AnimalMagicRoyale.Core;

namespace AnimalMagicRoyale.AI
{
    public static class BotConditions
    {
        public static BTCondition IsOutsideZone()
        {
            return new BTCondition(ctx => 
            {
                if (ZoneManager.Instance == null) return false;
                return !ZoneManager.Instance.IsInsideZone(ctx.Bot.transform.position);
            });
        }

        public static BTCondition ShouldFlee()
        {
            return new BTCondition(ctx => 
            {
                // Flee if score is highest and significant
                return ctx.FuzzyResult.fleeScore > ctx.FuzzyResult.attackScore && 
                       ctx.FuzzyResult.fleeScore > 0.4f;
            });
        }

        public static BTCondition ShouldAttack()
        {
            return new BTCondition(ctx => 
            {
                return ctx.NearestEnemy != null && 
                       ctx.FuzzyResult.attackScore > 0.2f &&
                       ctx.FuzzyResult.attackScore >= ctx.FuzzyResult.fleeScore;
            });
        }

        public static BTCondition ShouldInvestigate()
        {
            return new BTCondition(ctx => 
            {
                return ctx.NearestEnemy == null && 
                       ctx.InvestigationTarget.HasValue;
            });
        }

        public static BTCondition ShouldCollect()
        {
            return new BTCondition(ctx => 
            {
                return ctx.FuzzyResult.collectScore > 0.3f && 
                       (ctx.BestPhysicalLoot != null || ctx.BestMemoryLoot.HasValue);
            });
        }

        public static BTCondition HasEnemyTarget()
        {
            return new BTCondition(ctx => ctx.NearestEnemy != null);
        }

        public static BTCondition HasLootTarget()
        {
            return new BTCondition(ctx => ctx.BestPhysicalLoot != null || ctx.BestMemoryLoot.HasValue);
        }
    }
}
