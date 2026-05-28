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
                return ctx.FuzzyResult.attackScore > ctx.FuzzyResult.collectScore && 
                       ctx.NearestEnemy.HasValue;
            });
        }

        public static BTCondition ShouldCollect()
        {
            return new BTCondition(ctx => 
            {
                return ctx.FuzzyResult.collectScore > 0.3f && 
                       ctx.NearestLootBox.HasValue;
            });
        }

        public static BTCondition HasEnemyTarget()
        {
            return new BTCondition(ctx => ctx.NearestEnemy.HasValue);
        }

        public static BTCondition HasLootTarget()
        {
            return new BTCondition(ctx => ctx.NearestLootBox.HasValue);
        }
    }
}
