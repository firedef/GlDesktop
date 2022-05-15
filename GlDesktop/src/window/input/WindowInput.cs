using System.Collections;
using MathStuff.vectors;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace GlDesktop.window.input;

//TODO: move to backend
public class WindowInput {
	public const int mouseButtonCount = 8;
	public const int keyCount = 349;
	
	public MouseButtonFlags mouseButtons = MouseButtonFlags.none;

	//TODO: fix mouse position
	public float2 mousePosition;
	public float2 previousMousePosition;
	public float2 mouseDelta => mousePosition - previousMousePosition;

	public readonly BitArray keys = new(keyCount);

	public bool IsMouseButtonDown(MouseButtonFlags v) => (mouseButtons & v) != 0;
	public bool IsMouseButtonDown(MouseButton v) => IsMouseButtonDown((MouseButtonFlags)(1 << (int) v));
	public bool IsAnyMouseButtonDown() => mouseButtons != 0;

	public bool IsAnyKeyDown() {
		for (int i = 0; i < keyCount; i++)
			if (keys[i])
				return true;
		return false;
	}
	
	public bool IsKeyDown(Keys v) => keys[(int) v];

	public void SetMouseButton(MouseButton v, bool isDown) => mouseButtons = isDown 
		? mouseButtons | (MouseButtonFlags)(1 << (int)v) 
		: mouseButtons & ~(MouseButtonFlags)(1 << (int)v);

	public void SetKey(Keys v, bool isDown) => keys[(int)v] = isDown;

	public void Reset() {
		mouseButtons = 0;
		keys.SetAll(false);
	}
}