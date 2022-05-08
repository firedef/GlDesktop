using GnomeGlDesktop.gl.render;
using GnomeGlDesktop.utils;
using GnomeGlDesktop.window;
using ImGuiNET;
using MathStuff;
using MathStuff.vectors;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using X11;

namespace GnomeGlDesktop.imgui; 

public abstract class ImGuiWindow : BasicWindow {
	public static bool shiftPressed = false;
	public static bool ctrlPressed = false;
	public static bool altPressed = false;
	public static bool superPressed = false;
	protected readonly bool enableImGui;
	public bool isDesktop = true;
	
	private ImGuiController _controller = null!;

	public virtual Vector2i fbSize => Size;
	public virtual float2 scaleFactor => float2.one;

	public ImGuiWindow(NativeWindowSettings nativeWindowSettings, bool enableImGui) : base(nativeWindowSettings) {
		this.enableImGui = enableImGui;
		GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
	}
	
	protected override void OnLoad() {
		base.OnLoad();
		if (!enableImGui) return;
		
		//GlContext.global.MakeCurrent();
		//Context.MakeCurrent();
		_controller = new(fbSize.X, fbSize.Y);
	}

	protected override void OnResize(ResizeEventArgs e) {
		base.OnResize(e);
		if (!enableImGui) return;
		
		GlContext.global.MakeCurrent();
		//Context.MakeCurrent();
		_controller.WindowResized(fbSize.X, fbSize.Y);
	}

	protected override unsafe void OnRenderFrame() {
		shiftPressed = KeyboardState.IsKeyDown(Keys.LeftShift) || KeyboardState.IsKeyDown(Keys.RightShift);
		ctrlPressed = KeyboardState.IsKeyDown(Keys.LeftControl) || KeyboardState.IsKeyDown(Keys.RightControl);
		altPressed = KeyboardState.IsKeyDown(Keys.LeftAlt) || KeyboardState.IsKeyDown(Keys.RightAlt);
		superPressed = KeyboardState.IsKeyDown(Keys.LeftSuper) || KeyboardState.IsKeyDown(Keys.RightSuper);

		if (!enableImGui || isDesktop) return;

		_controller.scaleFactor = scaleFactor;
		_controller.Update(this);
		Layout();
		_controller.Render();
	}

	private static BlendingFactor sFactor = BlendingFactor.SrcAlpha;
	private static BlendingFactor dFactor = BlendingFactor.OneMinusSrcAlpha;
	private static int sId = 0;
	private static int dId = 0;
	private bool blend = false;
	private bool doubleBuffer = false;
	protected virtual void Layout() {
		ImGui.PushStyleColor(ImGuiCol.WindowBg, ((color) "#1c202bfa").ToVec4());
		ImGui.PushStyleColor(ImGuiCol.Border, ((color) "#394259").ToVec4());
		
		ImGui.PushStyleColor(ImGuiCol.TitleBg, ((color) "#1c202b").ToVec4());
		ImGui.PushStyleColor(ImGuiCol.TitleBgCollapsed, ((color) "#1c202bfa").ToVec4());
		ImGui.PushStyleColor(ImGuiCol.TitleBgActive, ((color) "#394259").ToVec4());
		
		ImGui.PushStyleColor(ImGuiCol.Separator, ((color) "#394259").ToVec4());
		
		ImGui.PushStyleColor(ImGuiCol.Text, ((color) "#b0c6ff").ToVec4());
		
		//ImGui.ShowDemoWindow();
		ImGui.PushStyleVar(ImGuiStyleVar.FrameRounding, 4);
		ImGui.PushStyleVar(ImGuiStyleVar.WindowRounding, 8);

		BlendingFactor[] blendingModes = Enum.GetValues<BlendingFactor>();

		GL.BlendFunc(sFactor, dFactor);

		ImGui.Checkbox("thread sleep", ref useThreadSleepForRenderDelay);

		ImGui.SliderInt("framerate", ref renderFrequency, 2, 120);

		if (ImGui.Button("exit")) {
			base.Close();
		}

		
	}

	protected override void OnTextInput(TextInputEventArgs e) {
		base.OnTextInput(e);
		GlContext.global.MakeCurrent();
		//Context.MakeCurrent();
		if (enableImGui) _controller.PressChar((char) e.Unicode);
	}

	protected override void OnMouseWheel(MouseWheelEventArgs e) {
		base.OnMouseWheel(e);
		GlContext.global.MakeCurrent();
		//Context.MakeCurrent();
		if (enableImGui) _controller.OnMouseScroll(e.Offset);
	}
}