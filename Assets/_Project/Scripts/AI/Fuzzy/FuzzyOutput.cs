using UnityEngine;

namespace AnimalMagicRoyale.AI
{
    public struct FuzzyOutput
    {
        public float attackScore;
        public float fleeScore;
        public float collectScore;

        public FuzzyOutput(float attack, float flee, float collect)
        {
            attackScore = attack;
            fleeScore = flee;
            collectScore = collect;
        }
    }
}
