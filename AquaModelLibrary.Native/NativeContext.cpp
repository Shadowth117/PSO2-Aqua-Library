#include "NativeContext.h"

namespace AquaModelLibrary
{
    void NativeContext::Initialize()
    {
        CoInitializeEx( nullptr, COINIT_MULTITHREADED );
    }
}