using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MathUtils
{
    public static Vector3[] FibonacciSphere(int samples) {
        Vector3[] result = new Vector3[samples];
        float phi = Mathf.PI * (Mathf.Sqrt(5f) - 1f);
        for (int i = 0; i < samples; i++) {
            float y = 1 - (i / (samples - 1f)) * 2;
            float radius = Mathf.Sqrt(1 - y * y);

            float theta = phi * i;
            float x = Mathf.Cos(theta) * radius;
            float z = Mathf.Sin(theta) * radius;

            result[i] = new Vector3(x, y, z);
        }
        return result;
    }
}
