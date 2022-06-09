#ifndef GLDESKTOP_CORE_LIBRARY_H
#define GLDESKTOP_CORE_LIBRARY_H

#include "src/x11/X11InputHandler.h"

extern "C" {
    void startXInput();
    void updateXInput();
    void endXInput();

    InputData* getInputDataPtr();
    int getKeySymCount();
    void writeKeySymTo(KeySym* ptr, int destSize);

    int keySymToUnicode(KeySym keySym, InputDataMods mods);
}

#endif //GLDESKTOP_CORE_LIBRARY_H
