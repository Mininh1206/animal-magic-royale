using System.Collections.Generic;
using UnityEngine;

namespace AnimalMagicRoyale.AI
{
    public class FuzzyCondition
    {
        public string VariableName { get; private set; }
        public FuzzySet Set { get; private set; }

        public FuzzyCondition(string variableName, FuzzySet set)
        {
            VariableName = variableName;
            Set = set;
        }
    }

    public class FuzzyRule
    {
        public List<FuzzyCondition> Conditions { get; private set; }
        // Salidas: Accion -> Peso (ej. "Attack" -> 0.9f)
        public Dictionary<string, float> Conclusions { get; private set; }

        public FuzzyRule()
        {
            Conditions = new List<FuzzyCondition>();
            Conclusions = new Dictionary<string, float>();
        }

        public FuzzyRule AddCondition(string variableName, FuzzySet set)
        {
            Conditions.Add(new FuzzyCondition(variableName, set));
            return this;
        }

        public FuzzyRule AddConclusion(string action, float weight)
        {
            Conclusions[action] = weight;
            return this;
        }

        /// <summary>
        /// Evalúa la regla usando el operador AND (mínimo) para las condiciones.
        /// Retorna el grado de activación de la regla.
        /// </summary>
        public float Evaluate(Dictionary<string, float> crispInputs)
        {
            float activation = 1f;

            foreach (var condition in Conditions)
            {
                if (crispInputs.TryGetValue(condition.VariableName, out float crispValue))
                {
                    float membership = condition.Set.Evaluate(crispValue);
                    activation = Mathf.Min(activation, membership); // AND lógico
                }
                else
                {
                    // Si falta una variable, la regla no se activa
                    return 0f;
                }
            }

            return activation;
        }
    }
}
