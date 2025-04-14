using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts
{
    public static class BoidHelper
    {
        const int numDirections = 300;

        public static readonly Vector3[] directions;

        static BoidHelper()
        {
            directions = new Vector3[numDirections]; 
            float ratio = (1 + Mathf.Sqrt(5)) / 2;

            for (int i=0; i<numDirections; i++)
            {
                float t = (float) i / numDirections;
                float inclination = Mathf.Acos(1 - 2 * t);
                float azimuth = ratio * Mathf.PI * 2*i;
                float x = Mathf.Sin(inclination) * Mathf.Cos(azimuth);
                float y = Mathf.Sin(inclination) * Mathf.Sin(azimuth);
                float z = Mathf.Cos(inclination);
                directions[i] = new Vector3(x, y, z);
            }
        }
    }
}
