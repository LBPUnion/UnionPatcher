using System;
using System.IO;
using System.Linq;
using System.Buffers.Binary;

/*
Linux ELF header refspec: https://refspecs.linuxfoundation.org/elf/gabi4+/ch4.eheader.html
Wikipedia entry on ELF: https://en.wikipedia.org/wiki/Executable_and_Linkable_Format
*/

namespace LBPUnion.UnionPatcher; 

public class ElfFile {
    internal const int MinimumSize = 52; 
        
    private enum WordSize : byte {
        ThirtyTwoBits = 0x01,
        SixtyFourBits = 0x02,
    }

    private enum Endianness : byte {
        Little = 0x01,
        Big = 0x02,
    }

    private enum InstructionSetArchitecture : UInt16 {
        PowerPC = 0x15, //64-bit PowerPC (PS3)
        ARM = 0x28, //32-bit ARM (Vita)
    }

    public string Name { get; } = "Binary Blob";

    public bool IsValid { get; } = false;
    public bool? Is64Bit { get; } = null;
    public bool? IsBigEndian { get; } = null;
    public string Architecture { get; } = null;

    public byte[] Contents { get; } = null;

    public ElfFile(byte[] fileContents) {
        if(fileContents.Length < MinimumSize)
            return;

        IsValid = fileContents[..0x04].SequenceEqual(new byte[] {
            0x7F,
            (byte)'E',
            (byte)'L',
            (byte)'F',
        });
            
        if(!IsValid) return;

        byte identClassValue = fileContents[0x04];
        byte identDataValue = fileContents[0x05];

        if(identClassValue == (byte)WordSize.ThirtyTwoBits || identClassValue == (byte)WordSize.SixtyFourBits)
            Is64Bit = identClassValue == (byte)WordSize.SixtyFourBits;

        if(identDataValue == (byte)Endianness.Little || identDataValue == (byte)Endianness.Big)
            IsBigEndian = identDataValue == (byte)Endianness.Big;

        Architecture = GetFileArchitecture(fileContents, IsBigEndian == true);

        Contents = fileContents;
    }

    public ElfFile(FileInfo file) : this(File.ReadAllBytes(file.FullName)) {
        Name = file.Name;
    }

    public ElfFile(string fileName) : this(new FileInfo(fileName)) {}

    private string GetFileArchitecture(byte[] elfHeader, bool isBigEndian) {
        byte[] architectureBytes = elfHeader[0x12..0x14];
        UInt16 fileArch = (isBigEndian) ?
            BinaryPrimitives.ReadUInt16BigEndian(architectureBytes) :
            BinaryPrimitives.ReadUInt16LittleEndian(architectureBytes);

        foreach(InstructionSetArchitecture arch in Enum.GetValues(typeof(InstructionSetArchitecture))) {
            if(fileArch == (UInt16)arch)
                return arch.ToString();
        }
        return null;
    }
}