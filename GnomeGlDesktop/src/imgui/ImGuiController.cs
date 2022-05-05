using GnomeGlDesktop.gl;
using GnomeGlDesktop.gl.buffers;
using GnomeGlDesktop.gl.shaders;
using GnomeGlDesktop.window;
using ImGuiNET;
using MathStuff;
using MathStuff.vectors;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL.Compatibility;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using Vector4 = System.Numerics.Vector4;

namespace GnomeGlDesktop.imgui; 

public class ImGuiController : IDisposable {
	private static readonly Keys[] _allKeys = Enum.GetValues<Keys>();

	private readonly List<char> _pressedChars = new();

	private bool _frameStarted;
	private int _winWidth;
	private int _winHeight;
	private Texture _fontTex;
	private ShaderProgram _shader;
	private float2 _scaleFactor = float2.one;

	private VAO _vao;
	private VBO _vbo;
	private IBO _ibo;
	private int _vboSize;
	private int _iboSize;

	private const string _vertShaderSrc = @"
#version 330 core
uniform mat4 projection_matrix;
layout(location = 0) in vec2 in_position;
layout(location = 1) in vec2 in_texCoord;
layout(location = 2) in vec4 in_color;
out vec4 color;
out vec2 texCoord;
void main()
{
    gl_Position = projection_matrix * vec4(in_position, 0, 1);
    color = in_color;
    texCoord = in_texCoord;
}";
	
	private const string _fragShaderSrc = @"
#version 330 core
uniform sampler2D in_fontTexture;
in vec4 color;
in vec2 texCoord;
out vec4 outputColor;
void main()
{
    outputColor = color * texture(in_fontTexture, texCoord);
}";

	public unsafe ImGuiController(int width, int height) {
		_winWidth = width;
		_winHeight = height;
		
		ImGui.SetCurrentContext(ImGui.CreateContext());
		
		ImGuiIOPtr io = ImGui.GetIO();
		LoadFonts(io);
		
		
		io.BackendFlags |= ImGuiBackendFlags.RendererHasVtxOffset;
		
		CreateDeviceResources();
		SetKeyMappings();
		SetFrameData(1f / 60f);
		
		ImGui.NewFrame();
		_frameStarted = true;
	}

	private static unsafe void LoadFonts(ImGuiIOPtr io) {
		// get ranges for characters
		IntPtr commonRanges = GetRanges(io.Fonts.GetGlyphRangesDefault(), io.Fonts.GetGlyphRangesCyrillic());
		IntPtr asianRanges = GetRanges(io.Fonts.GetGlyphRangesChineseSimplifiedCommon(), io.Fonts.GetGlyphRangesJapanese(), io.Fonts.GetGlyphRangesKorean());
		IntPtr regularIconsRanges = LoadCharacters("data/fonts/FontAwesomeGlyphs.json");
		IntPtr brandIconsRanges = LoadCharacters("data/fonts/FontAwesomeBrandsGlyphs.json");
		
		// create merge font confing
		ImFontConfigPtr cfgPtr = ImGuiNative.ImFontConfig_ImFontConfig();
		cfgPtr.MergeMode = true;
		
		// load fonts
		LoadFont(io, "data/fonts/Comfortaa-VariableFont_wght.ttf", new(), commonRanges);
		LoadFont(io, "data/fonts/NotoSansJP-Regular.otf", cfgPtr, asianRanges);
		LoadFont(io, "data/fonts/fa-solid-900.ttf", cfgPtr, regularIconsRanges);
		LoadFont(io, "data/fonts/fa-brands-400.ttf", cfgPtr, brandIconsRanges);
		
		// merge loaded fonts into one
		io.Fonts.Build();
	}

	private static IntPtr LoadCharacters(string path) => GetRanges(ImGuiGlyphs.ParseGlyphsJson(File.ReadAllText(Path.GetFullPath(path))));

	private static unsafe IntPtr GetRanges(IEnumerable<char> characters) {
		ImFontGlyphRangesBuilderPtr builderPtr = ImGuiNative.ImFontGlyphRangesBuilder_ImFontGlyphRangesBuilder();
		foreach (char character in characters) builderPtr.AddChar(character);
		
		builderPtr.BuildRanges(out ImVector ranges);
		builderPtr.Destroy();
		
		return ranges.Data;
	}
	
	private static unsafe IntPtr GetRanges(params IntPtr[] addRanges) {
		ImFontGlyphRangesBuilderPtr builderPtr = ImGuiNative.ImFontGlyphRangesBuilder_ImFontGlyphRangesBuilder();
		foreach (IntPtr range in addRanges) builderPtr.AddRanges(range);
		
		builderPtr.BuildRanges(out ImVector ranges);
		builderPtr.Destroy();
		
		return ranges.Data;
	}

	private static void LoadFont(ImGuiIOPtr io, string path, ImFontConfigPtr cfgPtr, IntPtr ranges) {
		io.Fonts.AddFontFromFileTTF(path, 14, cfgPtr, ranges);
	}

	public void PressChar(char c) => _pressedChars.Add(c);

	private unsafe void CreateDeviceResources() {
		_vboSize = 10_000;
		_iboSize = 2_000;
		
		_vao = VAO.Generate();
		_vbo = VBO.Generate();
		_ibo = IBO.Generate();
		_vbo.Alloc(_vboSize);
		_ibo.Alloc(_iboSize);
		_vao.Bind();
		_vbo.Bind();

		RecreateFontDeviceTex();

		_shader = new(_vertShaderSrc, _fragShaderSrc);
		ProcessVertexAttribute(0, 2, VertexAttribPointerType.Float, false, sizeof(ImDrawVert), 0);
		ProcessVertexAttribute(1, 2, VertexAttribPointerType.Float, false, sizeof(ImDrawVert), 8);
		ProcessVertexAttribute(2, 4, VertexAttribPointerType.UnsignedByte, true, sizeof(ImDrawVert), 16);
		// GL.VertexArrayVertexBuffer(_vao, 0, _vbo, IntPtr.Zero, sizeof(ImDrawVert));
		// GL.VertexArrayElementBuffer(_vao, _ibo);
		//
		// GL.EnableVertexArrayAttrib(_vao, 0);
		// GL.VertexArrayAttribBinding(_vao, 0, 0);
		// GL.VertexArrayAttribFormat(_vao, 0, 2, VertexAttribType.Float, false, 0);
		//
		// GL.EnableVertexArrayAttrib(_vao, 1);
		// GL.VertexArrayAttribBinding(_vao, 1, 0);
		// GL.VertexArrayAttribFormat(_vao, 1, 2, VertexAttribType.Float, false, 8);
		//
		// GL.EnableVertexArrayAttrib(_vao, 2);
		// GL.VertexArrayAttribBinding(_vao, 2, 0);
		// GL.VertexArrayAttribFormat(_vao, 2, 4, VertexAttribType.UnsignedByte, true, 16);
	}
	
	private static void ProcessVertexAttribute(uint loc, int size_, VertexAttribPointerType type, bool normalized, int stride, int offset) {
		GL.VertexAttribPointer(loc, size_, type, normalized, stride, offset);
		GL.EnableVertexAttribArray(loc);
	}

	private void RecreateFontDeviceTex() {
		ImGuiIOPtr io = ImGui.GetIO();
		io.Fonts.GetTexDataAsRGBA32(out IntPtr pixels, out int width, out int height);
		
		_fontTex = new("imgui", width, height, pixels);
		_fontTex.SetMinFilter(TextureMinFilter.Linear);
		_fontTex.SetMagFilter(TextureMagFilter.Linear);
		
		io.Fonts.SetTexID((IntPtr) _fontTex.GLTexture);
		io.Fonts.ClearTexData();
	}

	private void SetKeyMappings() {
		ImGuiIOPtr io = ImGui.GetIO();
		io.KeyMap[(int)ImGuiKey.Tab] = (int)Keys.Tab;
		io.KeyMap[(int)ImGuiKey.LeftArrow] = (int)Keys.Left;
		io.KeyMap[(int)ImGuiKey.RightArrow] = (int)Keys.Right;
		io.KeyMap[(int)ImGuiKey.UpArrow] = (int)Keys.Up;
		io.KeyMap[(int)ImGuiKey.DownArrow] = (int)Keys.Down;
		io.KeyMap[(int)ImGuiKey.PageUp] = (int)Keys.PageUp;
		io.KeyMap[(int)ImGuiKey.PageDown] = (int)Keys.PageDown;
		io.KeyMap[(int)ImGuiKey.Home] = (int)Keys.Home;
		io.KeyMap[(int)ImGuiKey.End] = (int)Keys.End;
		io.KeyMap[(int)ImGuiKey.Delete] = (int)Keys.Delete;
		io.KeyMap[(int)ImGuiKey.Backspace] = (int)Keys.Backspace;
		io.KeyMap[(int)ImGuiKey.Enter] = (int)Keys.Enter;
		io.KeyMap[(int)ImGuiKey.Escape] = (int)Keys.Escape;
		io.KeyMap[(int)ImGuiKey.A] = (int)Keys.A;
		io.KeyMap[(int)ImGuiKey.C] = (int)Keys.C;
		io.KeyMap[(int)ImGuiKey.V] = (int)Keys.V;
		io.KeyMap[(int)ImGuiKey.X] = (int)Keys.X;
		io.KeyMap[(int)ImGuiKey.Y] = (int)Keys.Y;
		io.KeyMap[(int)ImGuiKey.Z] = (int)Keys.Z;
	}

	private void SetFrameData(float dt) {
		ImGuiIOPtr io = ImGui.GetIO();
		io.DisplaySize = new(_winWidth / _scaleFactor.x, _winHeight / _scaleFactor.y);
		io.DisplayFramebufferScale = new(_scaleFactor.x, _scaleFactor.y);
		io.DeltaTime = dt;
	}

	public void WindowResized(int width, int height) {
		_winWidth = width;
		_winHeight = height;
	}

	public void Render() {
		if (!_frameStarted) return;
		_frameStarted = false;
		
		//_vao.Bind();
		//_ibo.Bind();
		//_vbo.Bind();
		//_shader.Bind();
		
		ImGui.Render();
		RenderDrawData(ImGui.GetDrawData());
	}

	public void Update(BasicWindow win) {
		if (_frameStarted) ImGui.Render();
		
		//_vao.Bind();
		//_ibo.Bind();
		//_vbo.Bind();
		//_shader.Bind();
		
		SetFrameData((float)win.renderTimeDelta/1000);
		UpdateInput(win);
		_frameStarted = true;
		ImGui.NewFrame();
	}

	private unsafe void RenderDrawData(ImDrawDataPtr data) {
		int listCount = data.CmdListsCount;
		if (listCount == 0) return;

		for (int i = 0; i < listCount; i++) {
			ImDrawListPtr cmd = data.CmdListsRange[i];
			
			int vSize = cmd.VtxBuffer.Size * sizeof(ImDrawVert);
			if (vSize > _vboSize) {
				_vboSize = math.max(_vboSize << 1, vSize);
				_vbo.Alloc(_vboSize);
			}
			
			int iSize = cmd.IdxBuffer.Size * sizeof(ushort);
			if (iSize > _iboSize) {
				_iboSize = math.max(_iboSize << 1, iSize);
				_ibo.Alloc(_iboSize);
			}
		}

		ImGuiIOPtr io = ImGui.GetIO();
		Matrix4 mvp = Matrix4.CreateOrthographicOffCenter(0.0f, io.DisplaySize.X, io.DisplaySize.Y, 0.0f, -1.0f, 1.0f);
		_shader.Bind();
		GL.ProgramUniformMatrix4fv(_shader.programHandle, _shader.UniformLocation("projection_matrix"), 1, 0, (float*) &mvp);
		GL.ProgramUniform1i(_shader.programHandle, _shader.UniformLocation("in_fontTexture"), 0);
		_vao.Bind();
		_vbo.Bind();
		_ibo.Bind();
		data.ScaleClipRects(io.DisplayFramebufferScale);
		
		GL.Enable(EnableCap.Blend);
		GL.Enable(EnableCap.ScissorTest);
		//GL.BlendEquation(BlendEquationModeEXT.FuncAdd);
		//GL.BlendFunc(BlendingFactor.One, BlendingFactor.OneMinusSrcAlpha);
		// GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
		GL.Disable(EnableCap.CullFace);
		GL.Disable(EnableCap.DepthTest);

		for (int i = 0; i < listCount; i++) {
			ImDrawListPtr cmd = data.CmdListsRange[i];
			_vbo.BufferSubData(0, cmd.VtxBuffer.Size * sizeof(ImDrawVert), (void*)cmd.VtxBuffer.Data);
			_ibo.BufferSubData(0, cmd.IdxBuffer.Size * sizeof(ushort), (void*)cmd.IdxBuffer.Data);

			for (int j = 0; j < cmd.CmdBuffer.Size; j++) {
				ImDrawCmdPtr ptr = cmd.CmdBuffer[j];
				if (ptr.UserCallback != IntPtr.Zero) throw new NotImplementedException();
				
				GL.ActiveTexture(TextureUnit.Texture0);
				GL.BindTexture(TextureTarget.Texture2d, (TextureHandle) (int) ptr.TextureId);

				Vector4 clip = ptr.ClipRect;
				GL.Scissor((int)clip.X, _winHeight - (int)clip.W, (int)(clip.Z - clip.X), (int)(clip.W - clip.Y));

				if ((io.BackendFlags & ImGuiBackendFlags.RendererHasVtxOffset) != 0)
					GL.DrawElementsBaseVertex(PrimitiveType.Triangles, (int) ptr.ElemCount, DrawElementsType.UnsignedShort, (void*) (ptr.IdxOffset * sizeof(ushort)), (int) ptr.VtxOffset);
				else
					GL.DrawElements(PrimitiveType.Triangles, (int)ptr.ElemCount, DrawElementsType.UnsignedShort, (void*)(ptr.IdxOffset * sizeof(ushort)));
			}
		}
		//GL.Disable(EnableCap.Blend);
		GL.Disable(EnableCap.ScissorTest);
	}

	private void UpdateInput(BasicWindow win) {
		ImGuiIOPtr io = ImGui.GetIO();
		MouseState mouse = win.MouseState;
		KeyboardState keyboard = win.KeyboardState;

		io.MouseDown[0] = mouse[MouseButton.Left];
		io.MouseDown[1] = mouse[MouseButton.Right];
		io.MouseDown[2] = mouse[MouseButton.Middle];

		io.MousePos = new(mouse.X, mouse.Y);

		foreach (Keys key in _allKeys) {
			if (key == Keys.Unknown) continue;
			io.KeysDown[(int)key] = keyboard.IsKeyDown(key);
		}

		foreach (char c in _pressedChars) io.AddInputCharacter(c);
		_pressedChars.Clear();
		
		io.KeyCtrl = keyboard.IsKeyDown(Keys.LeftControl) || keyboard.IsKeyDown(Keys.RightControl);
		io.KeyAlt = keyboard.IsKeyDown(Keys.LeftAlt) || keyboard.IsKeyDown(Keys.RightAlt);
		io.KeyShift = keyboard.IsKeyDown(Keys.LeftShift) || keyboard.IsKeyDown(Keys.RightShift);
		io.KeySuper = keyboard.IsKeyDown(Keys.LeftSuper) || keyboard.IsKeyDown(Keys.RightSuper);
	}

	public void OnMouseScroll(Vector2 v) {
		ImGuiIOPtr io = ImGui.GetIO();
		io.MouseWheel = v.Y;
		io.MouseWheelH = v.X;
	}

	public void Dispose() {
		_vao.Dispose();
		_vbo.Dispose();
		_ibo.Dispose();
		_fontTex.Dispose();
		_shader.Dispose();
	}
}