using UnityEngine;

namespace AnimalMagicRoyale.AI
{
    /// <summary>
    /// Define una función de pertenencia trapezoidal/triangular para un conjunto difuso.
    /// Puntos: a <= b <= c <= d.
    /// Si b == c, es un triángulo.
    /// </summary>
    public class FuzzySet
    {
        public string Name { get; private set; }
        public float A { get; private set; }
        public float B { get; private set; }
        public float C { get; private set; }
        public float D { get; private set; }

        public FuzzySet(string name, float a, float b, float c, float d)
        {
            Name = name;
            A = a;
            B = b;
            C = c;
            D = d;
        }

        public float Evaluate(float value)
        {
            if (value <= A || value >= D) return 0f;
            if (value >= B && value <= C) return 1f;
            
            if (value > A && value < B)
            {
                return (value - A) / (B - A);
            }
            if (value > C && value < D)
            {
                return (D - value) / (D - C);
            }

            return 0f;
        }
    }
}
