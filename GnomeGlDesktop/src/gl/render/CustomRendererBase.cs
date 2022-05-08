using GnomeGlDesktop.gl.render.attachments;
using GnomeGlDesktop.gl.shaders;
using GnomeGlDesktop.objects.mesh;
using MathStuff;
using MathStuff.vectors;
using OpenTK.Graphics.OpenGL.Compatibility;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace GnomeGlDesktop.gl.render; 

public class CustomRendererBase : RendererBase {
	private static Mesh<Vertex, ushort>? _mesh;
	private static Mesh<Vertex, ushort>? _mesh2;
	private static ShaderProgram shader;
	private static ShaderProgram shader2;
	private static ShaderProgram shader3;
	private static ShaderProgram shader4;

	private string vertShader = @"
#version 330 core
in layout(location=0) vec3 a_pos;
in layout(location=1) vec3 a_normal;
in layout(location=2) vec4 a_col;
in layout(location=3) vec2 a_uv0;
in layout(location=4) vec2 a_uv1;

out vec2 f_uv;
out vec4 f_col;

void main() {
	gl_Position = vec4(a_pos, 1.0);
	f_col = a_col.bgra;
	f_uv = a_uv0;
}
";

	private string fragShader = @"
#version 420 core
in vec4 f_col;
in vec2 f_uv;

out vec4 frag_col;
//out vec4 frag_blur;

uniform bool horizontal;
uniform float step = 0.003;

uniform float weight[5] = float[] (0.227027, 0.1945946, 0.1216216, 0.054054, 0.016216);
uniform sampler2D screen;

void main() {
	vec2 tex_offset = vec2(step);
	//vec2 tex_offset = 1.0 / textureSize(screen, 0);

    vec3 result = texture(screen, f_uv).rgb * (0.2270270270 + 2);
    if(horizontal)
    {
		result += texture(screen, f_uv + vec2(tex_offset.x * 1.3846153846, 0.0)).rgb * 0.3162162162;
		result += texture(screen, f_uv - vec2(tex_offset.x * 1.3846153846, 0.0)).rgb * 0.3162162162;

		result += texture(screen, f_uv + vec2(tex_offset.x * 3.2307692308, 0.0)).rgb * 0.0702702703;
		result += texture(screen, f_uv - vec2(tex_offset.x * 3.2307692308, 0.0)).rgb * 0.0702702703;
    }
    else
    {
        result += texture(screen, f_uv + vec2(0.0, tex_offset.y * 1.3846153846)).rgb * 0.3162162162;
		result += texture(screen, f_uv - vec2(0.0, tex_offset.y * 1.3846153846)).rgb * 0.3162162162;

		result += texture(screen, f_uv + vec2(0.0, tex_offset.y * 3.2307692308)).rgb * 0.0702702703;
		result += texture(screen, f_uv - vec2(0.0, tex_offset.y * 3.2307692308)).rgb * 0.0702702703;
    }
    frag_col = vec4(result / 3, 1.0);
}
";
	
	private string fragShader2 = @"
#version 420 core
in vec4 f_col;
in vec2 f_uv;

out vec4 frag_col;

void main() {
	frag_col = f_col;
}
";
	
	private string fragShader3 = @"
#version 420 core
in vec4 f_col;
in vec2 f_uv;

out vec4 frag_col;

layout(binding = 0) uniform sampler2D screen;
layout(binding = 1) uniform sampler2D blurred;

void main() {
	const float gamma = 2.2;
	const float exposure = 1;
	vec3 screen_col = texture(screen, f_uv).rgb;
	vec3 blur_col = texture(blurred, f_uv).rgb;

	vec3 col = screen_col + blur_col * 3;
	col = vec3(1.0) - exp(-col * exposure);
	col = pow(col, vec3(1.0 / gamma));
	frag_col = vec4(col, 1.0);
}
";
	
	private string fragShader4 = @"
#version 420 core
in vec4 f_col;
in vec2 f_uv;

out vec4 frag_col;

layout(binding = 0) uniform sampler2D screen;

float getLuminance(vec3 col) {
	return 0.2126 * col.r + 0.7152 * col.g + 0.0722 * col.b;
}

void main() {
	const float bias = 0.2;
	vec3 screen_col = texture(screen, f_uv).rgb;

	vec3 col = pow(screen_col, vec3(3.0));
	col = col * getLuminance(col);
	
	frag_col = vec4(col, 1.0);
}
";

	public float2 mousePos;
	public float2 smoothPos0;
	public float2 smoothPos1;
	public float2 smoothPos2;
	public float time;

	protected override unsafe void OnLoad() {
		base.OnLoad();
		
		shader = new();
		shader.LoadVertexShaderString(vertShader);
		shader.LoadFragmentShaderString(fragShader);
		shader.Bind();
		
		shader2 = new();
		shader2.LoadVertexShaderString(vertShader);
		shader2.LoadFragmentShaderString(fragShader2);
		shader2.Bind();
		
		shader3 = new();
		shader3.LoadVertexShaderString(vertShader);
		shader3.LoadFragmentShaderString(fragShader3);
		shader3.Bind();
		
		shader4 = new();
		shader4.LoadVertexShaderString(vertShader);
		shader4.LoadFragmentShaderString(fragShader4);
		shader4.Bind();
	
		_mesh = new();
		Vertex p0 = new(new(-.5f,-.5f), float3.front, color.white,   new(0, 0), float2.zero);
		Vertex p1 = new(new(-.5f, .5f), float3.front, color.white, new(0, 1), float2.zero);
		Vertex p2 = new(new( .5f, .5f), float3.front, color.white,    new(1, 1), float2.zero);
		Vertex p3 = new(new( .5f,-.5f), float3.front, color.white, new(1, 0), float2.zero);
		_mesh.AddQuad(p0, p1, p2, p3);
		_mesh!.Buffer();
		
		_mesh2 = new();
		p0 = new(new(-.5f,-.5f), float3.front, color.softBlue,   new(0, 0), float2.zero);
		p1 = new(new(   0, .5f), float3.front, color.softPurple,   new(1, 1), float2.zero);
		p2 = new(new( .5f,-.5f), float3.front, color.softRed, new(1, 0), float2.zero);
		_mesh2.AddTriangle(p0, p1, p2);
		_mesh2!.Buffer();

		// GLFW.SetCursorPosCallback(windows[0].windowPtr, (w, x, y) => mousePos = new((float) x, (float) y));
	}

	protected override void PostProcess() {
		shader.Bind();
		int locHorizontal = shader.UniformLocation("horizontal");
		int locStep = shader.UniformLocation("step");

		bool regularBlur = false;
		bool emissiveBlur = true;
		
		if (regularBlur)
			for (int i = 0; i < 9; i++) {
				GL.Uniform1i(locHorizontal, 0);
				GL.Uniform1f(locStep, 0.001f * (i + 3));
				postProcess.UniformDownsampledTextureAttachment(0, 0);
				postProcess.DrawToDownsampledTexture(shader);
				
				GL.Uniform1i(locHorizontal, 1);
				postProcess.UniformDownsampledTextureAttachment(0, 0);
				postProcess.DrawToDownsampledTexture(shader);
			}
		
		if (emissiveBlur) {
			shader4.Bind();
			postProcess.UniformDownsampledTextureAttachment(1, 0);
			postProcess.DrawToDownsampledTexture(shader4, DrawBufferMode.ColorAttachment1);
			
			shader.Bind();
			for (int i = 0; i < 9; i++) {
				GL.Uniform1i(locHorizontal, 0);
				GL.Uniform1f(locStep, 0.001f * (i + 3));
				postProcess.UniformDownsampledTextureAttachment(1, 0);
				postProcess.DrawToDownsampledTexture(shader, DrawBufferMode.ColorAttachment1);

				GL.Uniform1i(locHorizontal, 1);
				postProcess.UniformDownsampledTextureAttachment(1, 0);
				postProcess.DrawToDownsampledTexture(shader, DrawBufferMode.ColorAttachment1);
			}
		}
		
		shader3.Bind();
		postProcess.UniformTextureAttachment(0, 0);
		postProcess.UniformDownsampledTextureAttachment(1, 1);
		postProcess.PostProcess(shader3);
		
		// for (int i = 0; i < 1; i++) {
		// 	postProcess.PostProcess(shader);
		// }
		//postProcess.PostProcess(shader);
		//postProcess.PostProcess(shader);
	}

	protected override unsafe void OnRender() {
		GL.ClearColor(0, 0, 0, 1);

		GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
		
			
		GL.Enable(EnableCap.Blend);
		GL.Disable(EnableCap.DepthTest);

		//float time = (float)DateTime.Now.TimeOfDay.TotalMilliseconds;

		float angle = time * .0001f;
		const float b = MathF.PI * .5f;
		// _mesh!.vertices.ptr[0].position = float2.SinCos(angle + b * 0, MathF.Abs(MathF.Sin(time * .0005f) * 1.4f) + 0.8f);
		// _mesh!.vertices.ptr[1].position = float2.SinCos(angle + b * 1, MathF.Abs(MathF.Sin(time * .0005f) * 1.4f) + 0.8f);
		// _mesh!.vertices.ptr[2].position = float2.SinCos(angle + b * 2, MathF.Abs(MathF.Sin(time * .0005f) * 1.4f) + 0.8f);
		// _mesh!.vertices.ptr[3].position = float2.SinCos(angle + b * 3, MathF.Abs(MathF.Sin(time * .0005f) * 1.4f) + 0.8f);

		//float x = MathF.Sin(time * .0005f)* .1f;
		//float y = MathF.Cos(time * .0002f)* .1f;
		float x = -.08f;
		float y = -.08f;

		float x0 = -1 + x;
		float y0 = -1 + y;
		float x1 = 1 - x;
		float y1 = 1 - y;
		_mesh!.vertices.ptr[0].position = new(x0, y0);
		_mesh!.vertices.ptr[1].position = new(x0, y1);
		_mesh!.vertices.ptr[2].position = new(x1, y1);
		_mesh!.vertices.ptr[3].position = new(x1, y0);
		
		//_mesh!.vertices.ptr[0].position.x = math.abs(MathF.Sin(time * .0005f));
		//_mesh!.vertices.ptr[1].position.x = math.abs(MathF.Sin(time * .0002f));
		//_mesh!.vertices.ptr[2].position.x = math.abs(MathF.Sin(time * .0009f));
		//_mesh!.vertices.ptr[3].position.x = math.abs(MathF.Sin(time * .0001f));

		// shader.Bind();
		// postProcess.nextRenderTexture.Uniform();
		// _mesh.Buffer();
		// _mesh!.Draw();
		
		// _mesh2!.vertices.ptr[0].position.y = -.5f + MathF.Sin(time * .0005f + 0.0f) * .4f;
		// _mesh2!.vertices.ptr[1].position.y =  .5f + MathF.Sin(time * .0005f + 0.2f) * .4f;
		// _mesh2!.vertices.ptr[2].position.y = -.5f + MathF.Sin(time * .0005f + 0.4f) * .4f;
		
		GLFW.GetCursorPos(windows[0].windowPtr, out double xC, out double yC);
		mousePos = new((float)xC, (float)yC);

		mousePos /= new float2(1920, 1080);
		mousePos *= 2;
		mousePos -= 1;
		mousePos.FlipY();

		float delta = math.abs(smoothPos0.x - mousePos.x) + math.abs(smoothPos0.y - mousePos.y);
		
		float spd0 = .005f;
		smoothPos0 = mousePos * spd0 + smoothPos0 * (1 - spd0);
		float spd1 = .01f;
		smoothPos1 = mousePos * spd1 + smoothPos1 * (1 - spd1);
		float spd2 = .02f;
		smoothPos2 = mousePos * spd2 + smoothPos2 * (1 - spd2);

		float s = MathF.Sin(time * .00005f) * .3f + .2f;
		
		_mesh2!.vertices.ptr[0].position.x = -s + smoothPos0.x * .05f;
		_mesh2!.vertices.ptr[0].position.y = -s + smoothPos0.y * .05f + MathF.Sin(time * .0005f + 0.0f) * .1f;
		
		_mesh2!.vertices.ptr[1].position.x = 0 + smoothPos1.x * .05f;
		_mesh2!.vertices.ptr[1].position.y = s + smoothPos1.y * .05f + MathF.Sin(time * .0005f + 0.2f) * .1f;
		
		_mesh2!.vertices.ptr[2].position.x =  s + smoothPos2.x * .05f;
		_mesh2!.vertices.ptr[2].position.y = -s + smoothPos2.y * .05f + MathF.Sin(time * .0005f + 0.4f) * .1f;
		
		_mesh2!.vertices.ptr[0].color.rF = math.abs(MathF.Sin(time * .005f));
		_mesh2!.vertices.ptr[1].color.rF = math.abs(MathF.Sin(time * .001f));
		_mesh2!.vertices.ptr[2].color.rF = math.abs(MathF.Sin(time * .0001f));
		
		_mesh2!.vertices.ptr[0].color.gF = math.abs(MathF.Sin(time * .0005f));
		_mesh2!.vertices.ptr[1].color.gF = math.abs(MathF.Sin(time * .0001f));
		_mesh2!.vertices.ptr[2].color.gF = math.abs(MathF.Sin(time * .00001f));
		
		_mesh2!.vertices.ptr[0].color.bF = math.abs(MathF.Sin(time * .00005f));
		_mesh2!.vertices.ptr[1].color.bF = math.abs(MathF.Sin(time * .00001f));
		_mesh2!.vertices.ptr[2].color.bF = math.abs(MathF.Sin(time * .000001f));
		
		_mesh2.Buffer();
		
		shader2.Bind();
		_mesh2!.Draw();
		
		
		time += 8f * (delta * 4 + 1);
	}
}