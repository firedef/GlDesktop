using GnomeGlDesktop.gl.render.attachments;
using GnomeGlDesktop.gl.render.renderer;
using GnomeGlDesktop.gl.render.targets;
using GnomeGlDesktop.gl.shaders;
using GnomeGlDesktop.imgui;
using GnomeGlDesktop.window;
using ImGuiNET;
using MathStuff.vectors;
using OpenTK.Graphics.OpenGL;
using OpenTK.Windowing.Common;

namespace GnomeGlDesktop.gl.render.postProcess; 

public class ImGuiOverlay : IPostProcessEffect, IWindowEvents {
	public ImGuiController? controller;
	public GlfwWindow? window;
	public RenderTarget renderTarget;

	private ShaderProgram _drawToScreenShader;
	
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

layout(binding = 0) uniform sampler2D screen;
layout(binding = 1) uniform sampler2D imgui;

void main() {
	vec4 screen_col = texture(screen, f_uv).rgba;
	vec4 imgui_col = texture(imgui, f_uv).rgba;
	float l = imgui_col.r + imgui_col.g + imgui_col.b;
	if (l > .001) frag_col = imgui_col;
	else frag_col = screen_col;
    //frag_col = vec4(imgui_col.rgb * imgui_col.a + screen_col.rgb * (1 - imgui_col.a), 1);
}
";
	
	public void PostProcess(Renderer renderer) {
		if (controller == null) return;
		
		renderTarget.Begin();
		GL.Clear(ClearBufferMask.ColorBufferBit);
		
		GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
		controller!.scaleFactor = 1f;
		controller.Update(this, renderer);
		Layout();
		controller.Render();

		renderer.UniformTexture(0, 0);
		((RenderTexture)renderTarget.attachments[0]).Uniform(1);
		renderer.PostProcess(_drawToScreenShader);
	}

	private void Layout() {
		ImGui.ShowDemoWindow();
	}

	public void OnLoad(GlfwWindow win) {
		window = win;
		controller = new(win.size.x, win.size.y);
		renderTarget = new(win.size.x, win.size.y, 4);
		renderTarget.AddAttachment(0, AttachmentType.color, InternalFormat.Rgba16f);

		_drawToScreenShader = new();
		_drawToScreenShader.LoadVertexShaderString(vertShader);
		_drawToScreenShader.LoadFragmentShaderString(fragShader);
		_drawToScreenShader.Bind();
	}

	public void OnResize(GlfwWindow win, int2 s) {
		GlContext.global.MakeCurrent();
		controller!.WindowResized(win.size.x, win.size.y);
	}

	public void OnTextInput(GlfwWindow win, TextInputEventArgs e) {
		GlContext.global.MakeCurrent();
		controller!.PressChar((char) e.Unicode);
	}

	public void OnMouseWheel(GlfwWindow win, MouseWheelEventArgs e) {
		GlContext.global.MakeCurrent();
		controller!.OnMouseScroll(e.Offset);
	}
}