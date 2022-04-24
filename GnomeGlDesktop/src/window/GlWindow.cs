using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL.Compatibility;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using X11;
using Window = OpenTK.Windowing.GraphicsLibraryFramework.Window;

namespace GnomeGlDesktop.window; 

public class GlWindow : GameWindow {
	public List<GlWindow> childWindows = new();

	public GlWindow(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings) : base(gameWindowSettings, nativeWindowSettings) {
		
	}
	
	float[] vertices = {
		-0.5f, -0.5f, 0.0f,//Bottom-left vertex
		0.5f, -0.5f, 0.0f, //Bottom-right vertex
		0.0f,  0.5f, 0.0f  //Top vertex
	};

	private BufferHandle vbo;
	private VertexArrayHandle vao;
	private ProgramHandle shader;

	private string vertShader = @"
#version 330 core
in vec3 a_pos;

void main() {
	gl_Position = vec4(a_pos, 1.0);
}
";

	private string fragShader = @"
#version 330 core
out vec4 frag_col;

void main() {
	frag_col = vec4(1.0,.5,.2,1.0);
}
";

	protected override unsafe void OnLoad() {
		Context.MakeCurrent();
		base.OnLoad();

		ShaderHandle vert = GL.CreateShader(ShaderType.VertexShader);
		ShaderHandle frag = GL.CreateShader(ShaderType.FragmentShader);
		
		GL.ShaderSource(vert, vertShader);
		GL.ShaderSource(frag, fragShader);
		
		GL.CompileShader(vert);
		GL.CompileShader(frag);
		
		shader = GL.CreateProgram();
		GL.AttachShader(shader, vert);
		GL.AttachShader(shader, frag);
		GL.LinkProgram(shader);
		
		vao = GL.GenVertexArray();
		GL.BindVertexArray(vao);
		
		vbo = GL.GenBuffer();
		GL.BindBuffer(BufferTargetARB.ArrayBuffer, vbo);
		GL.BufferData(BufferTargetARB.ArrayBuffer, vertices, BufferUsageARB.DynamicDraw);
		
		GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * 4, 0);
		GL.EnableVertexAttribArray(0);
		
		foreach (GlWindow window in childWindows) {
			window.OnLoad();
			window.OnResize(new(Size));
		}
	}
	protected override void OnUnload() {
		base.OnUnload();
	}

	protected override void OnResize(ResizeEventArgs e) {
		Context.MakeCurrent();
		GL.Viewport(0, 0, e.Width, e.Height);
		base.OnResize(e);
		
		foreach (GlWindow window in childWindows) {
			window.OnResize(e);
		}
	}
	protected override unsafe void OnRenderFrame(FrameEventArgs args) {
		Context.MakeCurrent();
		base.OnRenderFrame(args); 
		GL.ClearColor(0,0,0,.2f);
		GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
		
		GL.UseProgram(shader);
		GL.BindVertexArray(vao);
		GL.DrawArrays(PrimitiveType.Triangles, 0, 3);
		
		GL.Flush();
		SwapBuffers();

		foreach (GlWindow window in childWindows) {
			window.ProcessEvents();
			
			window.OnRenderFrame(args);
		}
	}
}