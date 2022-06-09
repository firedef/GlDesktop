using System.Runtime.InteropServices;

namespace GlDesktop.external; 

public static class CoreApi {
    private const string libName = "data/libs/libGlDesktop_Core.so";

    [DllImport(libName, EntryPoint = "startXInput")]
    public static extern void StartXInput();
    
    [DllImport(libName, EntryPoint = "updateXInput")]
    public static extern void UpdateXInput();
    
    [DllImport(libName, EntryPoint = "endXInput")]
    public static extern void EndXInput();
    
    [DllImport(libName, EntryPoint = "getInputDataPtr")]
    public static extern unsafe XInputData*  GetInputDataPtr();
    
    [DllImport(libName, EntryPoint = "getKeySymCount")]
    public static extern int GetKeySymCount();
    
    [DllImport(libName, EntryPoint = "writeKeySymTo")]
    public static extern unsafe void WriteKeySymTo(KeySym* dest, int size);
    
    [DllImport(libName, EntryPoint = "keySymToUnicode")]
    public static extern int KeySymToUnicode(KeySym keySym, XInputDataMods mods);
}