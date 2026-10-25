#pragma once

#ifndef WINDHAWK_API_H
#define WINDHAWK_API_H

#include <windows.h>

#ifdef __cplusplus
extern "C" {
#endif

// Windhawk Mod Lifecycle Callbacks
BOOL Wh_ModInit(void);
void Wh_ModUninit(void);
void Wh_ModSettingsChanged(void);

// Logging
void Wh_Log(PCWSTR format, ...);

// Hooking
BOOL Wh_SetFunctionHook(void* targetFunction, void* hookFunction, void** originalFunction);

// Settings
PCWSTR Wh_GetStringSetting(PCWSTR name);
void Wh_FreeStringSetting(PCWSTR string);
int Wh_GetIntSetting(PCWSTR name);

#ifdef __cplusplus
}
#endif

#endif // WINDHAWK_API_H
