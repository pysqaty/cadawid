cbuffer ConstantBuffer : register(b0)
{
	matrix MVP;
	float4 Color;
}

struct VS_OUTPUT
{
	float4 Pos : SV_POSITION;
	float4 Color : COLOR0;
};

//--------------------------------------------------------------------------------------
// Vertex Shader
//--------------------------------------------------------------------------------------
VS_OUTPUT VS(float4 Pos : POSITION)
{
	VS_OUTPUT output = (VS_OUTPUT)0;
	output.Pos = mul(MVP, Pos);
	output.Color = Color;
	return output;
}


//--------------------------------------------------------------------------------------
// Geometry<Vertex, Index> Shader
//--------------------------------------------------------------------------------------
[maxvertexcount(2)]
void GS(line VS_OUTPUT input[2], inout LineStream<VS_OUTPUT> OutputStream)
{
	OutputStream.Append(input[0]);
	OutputStream.Append(input[1]);
}


//--------------------------------------------------------------------------------------
// Pixel Shader
//--------------------------------------------------------------------------------------
float4 PS(VS_OUTPUT input) : SV_Target
{
	return input.Color;
}