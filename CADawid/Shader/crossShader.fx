cbuffer ConstantBuffer : register(b0)
{
	matrix MVP;
	float4 Color;
}

cbuffer CrossConstantBuffer : register(b1) 
{
	float distance;
}

struct VS_OUTPUT
{
	float4 Pos : SV_POSITION;
	float4 Color : COLOR0;
	float4 LocalPos : LOCAL;
};

//--------------------------------------------------------------------------------------
// Vertex Shader
//--------------------------------------------------------------------------------------
VS_OUTPUT VS(float4 Pos : POSITION)
{
	VS_OUTPUT output = (VS_OUTPUT)0;
	output.LocalPos = Pos;
	output.Pos = mul(MVP, Pos);
	output.Color = Color;
	return output;
}

//--------------------------------------------------------------------------------------
// Geometry<Vertex, Index> Shader
//--------------------------------------------------------------------------------------
[maxvertexcount(3)]
void GS(triangle VS_OUTPUT input[3], inout TriangleStream<VS_OUTPUT> OutputStream)
{
	OutputStream.Append(input[0]);
	OutputStream.Append(input[1]);
	OutputStream.Append(input[2]);
}


//--------------------------------------------------------------------------------------
// Pixel Shader
//--------------------------------------------------------------------------------------
float4 PS(VS_OUTPUT input) : SV_Target
{
	float dist1 = abs(input.LocalPos.x + input.LocalPos.y) / sqrt(2);
	float dist2 = abs(input.LocalPos.x - input.LocalPos.y) / sqrt(2);
	if (dist1 > distance && dist2 > distance)
	{
		discard;
		return float4(1, 1, 1, 0);
	}
	else 
	{
		return input.Color;
	}
}