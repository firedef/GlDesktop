#ifndef GLDESKTOP_CORE_X11INPUTHANDLER_H
#define GLDESKTOP_CORE_X11INPUTHANDLER_H

#include "input/Input.h"

class X11InputHandler {
public:
    InputData inputData{};
    Display* display;
    Window win;
    
    void start() {
        display = XOpenDisplay(nullptr);
        win = DefaultRootWindow(display);

        XIEventMask mask[2];
        mask[0].deviceid = XIAllDevices;
        mask[0].mask_len = XIMaskLen(XI_LASTEVENT);
        mask[0].mask = static_cast<unsigned char*>(calloc(mask[0].mask_len, sizeof(char)));

        XISetMask(mask[0].mask, XI_ButtonPress);
        XISetMask(mask[0].mask, XI_ButtonRelease);
        XISetMask(mask[0].mask, XI_Motion);
        XISetMask(mask[0].mask, XI_KeyPress);
        XISetMask(mask[0].mask, XI_KeyRelease);

        XISelectEvents(display, win, mask, 1);
        XSync(display, false);
        free(mask[0].mask);
    }
    
    void update() {
        Input::updateInput(display, inputData);
    }
    
    void end() {
        XCloseDisplay(display);
    }
};


#endif //GLDESKTOP_CORE_X11INPUTHANDLER_H
