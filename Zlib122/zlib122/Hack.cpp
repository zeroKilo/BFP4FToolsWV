#include "stdafx.h"
#include <windows.h>
#include <stdio.h>
#include <stdlib.h>
#pragma comment(lib, "detours.lib")
#include "detours.h"

LPSTR newArguments = "+webSiteHostName \"localhost\" "
					 "+battleFundsHostName \"localhost\" "
					 "+survey 0 "
					 "+dc 1 "
					 "+sessionId 1234 "
					 "+lang en "
					 "+soldierName \"mysoldier\" "
					 "+multi 1 "
					 "+frontendUrl \"http://localhost/\" "
					 "+autoLogin 1 "
					 "+loggedIn \"true\" "
					 "+webBrowser 0";

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

//Typedefs
typedef int (__stdcall * WINMAIN)(HINSTANCE hInstance, HINSTANCE hPrevInstance, LPSTR lpCmdLine, int nShowCmd);
typedef int (__cdecl * PROTOCONNECT)(int pState, int iSecure, char *pAddr, int uAddr, __int16 iPort);
typedef int (* SOMELOG1)(int a1, char *Format, ...);

WINMAIN orgMain;
PROTOCONNECT orgProtoConnect;
SOMELOG1 orgLog;

DWORD buf;
DWORD ret = 0x00BB397A;
DWORD foundMaps[1000];
DWORD fMapCount = 0;

void PrintTagLabel(DWORD t, FILE* fp)
{
	t >>= 8;
	printf(" ");
	for(int i=0; i<4; i++)
	{
		BYTE b = (BYTE)(((t >> (6 * (3-i))) & 0x3F) + 0x20);
		printf("%c", b);
		fprintf(fp, "%c", b);
	}
}

void DecodeTagInfoMap(BYTE* buf)
{
	DWORD address = (DWORD)buf;
	for(int i=0; i<fMapCount; i++)
		if(foundMaps[i] == address)
			return;
	foundMaps[fMapCount++] = address;		
	FILE* fp = fopen ("TagMapLog.txt", "a+");
	printf("Visited TagInfoMap @0x%08X\n", buf);
	fprintf(fp, "Visited TagInfoMap @0x%08X\n", buf);
	while(true)
	{
		DWORD tagLabel = *(DWORD*)(buf);		
		DWORD next = *(BYTE*)(buf + 5);
		PrintTagLabel(tagLabel, fp);
		if(next != 0)
		{
			for(int i=0; i<next; i++)
			{
				printf(" %02X", buf[i]);
				fprintf(fp," %02X", buf[i]);
			}
			printf("\n");
			fprintf(fp,"\r\n");
			buf += next;
		}
		else
		{
			for(int i=0; i<8; i++)
			{
				printf(" %02X", buf[i]);
				fprintf(fp, " %02X", buf[i]);
			}
			printf("...\n");
			fprintf(fp, "...\r\n");
			break;
		}
	}
	fclose(fp);
}

void __declspec(naked) specialHook()
{
	_asm
	{
		pushad;
		mov buf, ecx;
	}
	DecodeTagInfoMap((BYTE*)buf);
	_asm
	{
		popad;
		movzx ecx, [ecx + 6];
		mov edx, [esp + 0x24];
		jmp ret;
	}
}

int __stdcall WinMain(HINSTANCE hInstance, HINSTANCE hPrevInstance, LPSTR lpCmdLine, int nShowCmd)
{
	printf("Detoured WinMain called...\n");
	printf("Arguments: %s\n", lpCmdLine);
	//printf("Arguments after: %s\n", newArguments);
	return orgMain(hInstance, hPrevInstance, lpCmdLine, nShowCmd);
}

int __cdecl ProtoSSLConnect(int pState, int iSecure, char *pAddr, int uAddr, unsigned short iPort)
{
	printf("ProtoSSLConnect was used: Address = %s:%d\n" , pAddr, iPort);
	return orgProtoConnect(pState, iSecure, pAddr, uAddr, iPort);
}

signed int __cdecl VerifyCertificate(int a1, size_t *a2, char a3)
{
	printf("VerifyCertificate was used\n");
	return 0;
}

int myLogger(int a1, char *Format, ...)
{
	va_list va;
	va_start(va, Format);
	printf("[Logger1]");
	vprintf(Format, va);
	printf("\n");
	return orgLog(a1, Format, va);
}

TCHAR szFileName[MAX_PATH + 1];

void Hack_Init()
{
	OpenConsole();	
	GetModuleFileName(NULL, szFileName, MAX_PATH + 1);
	printf("Exe name: %S\n", szFileName);
	if(wcsstr(szFileName, L"_w32ded.exe") != NULL)
	{
		printf("Hi from inside the server!\n");
		DetourFunction((PBYTE)0xAFA180, (PBYTE)VerifyCertificate);
	}
	else
	{
		printf("Hi from inside the game!\n");
		orgMain = (WINMAIN)DetourFunction((PBYTE)0x4059B0, (PBYTE)WinMain);
		orgProtoConnect = (PROTOCONNECT)DetourFunction((PBYTE)0xB18C50, (PBYTE)ProtoSSLConnect);
		//orgLog = (SOMELOG1)DetourFunction((PBYTE)0xA7B740, (PBYTE)myLogger);
		DetourFunction((PBYTE)0xB18580, (PBYTE)VerifyCertificate);
		DetourFunction((PBYTE)0xBB3972, (PBYTE)specialHook);
		FILE* fp = fopen ("TagMapLog.txt", "w");
		fclose(fp);
	}
	printf("Detours done.\n");
	MessageBoxA(0, "Attach now!", 0, 0);
}