#include "stdafx.h"
#include <windows.h>
#include <stdio.h>
#include <stdlib.h>
#pragma comment(lib, "detours.lib")
#include "detours.h"

bool isServer = false;
DWORD ptrEnum, cntEnum;
TCHAR szFileName[MAX_PATH + 1];
const BYTE JMP		= 0xEB;
const BYTE JZ		= 0x74;
const BYTE JNZ		= 0x75;
DWORD* pOpenAddress;
BYTE* pAccept;
DWORD patch1 = 0x9EF9C7;
DWORD patch2 = 0x9EFA1F;
bool conIsOpen = false;

void OpenConsole()
{
			AllocConsole();
			freopen("conin$","r",stdin);
			freopen("conout$","w",stdout);
			freopen("conout$","w",stderr);
			HWND consoleHandle = GetConsoleWindow();
			MoveWindow(consoleHandle,1,1,680,480,1);
			printf("Console initialized.\n");
}

void PrintEnum()
{
	char* desc;
	DWORD value;	
	FILE* fp = fopen ("EnumLog.txt", "a+");
	fprintf(fp, "Found Enum at 0x%08x with %d Entries\n", ptrEnum, cntEnum);
	for(int i=0; i<cntEnum; i++)
	{
		desc = (char*)*(DWORD*)ptrEnum;
		value = *(DWORD*)(ptrEnum + 4);
		fprintf(fp, "\t%08x = %s\n", value, desc);
		ptrEnum += 8;
	}
	fclose(fp);
}

void __declspec(naked) EnumHook()
{
	_asm
	{
		mov edx, [esp + 4];
		mov ptrEnum, edx;
		mov edx, [esp + 8];
		mov cntEnum, edx;
		pushad;
	}
	PrintEnum();
	_asm
	{
		popad;
		mov eax, ecx;
		mov ecx, [esp + 4];
		mov [eax], ecx;
		mov [eax + 4], edx;
		retn 8;
	}
}

signed int __cdecl VerifyCertificate(int a1, size_t *a2, char a3)
{
	return 0;
}

void BlazeLogger(char* str)
{
	FILE* fp;
	if(isServer)
		fp = fopen ("BlazeLogServer.txt", "a+");
	else
		fp = fopen ("BlazeLog.txt", "a+");
	fprintf(fp, str);
	fclose(fp);
}

void DetourBlazeLogger(DWORD org, DWORD target)
{
	DWORD old;
	VirtualProtect((LPVOID)org, 6, PAGE_EXECUTE_READWRITE, &old);
	*((BYTE*)org) = 0x68;
	*((DWORD*)(org + 1)) = target;
	*((BYTE*)(org + 5)) = 0xC3;
}

void ClearFile(char* str)
{
	FILE* fp = fopen (str, "w");
	fclose(fp);
}


void EnableConsole(bool open)
{	
	DWORD oldProtect = 0;
	VirtualProtect((LPVOID)pOpenAddress, 4, PAGE_EXECUTE_READWRITE, &oldProtect);
	VirtualProtect((LPVOID)pAccept, 1, PAGE_EXECUTE_READWRITE, &oldProtect);
	if(open)
	{
		*pOpenAddress = 1;
		*pAccept = 1;	
	}
	else
	{
		*pOpenAddress = 0;
		*pAccept = 0;	
	}
	conIsOpen = open;
}

DWORD WINAPI InputThread(LPVOID)
{
	while(true)
	{
		if(GetAsyncKeyState(VK_F12) & 1)
			EnableConsole(!conIsOpen);
		Sleep(10);
	}
	return 0;
}

DWORD WINAPI enable_ingame_console(LPVOID)
{	
	DWORD oldProtect = 0;
	HANDLE hGame = OpenProcess(PROCESS_ALL_ACCESS, false, GetCurrentProcessId());
	VirtualProtect((LPVOID)patch1, 1, PAGE_EXECUTE_READWRITE, &oldProtect);
	VirtualProtect((LPVOID)patch2, 1, PAGE_EXECUTE_READWRITE, &oldProtect);
	WriteProcessMemory(hGame, (PVOID)patch1, &JMP, sizeof(JMP), NULL);		
	WriteProcessMemory(hGame, (PVOID)patch2, &JMP, sizeof(JMP), NULL);
	DWORD dwClass = NULL;
	DWORD dwOpen = NULL;
	DWORD dwAccept = NULL;
	DWORD dwOpenAddress = NULL;
	DWORD dwInputAddress = NULL;
	DWORD dwRendDx9Base = 0;
	while(dwRendDx9Base == 0)
		dwRendDx9Base = (DWORD)GetModuleHandle(L"RendDX9.dll");
	DWORD dwReadAt = dwRendDx9Base + 0x62C13C;
	while(!dwClass)
		ReadProcessMemory(hGame, (PVOID)dwReadAt, &dwClass, 4, NULL);	
	pOpenAddress = (DWORD*)(dwClass + 4);
	while(!dwInputAddress)
		ReadProcessMemory(hGame, (PVOID)(dwClass + 696), &dwInputAddress, 4, NULL);
	dwInputAddress = dwInputAddress + 20;
	while(!dwAccept)
		ReadProcessMemory(hGame, (PVOID)dwInputAddress, &dwAccept, 4, NULL);		
	pAccept = (BYTE*)dwAccept;
	CloseHandle(hGame);	
	EnableConsole(false);
	CreateThread(0, 0, InputThread, 0, 0, 0);
	return 0;
}

void Hack_Init()
{
	GetModuleFileName(NULL, szFileName, MAX_PATH + 1);
	if(wcsstr(szFileName, L"_w32ded.exe") != NULL)
	{
		isServer = true;
		DetourFunction((PBYTE)0xAFA180, (PBYTE)VerifyCertificate);
		DetourBlazeLogger(0xAFFB30, (DWORD)BlazeLogger);
		ClearFile("BlazeLogServer.txt");
	}
	else
	{
		DetourFunction((PBYTE)0xAAE540, (PBYTE)VerifyCertificate);
		DetourBlazeLogger(0x00AB4410, (DWORD)BlazeLogger);
		//DetourFunction((PBYTE)0xBB3E90, (PBYTE)EnumHook);//Offset has to be determined
		
		//ClearFile("EnumLog.txt");
		ClearFile("BlazeLog.txt");
		//CreateThread(0, 0, enable_ingame_console, 0, 0, 0);
	}
	MessageBoxA(0, "Attach now!", 0, 0);
}
