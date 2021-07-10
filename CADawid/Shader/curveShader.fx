cbuffer GeometryConstantBuffer : register(b0)
{
	matrix MVP;
	float4 Color;
}

struct VS_OUTPUT
{
	float4 Pos0 : P0p;
	float4 Pos1 : P1p;
	float4 Pos2 : P2p;
	float4 Pos3 : P3p;
};

struct GS_OUTPUT
{
	float4 Pos : SV_POSITION;
	float4 Color : COLOR0;
};

struct SubdivisionParameters
{
	float4 p0;
	float4 p1;
	float4 p2;
	float4 p3;
	int depth;
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

void SubdivideCurve(float4 p0, float4 p1, float4 p2, float4 p3, float t,
	out float4 q0, out float4 r0, out float4 p, out float4 r1, out float4 q2) 
{
	q0 = Lerp(p0, p1, t);
	float4 q1 = Lerp(p1, p2, t);
	q2 = Lerp(p2, p3, t);

	r0 = Lerp(q0, q1, t);
	r1 = Lerp(q1, q2, t);

	p = Lerp(r0, r1, t);
}


//--------------------------------------------------------------------------------------
// Vertex Shader
//--------------------------------------------------------------------------------------
VS_OUTPUT VS(float4 Pos0 : P0p, float4 Pos1 : P1p, float4 Pos2 : P2p, float4 Pos3 : P3p)
{
	VS_OUTPUT output = (VS_OUTPUT)0;
	output.Pos0 = Pos0;
	output.Pos1 = Pos1;
	output.Pos2 = Pos2;
	output.Pos3 = Pos3;
	return output;
}

//--------------------------------------------------------------------------------------
// Geometry Shader
//--------------------------------------------------------------------------------------
[maxvertexcount(128)]
void GS(point VS_OUTPUT input[1], inout LineStream<GS_OUTPUT> OutputStream)
{
	GS_OUTPUT o;
	o.Color = Color;
	float4 p0 = input[0].Pos0;
	float4 p1 = input[0].Pos1;
	float4 p2 = input[0].Pos2;
	float4 p3 = input[0].Pos3;
	int stackLen = 0;

	SubdivisionParameters dummy;
	dummy.p0 = float4(0.0f, 0.0f, 0.0f, 0.0f);
	dummy.p1 = float4(0.0f, 0.0f, 0.0f, 0.0f);
	dummy.p2 = float4(0.0f, 0.0f, 0.0f, 0.0f);
	dummy.p3 = float4(0.0f, 0.0f, 0.0f, 0.0f);
	dummy.depth = -1;

	SubdivisionParameters current;
	current.p0 = p0;
	current.p1 = p1;
	current.p2 = p2;
	current.p3 = p3;
	current.depth = 0;
	SubdivisionParameters recStack[20] =
	{
		current, dummy, dummy, dummy, dummy,
		dummy, dummy, dummy, dummy, dummy,
		dummy, dummy, dummy, dummy, dummy,
		dummy, dummy, dummy, dummy, dummy,
	};
	stackLen++;

	while (stackLen != 0) 
	{
		current = recStack[--stackLen];
		float4 v = normalize(current.p3 - current.p0);
		float w1 = (current.p1 - current.p0);
		float w2 = (current.p2 - current.p0);
		float d1 = length(cross(v, w1)) / length(v);
		float d2 = length(cross(v, w2)) / length(v);
		float eps = 0.001;

		if (current.depth == 6 || (d1 < eps && d2 < eps))
		{
			o.Pos = mul(MVP, float4(current.p0.xyz, 1.0f));
			OutputStream.Append(o);
			o.Pos = mul(MVP, float4(current.p3.xyz, 1.0f));
			OutputStream.Append(o);
			OutputStream.RestartStrip();
		}
		else
		{
			float4 pp0 = current.p0;
			float4 q0;
			float4 r0;
			float4 p;
			float4 r1;
			float4 q2;
			float4 pp3 = current.p3;
			int depth = current.depth;

			SubdivideCurve(current.p0, current.p1, current.p2, current.p3, 0.5f,
				q0, r0, p, r1, q2);

			current.depth = depth + 1;

			current.p0 = pp0;
			current.p1 = q0;
			current.p2 = r0;
			current.p3 = p;
			recStack[stackLen++] = current;
			current.p0 = p;
			current.p1 = r1;
			current.p2 = q2;
			current.p3 = pp3;
			recStack[stackLen++] = current;
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