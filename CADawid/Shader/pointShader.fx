cbuffer ConstantBuffer : register(b0)
{
	matrix MVP;
	float4 Color;
}

cbuffer PointConstantBuffer : register(b1) 
{
	float R;
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
	float dist = input.LocalPos.x * input.LocalPos.x + input.LocalPos.y * input.LocalPos.y;
	if (dist < R * R)
	{
		return input.Color;
	}
	else
	{
		discard;
		return float4(1, 1, 1, 0);
	}
}