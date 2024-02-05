using System;
using System.Collections.Generic;
using UnityEngine;

namespace Filo{
    public static class Utils
    {
        
        public static bool Catenary(Vector3 p1, Vector3 p2, float l, int samples, List<Vector3> points){

            points.Clear();

            Vector3 vector = p2 - p1;
            Vector3 dir = Vector3.Scale(vector, new Vector3(1, 0, 1));

            Quaternion rot = Quaternion.LookRotation(dir);
            Quaternion irot = Quaternion.Inverse(rot);
            Vector3 n = irot * vector;

            float r = 0;
            float s = 0;
            float u = n.z;
            float v = n.y;
    
            // swap values if p1 is to the right of p2:
            if (r > u){
                float temp = r;
                r = u;
                u = temp;
                temp = s;
                s = v;
                v = temp;
            }

            // find z:
            float z = 0.005f;
            float target = Mathf.Sqrt(l*l-(v-s)*(v-s))/(u-r);
            while((float)Math.Sinh(z)/z < target){
                z += 0.005f;
            }

            if (z > 0.005f && samples > 1){
    
                float a = (u-r)/2.0f/z;
                float p = (r+u-a*Mathf.Log((l+v-s)/(l-v+s)))/2.0f;
                float q = (v+s-l*(float)Math.Cosh(z)/(float)Math.Sinh(z))/2.0f;

                float inc = (u-r)*(1.0f/(samples-1));
        
                for (int i = 0; i < samples; ++i)
                {
                    float x = r+inc*i;
                    points.Add(p1 + rot * new Vector3(0,a * (float)Math.Cosh((x - p) / a) + q,x));
                }
                return true;
            }else{
                return false;
            }
        }

        public static bool Sinusoid(Vector3 origin, Vector3 direction, float l, uint frequency, int samples, List<Vector3> points)
        {

            points.Clear();

            float magnitude = direction.magnitude;

            if (magnitude > 1E-4 && samples > 1){

                direction /= magnitude;
                Vector3 ortho = Vector3.Cross(direction,Vector3.forward);

                float inc = magnitude / (samples - 1);

                float d = frequency * 4;
                float d2 = d*d;

                // analytic approx to amplitude from wave arc length.
                float amplitude = Mathf.Sqrt(l*l/d2 - magnitude*magnitude/d2);

                if (float.IsNaN(amplitude))
                    return false;
    
                for (int i = 0; i < samples; ++i)
                {
                    float pctg = i/(float)(samples - 1);
                    points.Add(origin + direction * inc * i + ortho * Mathf.Sin(pctg * Mathf.PI*2 * frequency) * amplitude);
                }
                
                return true;
            }else{
                return false;
            }           
        }
    
        /**
         * Modulo operator that also follows intuition for negative arguments. That is , -1 mod 3 = 2, not -1.
         */
        public static float Mod(float a,float b)
        {
            return a - b * Mathf.Floor(a / b);
        }

        public static Vector3 Rotate2D(this Vector3 v, float angle){
            return new Vector3(
                    v.x * Mathf.Cos(angle) - v.y * Mathf.Sin(angle),
                    v.x * Mathf.Sin(angle) + v.y * Mathf.Cos(angle),
                    v.z
                );
        }
    }
}

