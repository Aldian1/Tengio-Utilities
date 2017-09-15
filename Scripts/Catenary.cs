namespace Tengio
{
    using System;
    using UnityEngine;

    public class Catenary
    {
        private static Vector3[] emptyCurve = new Vector3[] { new Vector3(0.0f, 0.0f, 0.0f), new Vector3(0.0f, 0.0f, 0.0f) };

        private Vector3 startPoint;
        private Vector3 endPoint;
        private float slack;
        private int steps;
        private bool shouldRegenPoints;
        private Vector3[] points;

        public bool ShouldRegenPoints
        {
            get { return shouldRegenPoints; }
            set
            {
                shouldRegenPoints = value;
            }
        }

        public Vector3 StartPoint
        {
            get { return startPoint; }
            set
            {
                if (value != startPoint)
                    shouldRegenPoints = true;
                startPoint = value;
            }
        }

        public Vector3 EndPoint
        {
            get { return endPoint; }
            set
            {
                if (value != endPoint)
                    shouldRegenPoints = true;
                endPoint = value;
            }
        }
        public float Slack
        {
            get { return slack; }
            set
            {
                if (value != slack)
                    shouldRegenPoints = true;
                slack = Mathf.Max(0.0f, value);
            }
        }
        public int Steps
        {
            get { return steps; }
            set
            {
                if (value != steps)
                    shouldRegenPoints = true;
                steps = Mathf.Max(2, value);
            }
        }

        public Vector3 MidPoint
        {
            get
            {
                Vector3 mid = Vector3.zero;
                if (steps == 2)
                {
                    return (points[0] + points[1]) * 0.5f;
                }
                else if (steps > 2)
                {
                    int m = steps / 2;
                    if ((steps % 2) == 0)
                    {
                        mid = (points[m] + points[m + 1]) * 0.5f;
                    }
                    else
                    {
                        mid = points[m];
                    }
                }
                return mid;
            }
        }

        public Catenary(Vector3 start, Vector3 end, int steps)
        {
            points = emptyCurve;
            startPoint = start;
            endPoint = end;
            slack = 0.5f;
            this.steps = steps;
            shouldRegenPoints = true;
        }

        public Vector3[] GetPoints()
        {
            if (!shouldRegenPoints)
                return points;

            if (steps < 2)
                return emptyCurve;

            float lineDist = Vector3.Distance(endPoint, startPoint);
            float lineDistH = Vector3.Distance(new Vector3(endPoint.x, startPoint.y, endPoint.z), startPoint);
            float l = lineDist + Mathf.Max(0.0001f, slack);
            float r = 0.0f;
            float s = startPoint.y;
            float u = lineDistH;
            float v = EndPoint.y;

            if ((u - r) == 0.0f)
                return emptyCurve;

            float ztarget = Mathf.Sqrt(Mathf.Pow(l, 2.0f) - Mathf.Pow(v - s, 2.0f)) / (u - r);

            int loops = 30;
            int iterationCount = 0;
            int maxIterations = loops * 10; // For safety.
            bool found = false;

            float z = 0.0f;
            float ztest = 0.0f;
            float zstep = 100.0f;
            float ztesttarget = 0.0f;
            for (int i = 0; i < loops; i++)
            {
                for (int j = 0; j < 10; j++)
                {
                    iterationCount++;
                    ztest = z + zstep;
                    ztesttarget = (float)Math.Sinh(ztest) / ztest;

                    if (float.IsInfinity(ztesttarget))
                        continue;

                    if (ztesttarget == ztarget)
                    {
                        found = true;
                        z = ztest;
                        break;
                    }
                    else if (ztesttarget > ztarget)
                    {
                        break;
                    }
                    else
                    {
                        z = ztest;
                    }

                    if (iterationCount > maxIterations)
                    {
                        found = true;
                        break;
                    }
                }

                if (found)
                    break;

                zstep *= 0.1f;
            }

            float a = (u - r) / 2.0f / z;
            float p = (r + u - a * Mathf.Log((l + v - s) / (l - v + s))) / 2.0f;
            float q = (v + s - l * (float)Math.Cosh(z) / (float)Math.Sinh(z)) / 2.0f;

            points = new Vector3[steps];
            float stepsf = steps - 1;
            float stepf;
            for (int i = 0; i < steps; i++)
            {
                stepf = i / stepsf;
                Vector3 pos = Vector3.zero;
                pos.x = Mathf.Lerp(StartPoint.x, EndPoint.x, stepf);
                pos.z = Mathf.Lerp(StartPoint.z, EndPoint.z, stepf);
                pos.y = a * (float)Math.Cosh(((stepf * lineDistH) - p) / a) + q;
                points[i] = pos;
            }

            shouldRegenPoints = false;
            return points;
        }
    }
}