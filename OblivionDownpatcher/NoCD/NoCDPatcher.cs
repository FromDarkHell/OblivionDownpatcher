using System.IO;

namespace OblivionDownpatcher.NoCD {
    public static class NoCDPatcher {
        /*
         * The original 1.0 version checks to see if a CD ROM is installed
         *      This is why the `NoCDRom` signature(s) is necessary as CD ROMs fade away:
         *      Assembly:
         *          call dword ptr ds:[<&CopyFileA>]
         *          call oblivion.404AC0
         *          test al, al
         *          jne oblivion.40DF73 
         *      The `jne` instruction at the end is replaced with a regular jmp instruction
         * It also checks to see if you have an Oblivion CD inserted
         *      For this I use the `CDCheck` signature(s)
         *      Assembly
         *          call dword ptr ds:[<&MessageBoxA>]
         *          jmp oblivion.40F1E4
         *          mov al, byte ptr ds:[AD5170]
         *          test al, al
         *          jne oblivion.40E035
         *      Same thing with the `jne` instruction at the end like in the `NoCDROM` patch
        */

        public static string PatchedCDROMSignature = "FF 15 ?? ?? ?? ?? E8 ?? ?? ?? ?? 90 90 E9 ?? ?? ?? ?? 90";
        public static string PatchedCDCheckSignature = "FF 15 ?? ?? ?? ?? E9 ?? ?? ?? ?? A0 ?? ?? ?? ?? 90 90 E9 ?? ?? ?? ?? 90";

        public static string UnpatchedCDROMSignature = "FF 15 ?? ?? ?? ?? E8 ?? ?? ?? ?? 84 C0 0F 85";
        public static string UnpatchedCDCheckSignature = "FF 15 ?? ?? ?? ?? E9 ?? ?? ?? ?? A0 ?? ?? ?? ?? 84 C0 0F 85";

        public static byte[] PatchedCDROM =   { 0x90, 0x90, 0xE9, 0x98, 0x00, 0x00, 0x00, 0x90 };
        public static byte[] PatchedCDCheck = { 0x90, 0x90, 0xE9, 0xB6, 0x00, 0x00, 0x00, 0x90 };

        public static bool IsCDPatched(string filePath) {
            byte[] exe = File.ReadAllBytes(filePath);
            int index = BytesFinder.FindIndex(exe, PatchedCDROMSignature);
            if (index == -1) // Unable to find the patched version, we're not patched
                return false;

            int CDIndex = BytesFinder.FindIndex(exe, PatchedCDCheckSignature); 
            if (CDIndex == -1) // Unable to find the patched version, we're not patched
                return false;

            return true;
        } 

        public static void ApplyCDPatch(string filepath) {
            if(!IsCDPatched(filepath)) {
                byte[] exe = File.ReadAllBytes(filepath);
                int index = BytesFinder.FindIndex(exe, UnpatchedCDROMSignature);
                if (index == -1) return; // Unable to find it, return
                index += 11;
                for (int i = 0; i < PatchedCDROM.Length; i++) {
                    exe[index + i] = PatchedCDROM[i];
                }

                index = BytesFinder.FindIndex(exe, UnpatchedCDCheckSignature);
                if (index == -1) return;
                index += 0x10;
                for(int i = 0; i < PatchedCDCheck.Length; i++) {
                    exe[index + i] = PatchedCDCheck[i];
                }

                if(!File.Exists(filepath + ".bak"))
                    File.Move(filepath, filepath + ".bak"); // Backup the old exe

                File.WriteAllBytes(filepath, exe); // Patch the current exe
            }
            else {
                if (File.Exists(filepath + ".bak")) {
                    File.Delete(filepath);
                    File.Move(filepath + ".bak", filepath);
                }
            }
        }
    }
}
