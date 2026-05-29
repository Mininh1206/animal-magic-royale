using System.Collections.Generic;
using UnityEngine;

namespace AnimalMagicRoyale.AI
{
    public class FuzzyController
    {
        private List<FuzzyRule> rules = new List<FuzzyRule>();
        
        // Sets para Vida (0-100)
        public FuzzySet HealthCritical = new FuzzySet("Critical", -10, 0, 20, 40);
        public FuzzySet HealthMedium = new FuzzySet("Medium", 20, 40, 60, 80);
        public FuzzySet HealthHigh = new FuzzySet("High", 60, 80, 100, 110);

        // Sets para Distancia Enemigo (0-50)
        public FuzzySet EnemyClose = new FuzzySet("Close", -5, 0, 8, 15);
        public FuzzySet EnemyMedium = new FuzzySet("Medium", 10, 18, 25, 35);
        public FuzzySet EnemyFar = new FuzzySet("Far", 25, 35, 100, 110);

        // Sets para Amenaza Percibida (0-1) (0 = sin amenaza, 1 = zona u otro peligro extremo)
        public FuzzySet ThreatLow = new FuzzySet("Low", -0.1f, 0f, 0.2f, 0.4f);
        public FuzzySet ThreatMedium = new FuzzySet("Medium", 0.2f, 0.4f, 0.6f, 0.8f);
        public FuzzySet ThreatHigh = new FuzzySet("High", 0.6f, 0.8f, 1f, 1.1f);

        public FuzzyController()
        {
            InitializeRules();
        }

        private void InitializeRules()
        {
            // IF Health=Critical AND EnemyDistance=Close THEN Flee=1.0, Attack=0.1
            rules.Add(new FuzzyRule()
                .AddCondition("Health", HealthCritical)
                .AddCondition("EnemyDistance", EnemyClose)
                .AddConclusion("Flee", 1.0f)
                .AddConclusion("Attack", 0.1f));

            // IF Health=High AND EnemyDistance=Close THEN Attack=1.0, Flee=0.0
            rules.Add(new FuzzyRule()
                .AddCondition("Health", HealthHigh)
                .AddCondition("EnemyDistance", EnemyClose)
                .AddConclusion("Attack", 1.0f)
                .AddConclusion("Flee", 0.0f));

            // IF Health=Medium AND EnemyDistance=Medium THEN Attack=0.6, Collect=0.4
            rules.Add(new FuzzyRule()
                .AddCondition("Health", HealthMedium)
                .AddCondition("EnemyDistance", EnemyMedium)
                .AddConclusion("Attack", 0.6f)
                .AddConclusion("Collect", 0.4f));

            // IF Health=High AND EnemyDistance=Far THEN Collect=0.8
            rules.Add(new FuzzyRule()
                .AddCondition("Health", HealthHigh)
                .AddCondition("EnemyDistance", EnemyFar)
                .AddConclusion("Collect", 0.8f));

            // IF Threat=High THEN Flee=1.0
            rules.Add(new FuzzyRule()
                .AddCondition("Threat", ThreatHigh)
                .AddConclusion("Flee", 1.0f));

            // IF Health=Critical AND EnemyDistance=Far THEN Flee=0.5, Collect=0.5
            rules.Add(new FuzzyRule()
                .AddCondition("Health", HealthCritical)
                .AddCondition("EnemyDistance", EnemyFar)
                .AddConclusion("Flee", 0.5f)
                .AddConclusion("Collect", 0.5f));

            // NUEVA: Cuando no hay enemigo cerca o hay poca amenaza, priorizar recolección
            rules.Add(new FuzzyRule()
                .AddCondition("Threat", ThreatLow)
                .AddCondition("EnemyDistance", EnemyFar)
                .AddConclusion("Collect", 0.7f));

            // NUEVA: Health media sin amenaza -> buscar loot
            rules.Add(new FuzzyRule()
                .AddCondition("Health", HealthMedium)
                .AddCondition("Threat", ThreatLow)
                .AddConclusion("Collect", 0.5f)
                .AddConclusion("Attack", 0.3f));

            // NUEVA: Health media, enemigo cerca -> atacar decididamente
            rules.Add(new FuzzyRule()
                .AddCondition("Health", HealthMedium)
                .AddCondition("EnemyDistance", EnemyClose)
                .AddConclusion("Attack", 0.9f)
                .AddConclusion("Flee", 0.1f));
        }

        public FuzzyOutput Evaluate(float health, float enemyDistance, float threat)
        {
            Dictionary<string, float> crispInputs = new Dictionary<string, float>
            {
                { "Health", health },
                { "EnemyDistance", enemyDistance },
                { "Threat", threat }
            };

            // Acumuladores para defuzzification (suma ponderada)
            float sumAttackWeights = 0f;
            float sumFleeWeights = 0f;
            float sumCollectWeights = 0f;

            float sumAttackActivations = 0f;
            float sumFleeActivations = 0f;
            float sumCollectActivations = 0f;

            foreach (var rule in rules)
            {
                float activation = rule.Evaluate(crispInputs);

                if (activation > 0f)
                {
                    if (rule.Conclusions.TryGetValue("Attack", out float attackVal))
                    {
                        sumAttackWeights += activation * attackVal;
                        sumAttackActivations += activation;
                    }
                    if (rule.Conclusions.TryGetValue("Flee", out float fleeVal))
                    {
                        sumFleeWeights += activation * fleeVal;
                        sumFleeActivations += activation;
                    }
                    if (rule.Conclusions.TryGetValue("Collect", out float collectVal))
                    {
                        sumCollectWeights += activation * collectVal;
                        sumCollectActivations += activation;
                    }
                }
            }

            float finalAttack = sumAttackActivations > 0 ? sumAttackWeights / sumAttackActivations : 0f;
            float finalFlee = sumFleeActivations > 0 ? sumFleeWeights / sumFleeActivations : 0f;
            float finalCollect = sumCollectActivations > 0 ? sumCollectWeights / sumCollectActivations : 0f;

            return new FuzzyOutput(finalAttack, finalFlee, finalCollect);
        }
    }
}
