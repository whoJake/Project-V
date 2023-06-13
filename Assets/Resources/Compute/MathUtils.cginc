float invlerp(float value, float from, float to){
    return (value - from) / (to - from);
}

float2 setmagnitude(float2 v, float mag){
    return normalize(v) * mag;
}

float3 setmagnitude(float3 v, float mag){
    return normalize(v) * mag;
}

float easeout(float x, int power) {
    return 1 - pow(1 - x, power);
}

float easein(float x, int power) {
    return pow(x, power);
}

float easeinout(float x, int power){
    float b = x > 0.5;
    return lerp( pow(2, power-1) * easein(x, power), 1 - pow(-2 * x + 2, power) / 2 , b );
}

float easebackout(float x, int power){
    float c1 = 1.70158;
    float c3 = c1 + 1;

    return 1 + c3 * pow( x - 1, power ) + c1 * pow( x - 1, power - 1 );
}

float remap(float val, float oldA, float oldB, float newA, float newB) {

    //return newA + invlerp( val, oldA, oldB ) * (newB - newA);

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

float dstFromLine(float3 lineStart, float3 lineEnd, float3 spot) {
    //https://math.stackexchange.com/questions/1905533/find-perpendicular-distance-from-point-to-line-in-3d
    float3 dir = (lineEnd - lineStart) / distance(lineStart, lineEnd);
    float3 startToSpotVec = spot - lineStart;
    float t = dot(startToSpotVec, dir);
    float3 projectedSpot = lineStart + dir * t;
    return distance(lineEnd, projectedSpot);
}

float dstFromLine2(float3 lineStart, float3 lineEnd, float3 spot) {
    //https://math.stackexchange.com/questions/1905533/find-perpendicular-distance-from-point-to-line-in-3d
    return length(cross(spot - lineStart, lineEnd - lineStart)) / length(lineEnd - lineStart);
}

//k range 0-1
float smoothmin(float a, float b, float k){
    float h = clamp(0.5 + 0.5*(a-b)/k, 0, 1.0);
    return lerp(a, b, h) - k*h*(1.0-h);
}

float smoothmax(float a, float b, float k){
    return smoothmin(a, b, -k);
}