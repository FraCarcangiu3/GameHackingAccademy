## Windows DLL Injector – Beginner Guide (English)

This project is a small C++ console app that injects a DLL into another running process (a game). The core idea: we force the target process to call `LoadLibraryA("C:\\path\\to\\your.dll")` by creating a thread inside it. The DLL then loads and can run code inside the target process.

Important: Use this project only where you have permission. DLL injection can violate ToS or anti-cheat policies.

### Goal of this document
- Explain everything from scratch, with no prior knowledge assumed.
- Help you build, run, and debug the injector and your DLL.
- Give you a mental model of what happens under the hood.

### Fundamentals (quick glossary)
- **Process**: a running program instance with its own virtual address space, handles, threads, and modules.
- **Thread**: an execution flow inside a process. A process can have many threads.
- **Handle**: a reference to a system object (process, thread, file, etc.). You need it to call APIs on that object.
- **Module**: an executable image loaded into the process (EXE or DLL). When a DLL loads, Windows calls its `DllMain`.
- **HMODULE**: the handle returned by `LoadLibrary*` representing the loaded module in a process.

### How Windows loads a DLL (normally)
1) The process calls `LoadLibraryA/W(path)`.
2) The loader maps the file, resolves imports (dependencies), and calls `DllMain(hinst, DLL_PROCESS_ATTACH, ...)`.
3) If `DllMain` returns TRUE, the DLL is considered successfully loaded.

Injection reuses the same mechanism by making another process execute the `LoadLibrary*` call.

### How the injector works – step by step
1) Find the target process (default: `wesnoth.exe`).
   - APIs used: `CreateToolhelp32Snapshot`, `Process32FirstW/Process32NextW`, comparing names case-insensitively.
2) Open the process with minimum required rights.
   - `PROCESS_CREATE_THREAD | PROCESS_QUERY_INFORMATION | PROCESS_VM_OPERATION | PROCESS_VM_WRITE | PROCESS_VM_READ`
3) Allocate memory inside the target process.
   - `VirtualAllocEx(..., PAGE_READWRITE)` to reserve space for the DLL path string.
4) Write the absolute DLL path into that memory.
   - `WriteProcessMemory` copies the string from our process to the target process.
5) Resolve `LoadLibraryA` address.
   - `GetModuleHandleA("kernel32.dll")` + `GetProcAddress("LoadLibraryA")` (the address is valid within the target, too).
6) Create a remote thread that runs `LoadLibraryA(remoteStringAddress)`.
   - `CreateRemoteThread(process, ..., LoadLibraryA, remoteString, ...)`
7) Wait for the thread to finish and read its exit code.
   - `GetExitCodeThread` returns a value: non-zero is the `HMODULE` of the loaded DLL (success).
8) Cleanup.
   - Free the remote memory (`VirtualFreeEx`) and close handles.

In `DLL_Injector/DLL_Injector/main.cpp` you’ll find:
- Clear console logs with the `[Injector]` prefix for each step.
- A pre-check that the DLL file exists before continuing.
- A short retry window (up to 30s) to wait for the process to appear.

### Why the wide-char (W) process APIs?
- `Process32FirstW/NextW` and `PROCESSENTRY32W` use Unicode and avoid issues with non‑ASCII process names.

### Why not `PROCESS_ALL_ACCESS`?
- Minimal privileges reduce permission/UAC failures and are the correct security practice.
- The selected rights are sufficient to create a thread and read/write/allocate memory.

### ANSI vs Unicode: `LoadLibraryA` vs `LoadLibraryW`
- `LoadLibraryA` expects an ANSI (8‑bit) string and works if the path has only simple ASCII characters.
- If your DLL path contains non‑ASCII characters, switch to `LoadLibraryW` and write a wide (UTF‑16) string in the target process.

### 32‑bit vs 64‑bit
- A 32‑bit injector/DLL cannot inject into a 64‑bit process with this technique (and vice versa).
- Always match architectures: game, DLL, and injector must all be the same.
- In Visual Studio: “Win32” = 32‑bit, “x64” = 64‑bit. Also check output folders (`Debug` vs `x64\\Debug`).

### Requirements
- Windows + Windows SDK (Visual Studio recommended).
- Matched architecture (see above).
- Often you must run the injector as Administrator if the target is elevated.

### Build (Visual Studio)
1) Open the solution and select the correct platform (Win32 for 32‑bit, x64 for 64‑bit).
2) Build `DLL_Injector` in Debug or Release.
3) Build your DLL (e.g., `Wesnoth_CodeCaveDLL`) with the same platform.

Typical output folders
- Win32 (32‑bit): `...\\YourDllProject\\Debug\\YourDll.dll`
- x64 (64‑bit): `...\\YourDllProject\\x64\\Debug\\YourDll.dll`

Pro tip: In File Explorer, right‑click your DLL → Shift + Copy as path, then paste it into the console to avoid typos.

### Usage (simplest path)
1) Start the game (`wesnoth.exe`).
2) Open a Command Prompt in the injector folder (or use absolute paths).
3) Run the injector with the absolute DLL path in quotes:

```bat
DLL_Injector.exe "C:\\Users\\you\\source\\repos\\Wesnoth_CodeCaveDLL\\Debug\\Wesnoth_CodeCaveDLL.dll"
```

If you pass no argument, the injector uses the hard‑coded `dll_path` in the source. Passing it via CLI is recommended.

### Reading the console output
Example (simplified):

```
[Injector] Target: wesnoth.exe
[Injector] DLL: C:\...\Wesnoth_CodeCaveDLL.dll
[Injector] PID found: 1234
[Injector] Remote memory at: 0x12345678 (size=...)
[Injector] Wrote DLL path to remote memory.
[Injector] LoadLibraryA address: 0x...
[Injector] Remote thread: 0x...
Remote thread exit code (HMODULE) = 0x5A0000
Done.
```

- If `HMODULE != 0`, the DLL loaded successfully.
- If `HMODULE == 0`, loading failed (see Troubleshooting).

### What `CreateRemoteThread` really does here
- We pass the entry function pointer for `LoadLibraryA` from `kernel32.dll`.
- We pass, as the thread parameter, the address (in the target) of the remote string with the absolute DLL path.
- The remote thread runs `LoadLibraryA(param)`, the loader maps the DLL, and returns the `HMODULE` which becomes the thread’s exit code.

### Why `LoadLibraryA` can fail
- The DLL file does not exist or is not accessible from the target process.
- Missing dependencies: your DLL imports another DLL which is not found in the target’s DLL search path.
- Architecture mismatch (64‑bit DLL in a 32‑bit process, or vice versa).
- Security software (AV/EDR) blocks injection or remote thread creation.
- Your DLL’s `DllMain` returns `FALSE` or crashes (e.g., doing heavy work in `DLL_PROCESS_ATTACH`).

### Golden rules for `DllMain`
- Keep it lightweight: avoid blocking I/O, thread joins, complex synchronization.
- If you need heavy work, spawn a new thread from `DllMain` and do it there.
- For first‑time debugging, a simple `MessageBoxA` in `DLL_PROCESS_ATTACH` is invaluable to confirm the DLL actually loads.

### Troubleshooting
1) “DLL path invalid or not found”
   - Don’t mix Win32 vs x64 output folders (`Debug` vs `x64\Debug`).
   - Use “Copy as path” on the freshly built DLL.
2) “Process not found / timed out”
   - The game isn’t running yet? The process name differs? Change `target` in code or add a CLI option.
3) `HMODULE == 0`
   - Missing DLL dependencies. Try copying your DLL next to the game EXE.
   - Non‑ASCII path with `LoadLibraryA` (switch to `LoadLibraryW`).
   - `DllMain` returns `FALSE` or crashes: add logs/MessageBox in `DLL_PROCESS_ATTACH` to confirm.
   - AV/EDR blocks `CreateRemoteThread` or injection.
4) “DLL loads but nothing happens in game”
   - Your offsets/codecave don’t match this exact game version. Re‑find them with a debugger/CE.
   - Ensure your DLL restores overwritten instructions correctly and calls `FlushInstructionCache` after patching.

### Useful extensions you can add
- Switch to `LoadLibraryW` and write a wide string into the target.
- CLI parameters: `DLL_Injector.exe <dllPath> [processName]`.
- Auto‑discover the DLL in typical output folders (`Debug`, `Release`, `x64\Debug`, etc.).
- File logging in addition to console logs.

### Legal note
Educational use only. Use responsibly and only where permitted. The authors are not liable for misuse.


