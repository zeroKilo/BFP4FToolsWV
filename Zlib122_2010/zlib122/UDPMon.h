LPTSTR lpszPipename1 = TEXT("\\\\.\\pipe\\UDPmon_send"); 
HANDLE hPipe1;
DWORD  cbToWrite1, cbWritten1; 
BOOL   fSuccess1; 

LPTSTR lpszPipename2 = TEXT("\\\\.\\pipe\\UDPmon_recv"); 
HANDLE hPipe2;
DWORD  cbToWrite2, cbWritten2;
BOOL   fSuccess2; 

bool SendDownThePipe1(char * str)
{
	cbToWrite1 = strlen(str);
	fSuccess1 = WriteFile(hPipe1, str, cbToWrite1, &cbWritten1, NULL);  
	return fSuccess1 && cbToWrite1 == cbWritten1;
}

bool SendDownThePipe2(char * str)
{
	cbToWrite2 = strlen(str);
	fSuccess2 = WriteFile(hPipe2, str, cbToWrite2, &cbWritten2, NULL);  
	return fSuccess2 && cbToWrite2 == cbWritten2;
}

bool InitUDPMon()
{
	hPipe1 = CreateFile(lpszPipename1, GENERIC_WRITE, FILE_SHARE_WRITE, 0, OPEN_EXISTING, FILE_ATTRIBUTE_NORMAL, 0);
	if (hPipe1 == INVALID_HANDLE_VALUE ) 
		return false;
	if(!SendDownThePipe1("BFP4F Server says hello on send pipe!\n"))
		return false;
	hPipe2 = CreateFile(lpszPipename2, GENERIC_WRITE, FILE_SHARE_WRITE, 0, OPEN_EXISTING, FILE_ATTRIBUTE_NORMAL, 0);
	if (hPipe2 == INVALID_HANDLE_VALUE ) 
		return false;
	return SendDownThePipe2("BFP4F Server says hello on recv pipe!\n");
}

DWORD readReturn;
DWORD readBuff;
DWORD readSize;
DWORD readEAX;
DWORD writeBuff;
DWORD writeSize;
DWORD writeEAX;

void PrintBitRead()
{
	DWORD tmpReturn = readReturn;
	DWORD tmpSize = readSize / 8;
	if(readSize % 8 != 0)
		tmpSize++;
	BYTE* tmpBuff = (BYTE*)readBuff;
	char* tmpHex = (char*)calloc(3, 1);
	for(int i = 0; i < tmpSize; i++)
	{
		sprintf(tmpHex, "%02X", tmpBuff[i]);
		SendDownThePipe2(tmpHex);
	}
	free(tmpHex);
	SendDownThePipe2("\n");
	readReturn = tmpReturn;
}

void PrintBitWrite()
{
	DWORD tmpSize = writeSize / 8;
	if(writeSize % 8 != 0)
		tmpSize++;
	BYTE* tmpBuff = (BYTE*)writeBuff;
	char* tmpHex = (char*)calloc(3, 1);
	for(int i = 0; i < tmpSize; i++)
	{
		sprintf(tmpHex, "%02X", tmpBuff[i]);
		SendDownThePipe1(tmpHex);
	}
	free(tmpHex);
	SendDownThePipe1("\n");
}

void __declspec(naked) BitStreamRead()
{
	_asm
	{
		//save eax
		mov readEAX, eax;
		//change return
		mov eax, [esp];
		mov readReturn, eax;
		mov eax, label_return;
		mov [esp], eax;
		//save buffer address
		mov eax, [esp + 4];
		mov readBuff, eax;
		//save buffer size
		mov eax, [esp + 8];
		mov readSize, eax;	
		//restore eax
		mov eax, readEAX;
		//rest of overwritten asm
		sub esp, 0xC;
		push ebp;
		mov ebp, [esp + 0x14];
		push 0x009FC288;
		ret;
label_return:
		//save regs
		pushad;
		//was success?
		test eax, eax;
		jz label_skip;
		//save to log
		call PrintBitRead;
label_skip:
		//restore regs
		popad;
		//restore return
		push readReturn;
		ret;
	}
}

void __declspec(naked) BitStreamWrite()
{
	_asm
	{
		//save eax
		mov writeEAX, eax;
		//save buffer address
		mov eax, [esp + 4];
		mov writeBuff, eax;
		//save buffer size
		mov eax, [esp + 8];
		mov writeSize, eax;	
		//restore eax
		mov eax, writeEAX;
		//save regs
		pushad;
		//save to log
		call PrintBitWrite;
		//restore regs
		popad;
		//rest of overwritten asm
		sub esp, 0xC;
		push ebp;
		push edi;
		push 0x009FCB05;
		ret;
	}
}