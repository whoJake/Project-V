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

    public static Vector3[] RandomPointsInBounds(int samples, Bounds bounds) {
        Vector3[] result = new Vector3[samples];
        Vector3 size = bounds.size;

        for(int i = 0; i < samples; i++){ 
            float ranX = Random.Range(0f, 1f);
            float ranY = Random.Range(0f, 1f);
            float ranZ = Random.Range(0f, 1f);

            result[i] = bounds.min + new Vector3(size.x * ranX, size.y * ranY, size.z * ranZ);
        }
        return result;
    }
}
