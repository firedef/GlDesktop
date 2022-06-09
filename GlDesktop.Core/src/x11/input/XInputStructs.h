#ifndef GLDESKTOP_CORE_XINPUTSTRUCTS_H
#define GLDESKTOP_CORE_XINPUTSTRUCTS_H

#include <X11/extensions/XInput2.h>
#include <unordered_set>
#include <vector>

enum InputDataMods : int {
    shift = 1,
    capsLock = 2,
    ctrl = 4,
    alt = 8,
    numLock = 16,
};

enum InputDataMouseButtons : int {
    left = 1,
    middle = 2,
    right = 4,
    wheelUp = 8,
    wheelDown = 16,

    reserved_0 = 32,
    reserved_1 = 64,

    back = 128,
    forward = 256,
};

struct InputData {
    InputDataMouseButtons mouseButtons;
    InputDataMods modifiers;

    double position_x;
    double position_y;
    double scroll_x;
    double scroll_y;

    std::unordered_set<KeySym> pressedButtons{};
};

#endif //GLDESKTOP_CORE_XINPUTSTRUCTS_H
