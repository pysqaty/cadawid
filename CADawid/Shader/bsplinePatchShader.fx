cbuffer ConstantBuffer : register(b0)
{
	matrix MVP;
	float4 Color;
}

cbuffer PatchConstantBuffer : register(b2)
{
	float2 precision;
}

static float knots[8] = { -3.0f, -2.0f, -1.0f, 0.0f, 1.0f, 2.0f, 3.0f, 4.0f };

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

float4 DeBoor(float4 controlPoints[4], float t, int ind, float T[8])
{
	float N[3 + 1] = { 0.0f, 0.0f, 0.0f, 0.0f };
	N[0] = 1;

	for (int n = 1; n <= 3; n++)
	{
		for (int j = n; j >= 0; j--)
		{
			int i = ind - n + j;
			N[j] = ((t - T[i - 1 + 3]) / (T[n + i - 1 + 3] - T[i - 1 + 3])) * (((j - 1) >= 0 && (j - 1) <= n - 1) ? N[j - 1] : 0.0f)
				+ ((T[n + i + 3] - t) / (T[n + i + 3] - T[i + 3])) * (((j) >= 0 && (j) <= n - 1) ? N[j] : 0.0f);
		}
	}
	float resx = 0.0f;
	float resy = 0.0f;
	float resz = 0.0f;

	for (int i = 0; i < 3 + 1; i++)
	{
		resx += controlPoints[i].x * N[i];
		resy += controlPoints[i].y * N[i];
		resz += controlPoints[i].z * N[i];
	}
	return float4(resx, resy, resz, 1);
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

	GS_OUTPUT o;
	o.Color = Color;

	float stepU = 1.0f / PrecisionU;
	float stepV = 1.0f / PrecisionV;

	int patchVertsCount = (PrecisionU + 1) * (PrecisionV + 1);


	if (InstanceID < 16) 
	{
		for (int ui = InstanceID; ui <= PrecisionU; ui += 16)
		{
			float4 cp1[4] = { i.p00, i.p01, i.p02, i.p03 };
			float4 n0 = DeBoor(cp1, ui * stepU, 1, knots);
			float4 cp2[4] = { i.p10, i.p11, i.p12, i.p13 };
			float4 n1 = DeBoor(cp2, ui * stepU, 1, knots);
			float4 cp3[4] = { i.p20, i.p21, i.p22, i.p23 };
			float4 n2 = DeBoor(cp3, ui * stepU, 1, knots);
			float4 cp4[4] = { i.p30, i.p31, i.p32, i.p33 };
			float4 n3 = DeBoor(cp4, ui * stepU, 1, knots);
			for (int vi = 0; vi <= PrecisionV; vi++)
			{
				float4 cpn[4] = { n0, n1, n2, n3 };
				float4 p = DeBoor(cpn, vi * stepV, 1, knots);
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
			float4 cp1[4] = { i.p00, i.p10, i.p20, i.p30 };
			float4 n0 = DeBoor(cp1, vi * stepV, 1, knots);
			float4 cp2[4] = { i.p01, i.p11, i.p21, i.p31 };
			float4 n1 = DeBoor(cp2, vi * stepV, 1, knots);
			float4 cp3[4] = { i.p02, i.p12, i.p22, i.p32 };
			float4 n2 = DeBoor(cp3, vi * stepV, 1, knots);
			float4 cp4[4] = { i.p03, i.p13, i.p23, i.p33 };
			float4 n3 = DeBoor(cp4, vi * stepV, 1, knots);
			for (int ui = 0; ui <= PrecisionU; ui++)
			{
				float4 cpn[4] = { n0, n1, n2, n3 };
				float4 p = DeBoor(cpn, ui * stepU, 1, knots);
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