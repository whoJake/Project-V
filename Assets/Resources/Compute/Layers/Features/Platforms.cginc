#ifndef __PLATFORMS__
#define __PLATFORMS__

#include "NoiseArgs.cginc"
#include "ChunkInfo.cginc"

struct Platform{
    float3 position;
    float radius;
    float flatness;

    float surfaceFeatureDepth;
    float shapeFeatureStrength;
    float hasStem;
    float2 stemRadius;
    float2 stemPinchRange;
    float stemFeatureStrength;

    uint radiusNoiseArgID;
    uint abovePlatformNoiseArgID;
    uint platform3DShapeNoiseArgID;
    uint stemRadiusNoiseArgID;
};

struct PlatformResult{
    float platformVal;
    float stemVal;
};

StructuredBuffer<Platform> _PlatformBuffer;

PlatformResult GetPlatformValue( float3 world_position ){
    uint platform_count, filler;
    _PlatformBuffer.GetDimensions( platform_count, filler );

    float platform_total = 0;
    float stem_total = 0;
    for( uint i = 0; i < platform_count; i++ ){
        Platform platform = _PlatformBuffer[i];
        float3 platform_position = platform.position;
        float platform_radius = platform.radius;

        float3 radial_dir = normalize( world_position - platform_position );
        float radial_dot = dot( radial_dir, float3( 0, 1, 0 ) );
        platform_radius -= saturate( abs( radial_dot ) ) * platform_radius * platform.flatness;

        float3 radial_position = platform_position + radial_dir * platform_radius;
        NoiseArg radial_args = _NoiseArgs[platform.radiusNoiseArgID];
        float radial_noise = fnoise01( radial_position * radial_args.scale, radial_args.octaves, radial_args.frequency, radial_args.persistance, radial_args.lacunarity ) * 2 - 1;

        platform_radius += radial_noise * platform_radius * platform.shapeFeatureStrength;
        
        NoiseArg above_platform_args = _NoiseArgs[platform.abovePlatformNoiseArgID];
        float3 world_position_at_platform = float3( world_position.x, platform_position.y, world_position.z );
        float above_noise = fnoise( world_position_at_platform, above_platform_args.octaves, above_platform_args.frequency, above_platform_args.persistance, above_platform_args.lacunarity );

        float vertical_dst_from_platform = world_position.y - platform_position.y - ( platform_radius * 0.1 ) - ( above_noise * platform.surfaceFeatureDepth ); //pos above, neg below
        float side_multiplier = saturate( -( vertical_dst_from_platform / ( platform_radius * 0.1 ) ) );
        //. 1 radius above, -1 radius below, 0 at platform height


        NoiseArg platform_args = _NoiseArgs[platform.platform3DShapeNoiseArgID];
        float3 platform_shape_coords = warp_coords( world_position, 3, 0.02, 0.5, 1.5, 10 ) * platform_args.scale;
        float platform_shape_noise = fnoise01( platform_shape_coords, platform_args.octaves, platform_args.frequency, platform_args.persistance, platform_args.lacunarity );
        platform_shape_noise *= sign(platform_args.octaves); //lol
        
        float dst_from_platform = distance( world_position, platform_position );
        float radius_mask = saturate( -( dst_from_platform - platform_radius ) / ( 12.0 ) );

        float radius_edge_mask = ( 1.0 - abs( clamp( -( dst_from_platform - platform_radius ) / ( platform.radius / 8.0 ), -1.0, 1.0 ) ) ) * 2.0; // 1 at the radius, goes towards zero both towards and away from centre
        float current_platform_shape = saturate( radius_mask * side_multiplier + platform_shape_noise * radius_edge_mask * side_multiplier );

        //Underneath pillar shape
        float radius_at_height = distance( platform_position.xz, _LayerOrigin.xz );
        float height_of_slope_below_platform = _LayerOrigin.y - GetNormalizedHeightAtRadius( radius_at_height ) * _LayerSize.y;

        float normalized_platform_to_slope_height = invlerp( world_position.y, platform_position.y, height_of_slope_below_platform );
        bool stem_multiplier = world_position.y < platform_position.y;
        
        float stem_radius_t = abs( saturate( remap( saturate( normalized_platform_to_slope_height ), platform.stemPinchRange.x, platform.stemPinchRange.y, 0, 1 ) ) - 0.5 ) * 2;
        stem_radius_t = easeinout( stem_radius_t, 3 );

        float stem_radius = lerp( platform.stemRadius.x, platform.stemRadius.y, stem_radius_t );

        NoiseArg stem_radius_noise_args = _NoiseArgs[platform.stemRadiusNoiseArgID];
        float3 stem_centre = float3( platform_position.x, world_position.y, platform_position.z );
        float3 stem_radius_noise_sample_position = stem_centre + normalize( world_position - stem_centre ) * stem_radius;
        float stem_radius_noise = fnoise01( stem_radius_noise_sample_position * stem_radius_noise_args.scale, stem_radius_noise_args.octaves, stem_radius_noise_args.frequency, stem_radius_noise_args.persistance, stem_radius_noise_args.lacunarity ) * 2 - 1;
        stem_radius += stem_radius_noise * platform.stemFeatureStrength;

        float dst_from_stem_centre = distance( world_position.xz, platform_position.xz );

        float platform_stem_shape = saturate( -( dst_from_stem_centre - stem_radius ) / 32.0 );
        
        platform_total += current_platform_shape;
        stem_total += platform_stem_shape * stem_multiplier * platform.hasStem;
    }

    PlatformResult result;
    result.platformVal = platform_total;
    result.stemVal = stem_total;
    return result;
}

#endif