using System.Numerics;
using GnomeGlDesktop.gl;
using GnomeGlDesktop.imgui;
using GnomeGlDesktop.utils;
using MathStuff;
using MathStuff.vectors;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL.Compatibility;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using X11;
using Vector2 = OpenTK.Mathematics.Vector2;
using Window = X11.Window;

namespace GnomeGlDesktop.window;

public struct Vertex {
	public float3 pos;
	public float3 col;
	public Vertex(float3 pos, float3 col) {
		this.pos = pos;
		this.col = col;
	}
}

public class GlWindow : ImGuiWindow {
	public List<GlWindow> childWindows = new();

	public GlWindow(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings, int i) : base(gameWindowSettings, nativeWindowSettings, i == 0) {
		
	}

	Vertex[] vertices = {
		new(new(-.5f,-.5f, 0), new(1,0,0)),
		new(new(.0f,  .5f, 0), new(0,1,0)),
		new(new(.5f, -.5f, 0), new(0,0,1)),
	};

	private VBO vbo;
	private VAO vao;
	private IBO ibo;
	private ShaderProgram shader;

	private string vertShader = @"
#version 330 core
in vec3 a_pos;
in vec3 a_col;

out vec3 f_col;

void main() {
	gl_Position = vec4(a_pos, 1.0);
	f_col = a_col;
}
";

	private string fragShader = @"
#version 330 core
in vec3 f_col;
out vec4 frag_col;

void main() {
	frag_col = vec4(f_col.xyz,1.0);
}
";

	protected override unsafe void OnLoad() {
		Context.MakeCurrent();
		VSync = VSyncMode.On;
		
		shader = new(vertShader, fragShader);
		shader.Bind();
		
		vao = VAO.Generate();
		vao.Bind();

		ibo = IBO.Generate();
		ibo.Bind();
		
		vbo = VBO.Generate();
		vbo.Bind();
		GL.BufferData(BufferTargetARB.ArrayBuffer, vertices, BufferUsageARB.DynamicDraw);
		
		GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, sizeof(Vertex), 0);
		GL.EnableVertexAttribArray(0);
		
		GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, sizeof(Vertex), sizeof(float3));
		GL.EnableVertexAttribArray(1);
		
		foreach (GlWindow window in childWindows) {
			window.OnLoad();
			window.OnResize(new(Size));
		}
		base.OnLoad();
	}
	protected override void OnUnload() {
		base.OnUnload();
	}

	protected override void OnResize(ResizeEventArgs e) {
		Context.MakeCurrent();
		GL.Viewport(0, 0, e.Width, e.Height);
		base.OnResize(e);
		
		//foreach (GlWindow window in childWindows) {
		//	window.OnResize(e);
		//}
	}

	private int frameCount = 0;
	private unsafe void UpdateMode() {
		Vector2 p = MouseState.Position;
		Vector2 winP = Location;

		if (p.X >= 0 && p.X < 32 && p.Y >= 0 && p.Y < 32) {
			// if (p.X >= winP.X && p.X < winP.X + 32 && p.Y >= winP.Y && p.Y < winP.Y + 32) {
			frameCount++;
			if (frameCount > 20) frameCount = 0;
			else return;
			isDesktop = !isDesktop;
			Console.WriteLine(isDesktop);
			
			Window x11Window = (Window)GLFW.GetX11Window(WindowPtr);
			IntPtr display = Xlib.XOpenDisplay(null);
			long[] arr = {(long)XLibUtils.XInternAtom(display, "_NET_WM_WINDOW_TYPE_DESKTOP", true)};
			
			Xlib.XLowerWindow(display, x11Window);
			
			if (!isDesktop) arr = new[]{(long)XLibUtils.XInternAtom(display, "_NET_WM_WINDOW_TYPE_NORMAL", true)};
			
			XLibUtils.XChangeProperty(display, x11Window, XLibUtils.XInternAtom(display, "_NET_WM_WINDOW_TYPE", true), Atom.Atom, 32, 0, arr, 1);

			Xlib.XCloseDisplay(display);
		}
	}
	protected override unsafe void OnRenderFrame(FrameEventArgs args) {
		Context.MakeCurrent();
		UpdateMode();
		if (isDesktop) GL.ClearColor(0,0,0,1f);
		else GL.ClearColor(0,0,0,.8f);
		GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
		GL.Viewport(0,0, Size.X, Size.Y);
		GL.Enable(EnableCap.Blend);
		
		float time = (float)DateTime.Now.TimeOfDay.TotalMilliseconds;
		vertices[0].col.x = math.abs(MathF.Sin(time * .005f));
		vertices[1].col.x = math.abs(MathF.Sin(time * .001f));
		vertices[2].col.x = math.abs(MathF.Sin(time * .0001f));
		
		//GL.BindBuffer(BufferTargetARB.ArrayBuffer, vbo);
		ibo.Bind();
		vao.Bind();
		vbo.Bind();
		shader.Bind();
		//ibo.Bind();
		GL.BufferData(BufferTargetARB.ArrayBuffer, vertices, BufferUsageARB.DynamicDraw);
		GL.DrawArrays(PrimitiveType.Triangles, 0, 3);
		
		base.OnRenderFrame(args);
		//GL.Flush();
		SwapBuffers();

		foreach (GlWindow window in childWindows) {
			window.ProcessEvents();
			
			window.OnRenderFrame(args);
		}
	}
}