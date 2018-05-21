#include "stdafx.h"
#include <windows.h>
#include <stdio.h>
#include <tchar.h>
#include "Hack.h"
#pragma pack(1)



extern "C" BOOL WINAPI DllMain(HINSTANCE hInst,DWORD reason,LPVOID)
{
	static HINSTANCE hL;
	if (reason == DLL_PROCESS_ATTACH)
	{
		hL = LoadLibrary(_T(".\\zlib122_org.dll"));
		if (!hL) return false;
		Hack_Init();
	}
	if (reason == DLL_PROCESS_DETACH)
		FreeLibrary(hL);
	return TRUE;
}

#pragma comment(linker, "/export:adler32=zlib122_org.adler32")

#pragma comment(linker, "/export:compress2=zlib122_org.compress2")

#pragma comment(linker, "/export:compress=zlib122_org.compress")

#pragma comment(linker, "/export:compressBound=zlib122_org.compressBound")

#pragma comment(linker, "/export:get_crc_table=zlib122_org.get_crc_table")

#pragma comment(linker, "/export:crc32=zlib122_org.crc32")

#pragma comment(linker, "/export:deflateSetDictionary=zlib122_org.deflateSetDictionary")

#pragma comment(linker, "/export:deflatePrime=zlib122_org.deflatePrime")

#pragma comment(linker, "/export:deflateBound=zlib122_org.deflateBound")

#pragma comment(linker, "/export:deflate=zlib122_org.deflate")

#pragma comment(linker, "/export:deflateEnd=zlib122_org.deflateEnd")

#pragma comment(linker, "/export:deflateCopy=zlib122_org.deflateCopy")

#pragma comment(linker, "/export:deflateReset=zlib122_org.deflateReset")

#pragma comment(linker, "/export:deflateParams=zlib122_org.deflateParams")

#pragma comment(linker, "/export:deflateInit2_=zlib122_org.deflateInit2_")

#pragma comment(linker, "/export:deflateInit_=zlib122_org.deflateInit_")

#pragma comment(linker, "/export:gzsetparams=zlib122_org.gzsetparams")

#pragma comment(linker, "/export:gzungetc=zlib122_org.gzungetc")

#pragma comment(linker, "/export:gzwrite=zlib122_org.gzwrite")

#pragma comment(linker, "/export:gzprintf=zlib122_org.gzprintf")

#pragma comment(linker, "/export:gzputc=zlib122_org.gzputc")

#pragma comment(linker, "/export:gzputs=zlib122_org.gzputs")

#pragma comment(linker, "/export:gzflush=zlib122_org.gzflush")

#pragma comment(linker, "/export:gzrewind=zlib122_org.gzrewind")

#pragma comment(linker, "/export:gzeof=zlib122_org.gzeof")

#pragma comment(linker, "/export:gzclose=zlib122_org.gzclose")

#pragma comment(linker, "/export:gzerror=zlib122_org.gzerror")

#pragma comment(linker, "/export:gzclearerr=zlib122_org.gzclearerr")

#pragma comment(linker, "/export:gzopen=zlib122_org.gzopen")

#pragma comment(linker, "/export:gzdopen=zlib122_org.gzdopen")

#pragma comment(linker, "/export:gzread=zlib122_org.gzread")

#pragma comment(linker, "/export:gzgetc=zlib122_org.gzgetc")

#pragma comment(linker, "/export:gzgets=zlib122_org.gzgets")

#pragma comment(linker, "/export:gzseek=zlib122_org.gzseek")

#pragma comment(linker, "/export:gztell=zlib122_org.gztell")

#pragma comment(linker, "/export:inflateBackInit_=zlib122_org.inflateBackInit_")

#pragma comment(linker, "/export:inflateBack=zlib122_org.inflateBack")

#pragma comment(linker, "/export:inflateBackEnd=zlib122_org.inflateBackEnd")

#pragma comment(linker, "/export:inflateReset=zlib122_org.inflateReset")

#pragma comment(linker, "/export:inflateInit2_=zlib122_org.inflateInit2_")

#pragma comment(linker, "/export:inflateInit_=zlib122_org.inflateInit_")

#pragma comment(linker, "/export:inflate=zlib122_org.inflate")

#pragma comment(linker, "/export:inflateEnd=zlib122_org.inflateEnd")

#pragma comment(linker, "/export:inflateSetDictionary=zlib122_org.inflateSetDictionary")

#pragma comment(linker, "/export:inflateSync=zlib122_org.inflateSync")

#pragma comment(linker, "/export:inflateSyncPoint=zlib122_org.inflateSyncPoint")

#pragma comment(linker, "/export:inflateCopy=zlib122_org.inflateCopy")

#pragma comment(linker, "/export:uncompress=zlib122_org.uncompress")

#pragma comment(linker, "/export:zlibVersion=zlib122_org.zlibVersion")

#pragma comment(linker, "/export:zlibCompileFlags=zlib122_org.zlibCompileFlags")

#pragma comment(linker, "/export:zError=zlib122_org.zError")


