#version 330 core

in rayIn
{
    vec3 rayOrigin;
    vec3 rayDestination;
    vec4 screen;
} v2f;

out vec4 outColor;

uniform vec4 _projectionParams;
uniform sampler3D volumeTexture;

bool cubeRayIntersection(vec3 rayOrigin, vec3 rayDirection, vec3 cubeMinimum, vec3 cubeMaximum, out float nearIntersectionDistance, out float farIntersectionDistance)
{
    // take inverse to avoid slow floating point division in pure ray-aabb function
    vec3 inverseDirection = 1.0 / rayDirection;

    // calculate raw distance parameters, effectively distance along ray to intersect axes
    vec3 distanceParameter1 = inverseDirection * (cubeMinimum - rayOrigin);
    vec3 distanceParameter2 = inverseDirection * (cubeMaximum - rayOrigin);

    vec3 minimumDistanceParameter = min(distanceParameter1, distanceParameter2);
    vec3 maximumDistanceParameter = max(distanceParameter1, distanceParameter2);

    nearIntersectionDistance = max(minimumDistanceParameter.x, max(minimumDistanceParameter.y, minimumDistanceParameter.z));
    farIntersectionDistance = min(maximumDistanceParameter.x, min(maximumDistanceParameter.y, maximumDistanceParameter.z));

    return nearIntersectionDistance <= farIntersectionDistance;
}

vec4 raymarchColor(gsampler3D raymarchTexture, float epsilon, vec3 rayOrigin, vec3 rayDirection)
{
    float nearIntersectionDistance, farIntersectionDistance;
    cubeRayIntersection(rayOrigin, rayDirection, -0.5, 0.5, nearIntersectionDistance, farIntersectionDistance);

    // if near intersection is less than zero (we're inside cube), then raycast from zero distance
    // todo nearIntersectionDistance *= (nearIntersectionDistance >= 0.0);

    float accumulatedDistance = nearIntersectionDistance;
    float maximumAccumulatedDistance = farIntersectionDistance;

    while (accumulatedDistance < maximumAccumulatedDistance)
    {
        vec3 accumulatedRay = rayOrigin + (rayDirection * accumulatedDistance) + 0.5;
        vec4 color = texture(raymarchTexture, accumulatedRay);

        if (color.a == 1.0)
        {
            return color;
        }

        accumulatedDistance += max(epsilon, color.a);
    }

    return vec4(0.0);
}

void main()
{
    vec2 screen = v2f.screen.xy / v2f.screen.w;
    vec3 rayDirection = normalize(v2f.rayDestination - v2f.rayOrigin);
    v2f.rayOrigin += rayDirection * _projectionParams.y;

    vec4 color = raymarchColor(volumeTexture, 0.00001, v2f.rayOrigin, rayDirection);

    if (color.a < 1.0)
    {
        discard;
    }

    outColor = color;
}