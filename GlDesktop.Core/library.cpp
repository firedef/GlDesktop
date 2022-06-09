#include "library.h"

#include "src/x11/input/InputDataExport.h"

X11InputHandler inputHandler{};

void startXInput() {
    inputHandler.start();
}

void updateXInput() {
    inputHandler.update();
}

void endXInput() {
    inputHandler.end();
}

InputData* getInputDataPtr() {
    return &inputHandler.inputData;
}

int getKeySymCount() {
    return InputDataExport::getKeySymCount(inputHandler.inputData);
}

void writeKeySymTo(KeySym *ptr, int destSize) {
    InputDataExport::writeKeySymsTo(inputHandler.inputData, ptr, destSize);
}

int keySymToUnicode(KeySym keySym, InputDataMods mods) {
    return Input::keySymToUnicodeChar(keySym, mods);
}
