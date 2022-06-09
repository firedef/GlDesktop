#include <xkbcommon/xkbcommon.h>
#include <cwctype>
#include <X11/XKBlib.h>
#include "Input.h"

wint_t Input::keySymToUnicodeChar(KeySym keySym, InputDataMods mods) {
    auto result = (wint_t) xkb_keysym_to_utf32(keySym);
    if (mods & InputDataMods::capsLock) {
        if (iswlower(result)) result = towupper(result);
        else result = towlower(result);
    }
    if (mods & InputDataMods::numLock) {
        if (keySym == XKB_KEY_KP_Insert) result = '0';
        if (keySym == XKB_KEY_KP_End) result = '1';
        if (keySym == XKB_KEY_KP_Down) result = '2';
        if (keySym == XKB_KEY_KP_Page_Down) result = '3';
        if (keySym == XKB_KEY_KP_Left) result = '4';
        if (keySym == XKB_KEY_KP_Begin) result = '5';
        if (keySym == XKB_KEY_KP_Right) result = '6';
        if (keySym == XKB_KEY_KP_Home) result = '7';
        if (keySym == XKB_KEY_KP_Up) result = '8';
        if (keySym == XKB_KEY_KP_Page_Up) result = '9';

        if (keySym == XKB_KEY_KP_Divide) result = '/';
        if (keySym == XKB_KEY_KP_Multiply) result = '*';
        if (keySym == XKB_KEY_KP_Subtract) result = '-';
        if (keySym == XKB_KEY_KP_Add) result = '+';
        if (keySym == XKB_KEY_KP_Delete) result = '.';
    }

    return result;
}

void Input::updateKeyPress(Display* display, InputData& inputData, XGenericEventCookie* cookie, XIDeviceEvent* event) {
    if (cookie->evtype != XI_KeyPress) return;

    KeyCode key = (*(XIDeviceEvent*) cookie->data).detail;

    int lang = event->group.effective;
    int mods = event->mods.effective;
    int level = mods & InputDataMods::shift;

    KeySym keySym = XkbKeycodeToKeysym(display, key, lang, level);

    inputData.modifiers = static_cast<InputDataMods>(mods);

    if (keySym == XKB_KEY_Shift_L ||
        keySym == XKB_KEY_Shift_R ||
        keySym == XKB_KEY_Control_L ||
        keySym == XKB_KEY_Control_R ||
        keySym == XKB_KEY_ISO_Next_Group)
        return;
    inputData.pressedButtons.insert(keySym);
}

void Input::updateKeyRelease(Display* display, InputData& inputData, XGenericEventCookie* cookie, XIDeviceEvent* event) {
    if (cookie->evtype != XI_KeyRelease) return;

    KeyCode key = (*(XIDeviceEvent*) cookie->data).detail;

    int lang = event->group.effective;
    int mods = event->mods.effective;
    int level = mods & InputDataMods::shift;

    KeySym keySym = XkbKeycodeToKeysym(display, key, lang, level);

    inputData.modifiers = static_cast<InputDataMods>(mods);
    inputData.pressedButtons.erase(keySym);
}

void Input::updatePointerPos(InputData& inputData, XGenericEventCookie* cookie, XIDeviceEvent* event) {
    if (cookie->evtype != XI_Motion) return;

    double* val = event->valuators.values;
    if (XIMaskIsSet(event->valuators.mask, 0)) inputData.position_x = *val++;
    if (XIMaskIsSet(event->valuators.mask, 1)) inputData.position_y = *val++;
    if (XIMaskIsSet(event->valuators.mask, 2)) inputData.scroll_x = *val++;
    if (XIMaskIsSet(event->valuators.mask, 3)) inputData.scroll_y = *val++;
}

void Input::updateMouseButtonPress(InputData& inputData, XGenericEventCookie* cookie) {
    if (cookie->evtype != XI_ButtonPress) return;

    int button = (*(XIDeviceEvent*) cookie->data).detail;
    inputData.mouseButtons = static_cast<InputDataMouseButtons>(inputData.mouseButtons | (1 << (button - 1)));
}

void Input::updateMouseButtonRelease(InputData& inputData, XGenericEventCookie* cookie) {
    if (cookie->evtype != XI_ButtonRelease) return;

    int button = (*(XIDeviceEvent*) cookie->data).detail;
    inputData.mouseButtons = static_cast<InputDataMouseButtons>(inputData.mouseButtons & ~(1 << (button - 1)));
}

void Input::updateInput(Display *display, InputData &inputData) {
    XEvent ev;

    XGenericEventCookie* cookie = &ev.xcookie;

    XNextEvent(display, &ev);

    if (!XGetEventData(display, cookie) || cookie->type != GenericEvent) {
        XFreeEventData(display, cookie);
        return;
    }

    auto* event = static_cast<XIDeviceEvent*>(cookie->data);

    updatePointerPos(inputData, cookie, event);
    updateMouseButtonPress(inputData, cookie);
    updateMouseButtonRelease(inputData, cookie);
    updateKeyPress(display, inputData, cookie, event);
    updateKeyRelease(display, inputData, cookie, event);

    XFreeEventData(display, cookie);
}