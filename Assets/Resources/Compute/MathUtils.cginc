float invlerp(float value, float from, float to){
    return (value - from) / (to - from);
}

float2 setmagnitude(float2 v, float mag){
    return normalize(v) * mag;
}

float3 setmagnitude(float3 v, float mag){
    return normalize(v) * mag;
}

float remap(float val, float oldA, float oldB, float newA, float newB) {
    float a = val - oldA;
    float b = newB - newA;
    float c = oldB - oldA;
    return oldA + (a * b) / c;
}

float2 clampmagnitude(float2 v, float min, float max){
    float mag = length(v);
    if(mag < min){
        return setmagnitude(v, min);
    }
    if(mag > max){
        return setmagnitude(v, max);
    }
    return v;
}

float3 clampmagnitude(float3 v, float min, float max){
    float mag = length(v);
    if(mag < min){
        return setmagnitude(v, min);
    }
    if(mag > max){
        return setmagnitude(v, max);
    }
    return v;
}

//k range 0-1
float smoothmin(float a, float b, float k){
    float h = clamp(0.5 + 0.5*(a-b)/k, 0, 1.0);
    return lerp(a, b, h) - k*h*(1.0-h);
}

float smoothmax(float a, float b, float k){
    return smoothmin(a, b, -k);
}