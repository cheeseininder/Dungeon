#define Max_Steps 100
#define Max_Dist 100
#define Surf_Dist 1e-3
float GetDist(float3 p)
{
    float d = length(p) - .5;
    return d;
}
void raystep_float(float3 rayOrigin, float3 rayDiretcion,out float raySteps)
{
    float distanceOrigin = 0;
    float distanceSurface = 0;
    for(int i = 0; i < Max_Steps; i ++)
    {
        float3 rayp = rayOrigin + distanceOrigin * rayDiretcion;
        distanceSurface = GetDist(rayp);
        distanceOrigin += distanceSurface;
        if(distanceSurface < Surf_Dist || distanceOrigin > Max_Dist)
            break;
        
    }
    raySteps = distanceOrigin;
}

void ColorRay_float(float d, out float4 color)
{
    color = 0;
    if(d < Max_Dist)
        color.x = 1;
}