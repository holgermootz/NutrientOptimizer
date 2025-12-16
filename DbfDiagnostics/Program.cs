using System;
using System.IO;
using System.Text;
using System.Collections.Generic;

/// <summary>
/// Advanced DBF file diagnostics tool.
/// Inspects DBF file structure byte-by-byte to understand encoding and field layout.
/// </summary>
class DbfDiagnostics
{
    static void Main()
    {
        var filePath = @"C:\Source\NutrientOptimizer\substances_win.dbf";

        if (!File.Exists(filePath))
        {
            Console.WriteLine($"File not found: {filePath}");
            return;
        }

        Console.WriteLine("================================================================================");
        Console.WriteLine("DBF FILE DIAGNOSTICS");
        Console.WriteLine("================================================================================\n");

        using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
        using (var reader = new BinaryReader(fs))
        {
            // Read header
            byte version = reader.ReadByte();
            byte year = reader.ReadByte();
            byte month = reader.ReadByte();
            byte day = reader.ReadByte();
            int recordCount = reader.ReadInt32();
            short headerLength = reader.ReadInt16();
            short recordLength = reader.ReadInt16();

            Console.WriteLine("FILE HEADER:");
            Console.WriteLine($"  Version Byte: 0x{version:X2} ({version})");
            Console.WriteLine($"  Last Update: {1900 + year}-{month:D2}-{day:D2}");
            Console.WriteLine($"  Record Count: {recordCount}");
            Console.WriteLine($"  Header Length: {headerLength} bytes");
            Console.WriteLine($"  Record Length: {recordLength} bytes");
            Console.WriteLine();

            // Skip reserved bytes (20 bytes total after version byte, we've read 7)
            fs.Seek(32, SeekOrigin.Begin);

            Console.WriteLine("FIELD DEFINITIONS (Raw Hex Dump):");
            Console.WriteLine("?????????????????????????????????????????????????????????????");
            
            int fieldCount = 0;
            while (true)
            {
                byte[] fieldHeader = reader.ReadBytes(32);
                
                if (fieldHeader.Length < 32)
                {
                    Console.WriteLine("  [EOF - Less than 32 bytes read]");
                    break;
                }

                if (fieldHeader[0] == 0x0D)
                {
                    Console.WriteLine("  [END OF FIELD MARKER: 0x0D]");
                    break;
                }

                fieldCount++;

                // Print raw hex
                Console.WriteLine($"\nField {fieldCount}:");
                Console.Write("  Hex: ");
                for (int i = 0; i < 32; i++)
                {
                    Console.Write($"{fieldHeader[i]:X2} ");
                    if ((i + 1) % 16 == 0) Console.Write("\n       ");
                }
                Console.WriteLine();

                // Try to extract field info
                string fieldName = Encoding.ASCII.GetString(fieldHeader, 0, 11)
                    .TrimEnd('\0')
                    .Replace("\0", "·");  // Show null chars

                char fieldType = (char)fieldHeader[11];
                int fieldLength = fieldHeader[16];
                int fieldDecimals = fieldHeader[17];

                Console.WriteLine($"  Name (11 bytes): '{fieldName}'");
                Console.WriteLine($"  Type (byte 11): 0x{fieldHeader[11]:X2} = '{fieldType}'");
                Console.WriteLine($"  Reserved (12-15): {string.Join(", ", new[] { fieldHeader[12], fieldHeader[13], fieldHeader[14], fieldHeader[15] }.Select(b => $"0x{b:X2}"))}");
                Console.WriteLine($"  Length (byte 16): {fieldLength}");
                Console.WriteLine($"  Decimals (byte 17): {fieldDecimals}");
                Console.WriteLine($"  Reserved (18-31): {string.Join(", ", new byte[14].Select((_, i) => fieldHeader[18 + i]).Select(b => $"0x{b:X2}"))}");
            }

            Console.WriteLine($"\nTotal Fields Detected: {fieldCount}");
            Console.WriteLine();

            // Try to read first record
            Console.WriteLine("FIRST RECORD (Raw Hex Dump):");
            Console.WriteLine("?????????????????????????????????????????????????????????????");
            
            fs.Seek(headerLength, SeekOrigin.Begin);
            byte deleteFlag = reader.ReadByte();
            Console.WriteLine($"  Deletion Flag (byte 0): 0x{deleteFlag:X2} ('{(char)deleteFlag}') {(deleteFlag == 0x2A ? "[DELETED]" : "[ACTIVE]")}");

            byte[] recordData = reader.ReadBytes(Math.Min(recordLength - 1, 256));
            Console.WriteLine($"  First 256 bytes of record data:");
            Console.Write("  Hex: ");
            for (int i = 0; i < recordData.Length; i++)
            {
                Console.Write($"{recordData[i]:X2} ");
                if ((i + 1) % 16 == 0) Console.Write("\n       ");
            }
            Console.WriteLine();

            Console.WriteLine();
            Console.WriteLine("  ASCII Interpretation (printable only):");
            string ascii = Encoding.ASCII.GetString(recordData)
                .Replace("\0", "·");  // Show nulls
            Console.WriteLine($"  '{ascii}'");

            Console.WriteLine();
            Console.WriteLine("POSSIBLE ENCODINGS:");
            Console.WriteLine("?????????????????????????????????????????????????????????????");
            Console.WriteLine($"  UTF-8:     {Encoding.UTF8.GetString(recordData).Replace("\0", "·")}");
            Console.WriteLine($"  Latin1:    {Encoding.Latin1.GetString(recordData).Replace("\0", "·")}");
            Console.WriteLine($"  CP850:     {Encoding.GetEncoding(850).GetString(recordData).Replace("\0", "·")}");
            Console.WriteLine($"  CP1252:    {Encoding.GetEncoding(1252).GetString(recordData).Replace("\0", "·")}");
        }

        Console.WriteLine("\n================================================================================");
        Console.WriteLine("END OF DIAGNOSTICS");
        Console.WriteLine("================================================================================");
    }
}
