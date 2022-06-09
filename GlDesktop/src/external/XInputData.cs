namespace GlDesktop.external;

[Flags]
public enum XInputDataMods : int {
    shift = 1,
    capsLock = 2,
    ctrl = 4,
    alt = 8,
    numLock = 16
}

[Flags]
public enum XInputDataMouseButtons : int {
    left = 1,
    middle = 2,
    right = 4,
    wheelUp = 8,
    wheelDown = 16,

    reserved_0 = 32,
    reserved_1 = 64,

    back = 128,
    forward = 256
}

public struct XInputData {
    public XInputDataMouseButtons mouseButtons;
    public XInputDataMods modifiers;

    public double position_x;
    public double position_y;
    public double scroll_x;
    public double scroll_y;
    
    // std::unordered_set<KeySym> pressedButtons{};
}