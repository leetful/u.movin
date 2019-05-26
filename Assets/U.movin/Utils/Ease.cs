using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace u.movin
{
    public static class Ease
    {
        public static Vector2[] Linear = new Vector2[2]{ new Vector2(1, 1), new Vector2(0, 0) };
        public static Vector2[] StrongInOut = new Vector2[2]{ new Vector2(0.7f, 0), new Vector2(0.3f, 1) };
        public static Vector2[] StrongOut = new Vector2[2]{ new Vector2(0.167f, 0.167f), new Vector2(0.3f, 1) };

        public static float CubicBezier(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, float p)
        {
            float v;

            if (p == p0.x)
            {
                v = 0;
            }
            else if (p == p3.x)
            {
                v = 1;
            }
            else
            {
                float a = -p0.x + 3 * p1.x - 3 * p2.x + p3.x;
                float b = 3 * p0.x - 6 * p1.x + 3 * p2.x;
                float c = -3 * p0.x + 3 * p1.x;
                float d = p0.x - p;
                float temp = SolveCubic(a, b, c, d);
                if (temp == -1) return -1;
                v = temp;
            }

            return Cubed(1 - v) * p0.y + 3 * v * Squared(1 - v) * p1.y + 3 * Squared(v) * (1 - v) * p2.y + Cubed(v) * p3.y;

        }

        public static float Cubed(float v) { return v * v * v; }
        public static float Squared(float v) { return v * v; }
        public static float CubicRoot(float v) { return Mathf.Pow(v, 1.0f / 3.0f); }

        public static float SolveCubic(float a, float b, float c, float d)
        {
            if (a == 0) return SolveQuadratic(b, c, d);
            if (d == 0) return 0;

            b /= a;
            c /= a;
            d /= a;
            float q = (3.0f * c - Squared(b)) / 9.0f;
            float r = (-27.0f * d + b * (9.0f * c - 2.0f * Squared(b))) / 54.0f;
            float disc = Cubed(q) + Squared(r);
            float term1 = b / 3.0f;

            if (disc > 0)
            {
                float s = r + Mathf.Sqrt(disc);
                s = (s < 0) ? -CubicRoot(-s) : CubicRoot(s);
                float t = r - Mathf.Sqrt(disc);
                t = (t < 0) ? -CubicRoot(-t) : CubicRoot(t);

                float result = -term1 + s + t;
                if (result >= 0 && result <= 1) return result;
            }
            else if (disc == 0)
            {
                float r13 = (r < 0) ? -CubicRoot(-r) : CubicRoot(r);

                float result = -term1 + 2.0f * r13;
                if (result >= 0 && result <= 1) return result;

                result = -(r13 + term1);
                if (result >= 0 && result <= 1) return result;
            }
            else
            {
                q = -q;
                float dum1 = q * q * q;
                dum1 = Mathf.Acos(r / Mathf.Sqrt(dum1));
                float r13 = 2.0f * Mathf.Sqrt(q);

                float result = -term1 + r13 * Mathf.Cos(dum1 / 3.0f);
                if (result >= 0 && result <= 1) return result;

                result = -term1 + r13 * Mathf.Cos((dum1 + 2.0f * Mathf.PI) / 3.0f);
                if (result >= 0 && result <= 1) return result;

                result = -term1 + r13 * Mathf.Cos((dum1 + 4.0f * Mathf.PI) / 3.0f);
                if (result >= 0 && result <= 1) return result;
            }

            return -1;
        }

        public static float SolveQuadratic(float a, float b, float c)
        {
            float result = (-b + Mathf.Sqrt(Squared(b) - 4 * a * c)) / (2 * a);
            if (result >= 0 && result <= 1) return result;

            result = (-b - Mathf.Sqrt(Squared(b) - 4 * a * c)) / (2 * a);
            if (result >= 0 && result <= 1) return result;

            return -1;
        }

    }
}