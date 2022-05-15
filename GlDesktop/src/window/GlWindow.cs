using GlDesktop.gl;
using GlDesktop.gl.render;
using GlDesktop.gl.render.targets;
using GlDesktop.gl.shaders;
using GlDesktop.imgui;
using GlDesktop.imgui.addons;
using GlDesktop.objects.mesh;
using GlDesktop.utils;
using ImGuiNET;
using MathStuff;
using MathStuff.vectors;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL.Compatibility;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using X11;
using Vector2 = OpenTK.Mathematics.Vector2;
using Window = X11.Window;

namespace GlDesktop.window;
/*
public class GlWindow : ImGuiWindow {
	public GlWindow(NativeWindowSettings nativeWindowSettings, int i) : base(nativeWindowSettings, i == 0) {
		
	}

	private static Mesh<Vertex, ushort>? _mesh;
	private static ShaderProgram shader;

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
	
	private static RenderTarget _renderTarget;
	private static float quality => _renderTarget.framebufferSize.X / 1920f;
	private static float targetQuality = 1;
	private static int samples = 0;
	private static int maxSamples = 0;
	public override Vector2i fbSize => _renderTarget.framebufferSize;
	public override float2 scaleFactor => quality;

	protected override void OnLoad() {
		GlContext.global.MakeCurrent();
		if (childs.Count != 0) {
			GL.GetInteger(GetPName.MaxFramebufferSamples, ref maxSamples);
			_renderTarget = new((int)(1920 * targetQuality), (int)(1080 * targetQuality));
		
			shader = new();
			Console.WriteLine(shader.LoadVertexShaderString(vertShader));
			Console.WriteLine(shader.LoadFragmentShaderString(fragShader));
			shader.Bind();
	
			_mesh = new();
			Vertex p0 = new(new(-.5f,-.5f), float3.front, color.softBlue, float2.zero, float2.zero);
			Vertex p1 = new(new(-.5f, .5f), float3.front, color.softPurple, float2.zero, float2.zero);
			Vertex p2 = new(new( .5f, .5f), float3.front, color.softRed, float2.zero, float2.zero);
			Vertex p3 = new(new( .5f,-.5f), float3.front, color.softYellow, float2.zero, float2.zero);
			_mesh.AddQuad(p0, p1, p2, p3);
			_mesh!.Buffer();
		}
		
		base.OnLoad();
		Context.MakeCurrent();
		GLFW.SwapInterval(1);
	}

	private static ImGuiFileDialog? _fileDialog;
	private static float opacity = 1f;
	private static float opacityDesktopModeMul = 1f;
	private static float opacityAppModeMul = .8f;
	private static int bitsPerTex = 16;
	
	protected override void Layout() {
		_fileDialog ??= new("/mnt/sdb/");
		base.Layout();
		ImGui.SliderFloat("opacity", ref opacity, 0, 1);
		ImGui.SliderFloat("opacity desktop mode mul", ref opacityDesktopModeMul, 0, 1);
		ImGui.SliderFloat("opacity app mode mul", ref opacityAppModeMul, 0, 1);

		bool upd = false;
		upd |= ImGui.SliderFloat("rendering quality", ref targetQuality, 0, 1);
		upd |= ImGui.SliderInt("samples", ref samples, 0, (int) math.log2(maxSamples), (1 << samples).ToString());
		upd |= ImGui.SliderInt("color bits", ref bitsPerTex, 0, 32);
		
		if (ImGuiElements.ButtonPurple("recreate render buffers") || upd) {
			InternalFormat format = bitsPerTex switch {
				<= 4 => InternalFormat.Rgb4,
				<= 5 => InternalFormat.Rgb5,
				<= 8 => InternalFormat.Rgb8,
				<= 10 => InternalFormat.Rgb10,
				<= 12 => InternalFormat.Rgb12,
				<= 16 => InternalFormat.Rgb16f,
				_ => InternalFormat.Rgb32f,
			};
			_renderTarget.RecreateRenderBuffers((int)(1920 * targetQuality), (int)(1080 * targetQuality), (samples < 2 || targetQuality < 1f) ? 0 : (1 << samples));
			OnResize(new(Size));
		}
		_fileDialog?.Layout();
	}

	protected override void OnResize(ResizeEventArgs e) {
		Context.MakeCurrent();
		//GlContext.global.MakeCurrent();
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

		GlContext.global.MakeCurrent();
		if (childs.Count != 0)
		{
			_renderTarget.Begin();
			
			GL.ClearColor(0, 0, 0, (isDesktop ? opacityDesktopModeMul : opacityAppModeMul) * opacity);

			GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
			GL.Viewport(0, 0, _renderTarget.framebufferSize.X, _renderTarget.framebufferSize.Y);
			
			GL.Enable(EnableCap.Blend);

			float time = (float)DateTime.Now.TimeOfDay.TotalMilliseconds;
			_mesh!.vertices.ptr[0].color.rF = math.abs(MathF.Sin(time * .005f));
			_mesh!.vertices.ptr[1].color.rF = math.abs(MathF.Sin(time * .001f));
			_mesh!.vertices.ptr[2].color.rF = math.abs(MathF.Sin(time * .0001f));

			shader.Bind();
			_mesh.Buffer();
			_mesh!.Draw();
			
			base.OnRenderFrame();
			
			_renderTarget.End();
		}
		
		Context.MakeCurrent();
		_renderTarget.BlitToScreen(Size);
	}
}*/