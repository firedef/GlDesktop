using GnomeGlDesktop.gl;
using GnomeGlDesktop.imgui;
using GnomeGlDesktop.objects.mesh;
using GnomeGlDesktop.utils;
using MathStuff;
using MathStuff.vectors;
using OpenTK.Graphics.OpenGL.Compatibility;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using X11;
using Vector2 = OpenTK.Mathematics.Vector2;
using Window = X11.Window;

namespace GnomeGlDesktop.window;

public class GlWindow : ImGuiWindow {
	public GlWindow(NativeWindowSettings nativeWindowSettings, int i) : base(nativeWindowSettings, i == 0) {
		
	}

	private Mesh<Vertex, ushort>? _mesh;
	private ShaderProgram shader;

	private string vertShader = @"
#version 330 core
in layout(location=0) vec3 a_pos;
in layout(location=1) vec3 a_normal;
in layout(location=2) vec4 a_col;
in layout(location=3) vec2 a_uv0;
in layout(location=4) vec2 a_uv1;

out vec4 f_col;

void main() {
	gl_Position = vec4(a_pos, 1.0);
	f_col = a_col.bgra;
}
";

	private string fragShader = @"
#version 330 core
in vec4 f_col;
out vec4 frag_col;

void main() {
	frag_col = f_col;
}
";

	protected override unsafe void OnLoad() {
		Context.MakeCurrent();
		
		shader = new(vertShader, fragShader);
		shader.Bind();

		_mesh = new();
		Vertex p0 = new(new(-.5f,-.5f), float3.front, color.softBlue, float2.zero, float2.zero);
		Vertex p1 = new(new(-.5f, .5f), float3.front, color.softPurple, float2.zero, float2.zero);
		Vertex p2 = new(new( .5f, .5f), float3.front, color.softRed, float2.zero, float2.zero);
		Vertex p3 = new(new( .5f,-.5f), float3.front, color.softYellow, float2.zero, float2.zero);
		_mesh.AddQuad(p0, p1, p2, p3);
		_mesh!.Buffer();
		
		base.OnLoad();
	}

	protected override void OnResize(ResizeEventArgs e) {
		Context.MakeCurrent();
		GL.Viewport(0, 0, e.Width, e.Height);
		base.OnResize(e);
	}

	private int frameCount = 0;
	private unsafe void UpdateMode() {
		Vector2 p = MouseState.Position;
		Vector2 winP = Location;

		if (p.X is < 0 or >= 32 || p.Y is < 0 or >= 32) return;
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
	protected override unsafe void OnRenderFrame() {
		UpdateMode();
		GL.ClearColor(0, 0, 0, isDesktop ? 1f : .8f);

		GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
		GL.Viewport(0,0, Size.X, Size.Y);
		GL.Enable(EnableCap.Blend);
		
		float time = (float)DateTime.Now.TimeOfDay.TotalMilliseconds;
		_mesh!.vertices.ptr[0].color.rF = math.abs(MathF.Sin(time * .005f));
		_mesh!.vertices.ptr[1].color.rF = math.abs(MathF.Sin(time * .001f));
		_mesh!.vertices.ptr[2].color.rF = math.abs(MathF.Sin(time * .0001f));
		
		shader.Bind();
		_mesh.Buffer();
		_mesh!.Draw();
		
		base.OnRenderFrame();
	}
}