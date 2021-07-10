cbuffer ConstantBuffer : register(b0)
{
	matrix MVP;
	float4 Color;
}

cbuffer PatchConstantBuffer : register(b2)
{
	float2 precision;
}

struct VS_INOUT 
{
	float4 p00 : P00p;
	float4 p10 : P10p;
	float4 p20 : P20p;
	float4 p30 : P30p;
	float4 p01 : P01p;
	float4 p11 : P11p;
	float4 p21 : P21p;
	float4 p31 : P31p;
	float4 p02 : P02p;
	float4 p12 : P12p;
	float4 p22 : P22p;
	float4 p32 : P32p;
	float4 p03 : P03p;
	float4 p13 : P13p;
	float4 p23 : P23p;
	float4 p33 : P33p;
};

struct GS_OUTPUT
{
	float4 Pos : SV_POSITION;
	float4 Color : COLOR0;
};


float4 Lerp(float4 p0, float4 p1, float t) 
{
	return (1 - t) * p0 + t * p1;
}

float4 DeCasteljau(float4 p0, float4 p1, float4 p2, float4 p3, float t)
{
	float4 q0 = Lerp(p0, p1, t);
	float4 q1 = Lerp(p1, p2, t);
	float4 q2 = Lerp(p2, p3, t);

	float4 r0 = Lerp(q0, q1, t);
	float4 r1 = Lerp(q1, q2, t);

	float4 p = Lerp(r0, r1, t);
	return p;
}


//--------------------------------------------------------------------------------------
// Vertex Shader
//--------------------------------------------------------------------------------------
VS_INOUT VS(VS_INOUT i)
{
	return i;
}


//--------------------------------------------------------------------------------------
// Geometry Shader
//--------------------------------------------------------------------------------------
[instance(32)]
[maxvertexcount(128)]
void GS(point VS_INOUT input[1], inout LineStream<GS_OUTPUT> OutputStream, uint InstanceID : SV_GSInstanceID)
{
	VS_INOUT i = input[0];
	int PrecisionU = (int)precision.x;
	int PrecisionV = (int)precision.y;

	float4 b0 = float4(i.p00.w, i.p01.w, i.p02.w, i.p03.w);
	float4 b1 = float4(i.p10.w, i.p11.w, i.p12.w, i.p13.w);
	float4 b2 = float4(i.p20.w, i.p21.w, i.p22.w, i.p23.w);
	float4 b3 = float4(i.p30.w, i.p31.w, i.p32.w, i.p33.w);

	i.p00.w = 1;
	i.p10.w = 1;
	i.p20.w = 1;
	i.p30.w = 1;
	i.p01.w = 1;
	i.p11.w = 1;
	i.p21.w = 1;
	i.p31.w = 1;
	i.p02.w = 1;
	i.p12.w = 1;
	i.p22.w = 1;
	i.p32.w = 1;
	i.p03.w = 1;
	i.p13.w = 1;
	i.p23.w = 1;
	i.p33.w = 1;

	GS_OUTPUT o;
	o.Color = Color;

	float stepU = 1.0f / PrecisionU;
	float stepV = 1.0f / PrecisionV;

	int patchVertsCount = (PrecisionU + 1) * (PrecisionV + 1);


	if (InstanceID < 16) 
	{
		for (int ui = InstanceID; ui <= PrecisionU; ui += 16)
		{
			for (int vi = 0; vi <= PrecisionV; vi++)
			{
				float v = vi * stepV;
				float u = ui * stepU;
				float4 n0 = DeCasteljau(
					i.p00,
					i.p01,
					i.p02,
					i.p03, u);
				float4 n1 = DeCasteljau(
					i.p10,
					(i.p11 * u + b0 * v) / ((u + v) == 0 ? 1 : (u + v)),
					(i.p12 * u + b1 * (1 - v)) / ((u + 1 - v) == 0 ? 1 : (u + 1 - v)),
					i.p13, u);
				float4 n2 = DeCasteljau(
					i.p20,
					(i.p21 * (1 - u) + b2 * v) / ((1 - u + v) == 0 ? 1 : (1 - u + v)),
					(i.p22 * (1 - u) + b3 * (1 - v)) / ((2 - u - v) == 0 ? 1 : (2 - u - v)),
					i.p23, u);
				float4 n3 = DeCasteljau(
					i.p30,
					i.p31,
					i.p32,
					i.p33, u);
				float4 p = DeCasteljau(n0, n1, n2, n3, v);
				o.Pos = mul(MVP, p);
				OutputStream.Append(o);
			}
			OutputStream.RestartStrip();
		}
	}
	else 
	{
		for (int vi = InstanceID - 16; vi <= PrecisionV; vi += 16)
		{
			
			for (int ui = 0; ui <= PrecisionU; ui++)
			{
				float u = ui * stepU;
				float v = vi * stepV;
				float4 n0 = DeCasteljau(
					i.p00,
					i.p10,
					i.p20,
					i.p30, v);
				float4 n1 = DeCasteljau(
					i.p01,
					(i.p11 * u + b0 * v) / ((u + v) == 0 ? 1 : (u + v)),
					(i.p21 * (1 - u) + b2 * v) / ((1 - u + v) == 0 ? 1 : (1 - u + v)),
					i.p31, v);
				float4 n2 = DeCasteljau(
					i.p02,
					(i.p12 * u + b1 * (1 - v)) / ((u + 1 - v) == 0 ? 1 : (u + 1 - v)),
					(i.p22 * (1 - u) + b3 * (1 - v)) / ((2 - u - v) == 0 ? 1 : (2 - u - v)),
					i.p32, v);
				float4 n3 = DeCasteljau(
					i.p03,
					i.p13,
					i.p23,
					i.p33, v);
				float4 p = DeCasteljau(n0, n1, n2, n3, u);
				o.Pos = mul(MVP, p);
				OutputStream.Append(o);
			}
			OutputStream.RestartStrip();
		}
	}
	
}


//--------------------------------------------------------------------------------------
// Pixel Shader
//--------------------------------------------------------------------------------------
float4 PS(GS_OUTPUT input) : SV_Target
{
	return input.Color;
}