#include "pch.h"

int (WINAPI* Real_MessageBoxW)(HWND hWnd, LPCWSTR lpText, LPCWSTR lpCaption, UINT uType) = MessageBoxW;

int WINAPI Mine_MessageBoxW(HWND hWnd, LPCWSTR lpText, LPCWSTR lpCaption, UINT uType);

int WINAPI Mine_MessageBoxW(HWND hWnd, LPCWSTR lpText, LPCWSTR lpCaption, UINT uType)
{
	return Real_MessageBoxW(nullptr, L"Hello from Mine_MessageBoxW", L"Hello", MB_OK);
}

BOOL APIENTRY DllMain(HMODULE hModule,
	DWORD  ul_reason_for_call,
	LPVOID lpReserved
)
{
	switch (ul_reason_for_call)
	{
	case DLL_PROCESS_ATTACH:
	{
		MessageBoxW(nullptr, L"Hello from Real_MessageBoxW", L"Hello", MB_OK);
		DetourTransactionBegin();
		DetourUpdateThread(GetCurrentThread());
		DetourAttach(&(PVOID&)Real_MessageBoxW, Mine_MessageBoxW);
		DetourTransactionCommit();
		MessageBoxW(nullptr, L"Hello from Real_MessageBoxW", L"Hello", MB_OK);
	}
	case DLL_THREAD_ATTACH:
	case DLL_THREAD_DETACH:
	case DLL_PROCESS_DETACH:
		break;
	}
	return TRUE;
}