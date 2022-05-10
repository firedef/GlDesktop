using GnomeGlDesktop.gl.render.postProcess;
using GnomeGlDesktop.gl.render.renderable;
using GnomeGlDesktop.gl.render.renderer;
using GnomeGlDesktop.gl.shaders;
using GnomeGlDesktop.objects.mesh;
using GnomeGlDesktop.window;
using MathStuff;
using MathStuff.vectors;
using OpenTK.Graphics.OpenGL;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace GnomeGlDesktop.gl.render; 

public class CustomRenderer {
	public readonly Renderer renderer;
	
	public Mesh<Vertex, ushort>? _mesh;
	public Mesh<Vertex, ushort>? _mesh2;
	public ShaderProgram shader;
	public ShaderProgram shader2;
	public ShaderProgram shader3;
	public ShaderProgram shader4;

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

    vec3 result = texture(screen, f_uv).rgb * 0.2270270270;
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
    frag_col = vec4(result, 1.0);
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

	public CustomRenderer() {
		renderer = new();
		
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
		
		renderer.AddRenderable(new TestRenderable(this));
		renderer.AddPostFx(new TestPostProcessFx(this));
	}

	public void AddImGui() {
		ImGuiOverlay imGuiOverlay = new();
		renderer.GetWindow(0).windowEventsListeners.Add(imGuiOverlay);
		renderer.AddPostFx(imGuiOverlay);
		
		imGuiOverlay.OnLoad(renderer.GetWindow(0));
	}
}

public class TestRenderable : IRenderable {
	public CustomRenderer custom;

	public TestRenderable(CustomRenderer custom) => this.custom = custom;

	public unsafe void Render(Renderer renderer) {
		GL.ClearColor(0, 0, 0, 1);

		GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
		
			
		GL.Enable(EnableCap.Blend);
		GL.Disable(EnableCap.DepthTest);

		float angle = custom.time * .0001f;
		const float b = MathF.PI * .5f;
		
		float x = -.08f;
		float y = -.08f;

		float x0 = -1 + x;
		float y0 = -1 + y;
		float x1 = 1 - x;
		float y1 = 1 - y;
		custom._mesh!.vertices.ptr[0].position = new(x0, y0);
		custom._mesh!.vertices.ptr[1].position = new(x0, y1);
		custom._mesh!.vertices.ptr[2].position = new(x1, y1);
		custom._mesh!.vertices.ptr[3].position = new(x1, y0);
		
		GLFW.GetCursorPos(custom.renderer.GetWindow(0).windowPtr, out double xC, out double yC);
		custom.mousePos = new((float)xC, (float)yC);

		custom.mousePos /= new float2(1920, 1080);
		custom.mousePos *= 2;
		custom.mousePos -= 1;
		custom.mousePos.FlipY();

		float delta = math.abs(custom.smoothPos0.x - custom.mousePos.x) + math.abs(custom.smoothPos0.y - custom.mousePos.y);
		
		float spd0 = .005f;
		custom.smoothPos0 = custom.mousePos * spd0 + custom.smoothPos0 * (1 - spd0);
		float spd1 = .01f;
		custom.smoothPos1 = custom.mousePos * spd1 + custom.smoothPos1 * (1 - spd1);
		float spd2 = .02f;
		custom.smoothPos2 = custom.mousePos * spd2 + custom.smoothPos2 * (1 - spd2);

		float s = MathF.Sin(custom.time * .00005f) * .3f + .2f;
		
		custom._mesh2!.vertices.ptr[0].position.x = -s + custom.smoothPos0.x * .05f;
		custom._mesh2!.vertices.ptr[0].position.y = -s + custom.smoothPos0.y * .05f + MathF.Sin(custom.time * .0005f + 0.0f) * .1f;
		
		custom._mesh2!.vertices.ptr[1].position.x = 0 + custom.smoothPos1.x * .05f;
		custom._mesh2!.vertices.ptr[1].position.y = s + custom.smoothPos1.y * .05f + MathF.Sin(custom.time * .0005f + 0.2f) * .1f;
		
		custom._mesh2!.vertices.ptr[2].position.x =  s + custom.smoothPos2.x * .05f;
		custom._mesh2!.vertices.ptr[2].position.y = -s + custom.smoothPos2.y * .05f + MathF.Sin(custom.time * .0005f + 0.4f) * .1f;
		
		custom._mesh2!.vertices.ptr[0].color.rF = math.abs(MathF.Sin(custom.time * .005f));
		custom._mesh2!.vertices.ptr[1].color.rF = math.abs(MathF.Sin(custom.time * .001f));
		custom._mesh2!.vertices.ptr[2].color.rF = math.abs(MathF.Sin(custom.time * .0001f));
		
		custom._mesh2!.vertices.ptr[0].color.gF = math.abs(MathF.Sin(custom.time * .0005f));
		custom._mesh2!.vertices.ptr[1].color.gF = math.abs(MathF.Sin(custom.time * .0001f));
		custom._mesh2!.vertices.ptr[2].color.gF = math.abs(MathF.Sin(custom.time * .00001f));
		
		custom._mesh2!.vertices.ptr[0].color.bF = math.abs(MathF.Sin(custom.time * .00005f));
		custom._mesh2!.vertices.ptr[1].color.bF = math.abs(MathF.Sin(custom.time * .00001f));
		custom._mesh2!.vertices.ptr[2].color.bF = math.abs(MathF.Sin(custom.time * .000001f));
		
		custom._mesh2.Buffer();
		
		custom.shader2.Bind();
		custom._mesh2!.Draw();
		
		custom.time += 8f * (delta * 4 + 1);

		GlfwWindow win = renderer.GetWindow(0);
		win.input.mousePosition = new((float)xC, (float)yC);
	}
}

public class TestPostProcessFx : IPostProcessEffect {
	public CustomRenderer custom;

	public TestPostProcessFx(CustomRenderer custom) => this.custom = custom;

	public void PostProcess(Renderer renderer) {
		custom.shader.Bind();
		int locHorizontal = custom.shader.UniformLocation("horizontal");
		int locStep = custom.shader.UniformLocation("step");

		bool regularBlur = false;
		bool emissiveBlur = true;
		
		if (regularBlur)
			for (int i = 0; i < 9; i++) {
				GL.Uniform1i(locHorizontal, 0);
				GL.Uniform1f(locStep, 0.001f * (i + 3));
				custom.renderer.UniformDownsampledTexture(0, 0);
				custom.renderer.ApplyToDownsampledTexture(custom.shader);
				
				GL.Uniform1i(locHorizontal, 1);
				custom.renderer.UniformDownsampledTexture(0, 0);
				custom.renderer.ApplyToDownsampledTexture(custom.shader);
			}
		
		if (emissiveBlur) {
			custom.shader4.Bind();
			custom.renderer.UniformDownsampledTexture(1, 0);
			custom.renderer.ApplyToDownsampledTexture(custom.shader4, DrawBufferMode.ColorAttachment1);
			
			custom.shader.Bind();
			for (int i = 0; i < 4; i++) {
				GL.Uniform1i(locHorizontal, 0);
				GL.Uniform1f(locStep, 0.001f * (i + 3));
				custom.renderer.UniformDownsampledTexture(1, 0);
				custom.renderer.ApplyToDownsampledTexture(custom.shader, DrawBufferMode.ColorAttachment1);

				GL.Uniform1i(locHorizontal, 1);
				custom.renderer.UniformDownsampledTexture(1, 0);
				custom.renderer.ApplyToDownsampledTexture(custom.shader, DrawBufferMode.ColorAttachment1);
			}
		}
		
		custom.shader3.Bind();
		custom.renderer.UniformTexture(0, 0);
		custom.renderer.UniformDownsampledTexture(1, 1);
		custom.renderer.PostProcess(custom.shader3);
	}
}