#ifndef GLDESKTOP_CORE_INPUT_H
#define GLDESKTOP_CORE_INPUT_H

#include "XInputStructs.h"
#include <string>

class Input {
public:
    static wint_t keySymToUnicodeChar(KeySym keySym, InputDataMods mods);

    static void updateInput(Display* display, InputData& inputData);
    
    static void updateKeyPress(Display *display, InputData &inputData, XGenericEventCookie *cookie, XIDeviceEvent *event);

    static void updateKeyRelease(Display *display, InputData &inputData, XGenericEventCookie *cookie, XIDeviceEvent *event);

    static void updatePointerPos(InputData &inputData, XGenericEventCookie *cookie, XIDeviceEvent *event);

    static void updateMouseButtonPress(InputData &inputData, XGenericEventCookie *cookie);

    static void updateMouseButtonRelease(InputData &inputData, XGenericEventCookie *cookie);
};


#endif //GLDESKTOP_CORE_INPUT_H
