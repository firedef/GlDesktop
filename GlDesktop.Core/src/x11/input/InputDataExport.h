#ifndef GLDESKTOP_CORE_INPUTDATAEXPORT_H
#define GLDESKTOP_CORE_INPUTDATAEXPORT_H
#include <array>
#include "XInputStructs.h"

class InputDataExport {
public:
    static int getKeySymCount(InputData& data) {
        return (int) data.pressedButtons.size();
    }
    
    static void writeKeySymsTo(InputData& data, KeySym* ptr, int destSize) {
        int i = 0;
        for (const auto &button: data.pressedButtons) {
            *ptr++ = button;
            i++;
            if (i >= destSize) return; 
        }
    } 
};


#endif //GLDESKTOP_CORE_INPUTDATAEXPORT_H
