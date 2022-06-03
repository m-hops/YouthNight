#ifndef OSFOG_INCLUDE
#define OSFOG_INCLUDE

#pragma multi_compile _MAIN_LIGHT_SHADOWS
#pragma multi_compile _MAIN_LIGHT_SHADOWS_CASCADE
#pragma multi_compile _SOFT_SHADOWS

half3 rayDirection;
half3 mainLightDirection;
half3 mainLightColor;

int _LightCountButo = 0;
half3 _LightPosButo[8];
half _LightIntensityButo[8];
half3 _LightColorButo[8];


half GetLightFalloff(half3 RayPosition, half3 LightPosition)
{
	half d = distance(RayPosition, LightPosition);
	return 1.0 / (1.0 + (d * d));
}

half3 GetAdditionalLightData(half3 RayPosition)
{
	half3 finalColor = half3(0, 0, 0);
	
	for (int lightIndex = 0; lightIndex < _LightCountButo; lightIndex++)
	{
		finalColor += _LightColorButo[lightIndex] * GetLightFalloff(RayPosition, _LightPosButo[lightIndex]) * _LightIntensityButo[lightIndex];
	}
	return finalColor;
}

half GetShadowAttenuation(half3 RayPos)
{
	#ifndef SHADERGRAPH_PREVIEW
	#if SHADOWS_SCREEN
	   half4 clipPos = TransformWorldToHClip(RayPos);
	   half4 shadowCoord = ComputeScreenPos(clipPos);
	#else
		half4 shadowCoord = TransformWorldToShadowCoord(RayPos);
	#endif
		return saturate(GetMainLight(shadowCoord).shadowAttenuation);
	#endif
}

half GetRandomHalf(half2 Seed)
{
	return saturate(frac(sin(dot(Seed, float2(12.9898, 78.233))) * 43758.5453));
}

half HenyeyGreenstein(half eccentricity)
{
	half cosAngle = dot(normalize(mainLightDirection), normalize(rayDirection));
	half e2 = eccentricity * eccentricity;
	return ((1.0 - e2) / pow(abs(1.0 + e2 - 2.0 * eccentricity * cosAngle), 1.5)) / 4.0 * 3.1416;
}

half GetFogExp(half VolumeThickness)
{
	// From /com.unity.render-pipelines.high-definition/Runtime/Lighting/AtmosphericScattering/Fog.cs
	// Exp[-d / H] = 0.001
	// -d / H = Log[0.001]
    // H = d / -Log[0.001]
	// Note: Cache the fogExp term.
	return 0.01;
	half H = VolumeThickness * 0.144765;
	return 1.0 / H;
}

half GetFogDensityByHeight(half HeightFalloff, half BaseHeight, half3 RayPosition)
{
	half height = max(RayPosition.y - BaseHeight, 0.0);
	return exp(-height * HeightFalloff);
}

half GetFogDensityByNoise(Texture3D NoiseTexture, SamplerState Sampler, half3 RayPosition, half NoiseScale, half3 WindVelocity, half NoiseMin, half NoiseMax)
{
	half3 uv = RayPosition * NoiseScale + WindVelocity * _Time.y;
	half4 value = SAMPLE_TEXTURE3D_LOD(NoiseTexture, Sampler, uv, 0);
	half fbm = value.r * 0.5 + value.g * 0.25 + value.b * 0.125 + value.a * 0.0625;
	
	fbm *= fbm;
	fbm = lerp(NoiseMin, NoiseMax, fbm);
	return fbm;
}

half CalculateExponentialHeightFog(half extinction, half fogExp, half rayOrigin_Y, half rayLength)
{
	return saturate((extinction / fogExp) * exp(-rayOrigin_Y * fogExp) * (1.0 - exp(-rayLength * rayDirection.y * fogExp)) / rayDirection.y);
}


float4x4 _CameraToWorld;
float4x4 _CameraInverseProjection;

void SampleVolumetricFog_half(half2 UV, half3 RayOrigin, half Depth,
half MaxDistanceNonVolumetric, half MaxDistanceVolumetric, half Anisotropy,
half BaseHeight, half HeightFalloff, half FogDensity, half LightIntensity,
half ShadowIntensity, Texture2D ColorRamp, half ColorRampInfluence, Texture3D NoiseTexture,
SamplerState NoiseSampler, half3 NoiseWindVelocity, half NoiseScale, half Depth01, half NoiseMin,
half NoiseMax, bool animateSamplePosition, int SampleCount, SamplerState PointSampler, 
out half3 Color, out half Alpha)
{
	// Out Value Setup
	Color = half3(0.0, 0.0, 0.0);
	Alpha = 1.0;
	
	#ifndef SHADERGRAPH_PREVIEW
	// Initializing and Checking Params
	HeightFalloff = max(HeightFalloff, 0.0001);
	LightIntensity = max(LightIntensity, 0.0);
	ShadowIntensity = max(ShadowIntensity, 0.0);
	FogDensity = max(FogDensity, 0.0000001);
	
	
	// Light Setup
	Light mainLight = GetMainLight();
	mainLightColor = normalize(mainLight.color);
	mainLightDirection = mainLight.direction;
	
	// Ray Setup
	half3 viewVector = mul(unity_CameraInvProjection, half4(UV * 2 - 1, 0.0, -1)).xyz;
	viewVector = mul(unity_CameraToWorld, half4(viewVector, 0.0)).xyz;
	half viewLength = length(viewVector);
	half realDepth = Depth * viewLength;
	rayDirection = viewVector / length(viewLength);
	
	half targetRayDistance = min(MaxDistanceVolumetric, realDepth);
	 
	
	// Lighting
	// Default Fog Albedo is registered as half3(1.0, 0.964, 0.92) 
	half extinction = FogDensity * 0.001;
	half hg = HenyeyGreenstein(Anisotropy);
	half3 worldColor = normalize(unity_AmbientSky + unity_AmbientSky + unity_AmbientGround).rgb;
	static half points[3] =
	{
		0.165,
		0.495,
		0.825
	};
	
	
	// Ray March
	half2 seed = UV;
	if(animateSamplePosition)
		seed += _Time.x;
	
	half random = GetRandomHalf(seed);
	
	half invStepCount = 1.0 / (half)SampleCount;
	half invNoiseScale = 1.0 / NoiseScale;
	half invTargetRayDistance = 1.0 / targetRayDistance;
	
	half3 rayPosition = RayOrigin;
	half lowerLimit, rayDepth_previous, rayDepth_current = 0.0;
	
	half3 fogColor;
	
	for (int i = 1; i <= SampleCount; i++)
	{
		// Positioning
		half ratio = (half)i * invStepCount;
		half upperLimit = (ratio * ratio) * targetRayDistance;
		rayDepth_current = lerp(lowerLimit, upperLimit, random);
		lowerLimit = upperLimit;
		rayPosition = RayOrigin + rayDirection * rayDepth_current;
		half stepLength = rayDepth_current - rayDepth_previous;
		rayDepth_previous = rayDepth_current;
		
		// Transmittance
		half relDist = saturate(rayDepth_current * invTargetRayDistance);
		half sampleDensity = GetFogDensityByHeight(HeightFalloff, BaseHeight, rayPosition) * GetFogDensityByNoise(NoiseTexture, NoiseSampler, rayPosition, invNoiseScale, NoiseWindVelocity, NoiseMin, NoiseMax);
		half sampleExtinction = extinction * sampleDensity;
		
		
		if (sampleExtinction > 0.0001)
		{
			// Evaluate Color
			half3 colorSamples[3];
			for (int i = 0; i <= 2; i++)
			{
				colorSamples[i] = SAMPLE_TEXTURE2D_LOD(ColorRamp, PointSampler, half2(relDist, points[i]), 0).rgb;
			}
			
			half3 shadedColor = lerp(worldColor, colorSamples[0], ColorRampInfluence);
			half3 litColor = lerp(mainLightColor, colorSamples[1], ColorRampInfluence) * LightIntensity;
			half3 emitColor = colorSamples[2] * ColorRampInfluence;
			
			half shadowAttenuation = GetShadowAttenuation(rayPosition);
			half shadowIntensity = lerp(ShadowIntensity, 1.0, shadowAttenuation);
			
			fogColor = lerp(shadedColor, litColor * hg, shadowAttenuation) + emitColor;
			
			
			// Solve Transmittance
			half transmittance = exp(-sampleExtinction * stepLength * shadowIntensity);
			half3 additionalLightColor = GetAdditionalLightData(rayPosition);
			
			// Solve Color
			half3 inScatteringData = sampleExtinction * (fogColor + additionalLightColor);
			half3 integratedScattering = (inScatteringData - (inScatteringData * transmittance)) / sampleExtinction;	
			Color += Alpha * integratedScattering;
			
			
			// Apply Transmittance
			Alpha *= transmittance;
		}
	}
	
	if (MaxDistanceNonVolumetric > MaxDistanceVolumetric)
	{
		// Calculate non-Volumetric Height Fog
		if (Depth01 < 1.0)
		{
			MaxDistanceNonVolumetric = min(MaxDistanceNonVolumetric, realDepth);
		}
	
		half rayLength = MaxDistanceNonVolumetric - rayDepth_current;
		half rayHeight = max(rayPosition.y - BaseHeight, 0.0);
		half fogAmount = CalculateExponentialHeightFog(extinction, HeightFalloff, rayHeight, rayLength);
	
		// Apply Transmittance
		half alpha_previous = Alpha;
		Alpha *= 1.0 - fogAmount;
	
		// Evaluate Color
		half3 colorSamples[3];
		for (int f = 0; f <= 2; f++)
		{
			colorSamples[f] = SAMPLE_TEXTURE2D_LOD(ColorRamp, PointSampler, half2(1.0, points[f]), 0).rgb;
		}
	
		half3 litColor = lerp(mainLightColor, colorSamples[1], ColorRampInfluence) * LightIntensity;
		half3 emitColor = colorSamples[2] * ColorRampInfluence;
	
		fogColor = (litColor * hg) + emitColor;
		Color += fogColor * (alpha_previous - Alpha);
	}
	#endif
	
}


#endif