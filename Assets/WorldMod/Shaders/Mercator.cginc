#ifndef MERCATOR
#define MERCATOR

float2 CoordinateToUV(float2 coord)
{
	return float2((PI + coord.x) / (PI * 2.0), (PI / 2.0 + coord.y) / PI);
}

float2 UVToCoordinate(float2 uv)
{
	return float2(uv.x * PI * 2.0 - PI, uv.y * PI - PI / 2.0);
}

float3 CoordinateToPoint(float2 In)
{
	float y = sin(In.y);
	float r = cos(In.y);
	float x = sin(In.x) * r;
	float z = -cos(In.x) * r;
	return float3(x ,y, z);
}

float2 PointToCoordinate(float3 In)
{
	return float2(atan2(In.x, -In.z), asin(In.y)); 
}

void CoordinateToPoint_float(float2 In, out float3 Out)
{
	Out = CoordinateToPoint(In);
}

void PointToCoordinate_float(float3 In, out float2 Out)
{
	Out = PointToCoordinate(In);
}

void CoordinateToUV_float(float2 coord, out float2 uv)
{
	uv = CoordinateToUV(coord);
}

void UVToCoordinate_float(float2 uv, out float2 coord)
{
	coord = UVToCoordinate(uv);
}

#endif