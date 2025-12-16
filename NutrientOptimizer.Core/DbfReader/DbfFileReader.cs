using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Linq;

namespace NutrientOptimizer.Core.DbfReader;

/// <summary>
/// Reads DBF (DBase) files and extracts field and record data.
/// Supports DBF format used by legacy databases.
/// </summary>
public class DbfFileReader
{
    private const byte END_OF_FIELDS_MARKER = 0x0D;
    private const byte DELETED_RECORD_MARKER = 0x2A;

    public class FieldDefinition
    {
        public string Name { get; set; } = string.Empty;
        public char Type { get; set; }
        public int Length { get; set; }
        public int Decimals { get; set; }
    }

    public class DbfFileInfo
    {
        public byte Version { get; set; }
        public DateTime LastUpdate { get; set; }
        public int RecordCount { get; set; }
        public int HeaderLength { get; set; }
        public int RecordLength { get; set; }
        public List<FieldDefinition> Fields { get; set; } = new();
    }

    /// <summary>
    /// Read a DBF file and return file info and records as dictionaries
    /// </summary>
    public static (DbfFileInfo info, List<Dictionary<string, string>> records) ReadDbfFile(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException($"DBF file not found: {filePath}");

        using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
        using var reader = new BinaryReader(fs);

        // Read and parse header
        var info = ReadHeader(reader);

        // Read field definitions
        ReadFieldDefinitions(reader, info);

        // Read records
        var records = ReadRecords(reader, info);

        return (info, records);
    }

    private static DbfFileInfo ReadHeader(BinaryReader reader)
    {
        reader.BaseStream.Seek(0, SeekOrigin.Begin);

        byte version = reader.ReadByte();
        byte lastUpdateYear = reader.ReadByte();
        byte lastUpdateMonth = reader.ReadByte();
        byte lastUpdateDay = reader.ReadByte();

        int recordCount = reader.ReadInt32();
        short headerLength = reader.ReadInt16();
        short recordLength = reader.ReadInt16();

        // Year is stored as years since 1900
        int fullYear = 1900 + lastUpdateYear;

        var info = new DbfFileInfo
        {
            Version = version,
            LastUpdate = new DateTime(fullYear, lastUpdateMonth, lastUpdateDay),
            RecordCount = recordCount,
            HeaderLength = headerLength,
            RecordLength = recordLength
        };

        return info;
    }

    private static void ReadFieldDefinitions(BinaryReader reader, DbfFileInfo info)
    {
        reader.BaseStream.Seek(32, SeekOrigin.Begin);

        while (true)
        {
            byte[] fieldHeader = reader.ReadBytes(32);

            if (fieldHeader.Length == 0 || fieldHeader[0] == END_OF_FIELDS_MARKER)
                break;

            if (fieldHeader.Length < 18)
            {
                // Invalid field header, skip
                break;
            }

            string fieldName = Encoding.ASCII.GetString(fieldHeader, 0, 11)
                .TrimEnd('\0')
                .Trim();

            char fieldType = (char)fieldHeader[11];
            int fieldLength = fieldHeader[16];
            int fieldDecimals = fieldHeader[17];

            info.Fields.Add(new FieldDefinition
            {
                Name = fieldName,
                Type = fieldType,
                Length = fieldLength,
                Decimals = fieldDecimals
            });
        }
    }

    private static List<Dictionary<string, string>> ReadRecords(BinaryReader reader, DbfFileInfo info)
    {
        var records = new List<Dictionary<string, string>>();

        reader.BaseStream.Seek(info.HeaderLength, SeekOrigin.Begin);

        for (int i = 0; i < info.RecordCount; i++)
        {
            byte deletionFlag = reader.ReadByte();

            if (deletionFlag == DELETED_RECORD_MARKER)
            {
                // Skip deleted record
                reader.ReadBytes(info.RecordLength - 1);
                continue;
            }

            var record = new Dictionary<string, string>();

            foreach (var field in info.Fields)
            {
                byte[] data = reader.ReadBytes(field.Length);
                string value = Encoding.ASCII.GetString(data)
                    .TrimEnd('\0')
                    .Trim();

                record[field.Name] = value;
            }

            records.Add(record);
        }

        return records;
    }

    /// <summary>
    /// Print file info to console
    /// </summary>
    public static void PrintFileInfo(DbfFileInfo info)
    {
        Console.WriteLine("DBF File Information:");
        Console.WriteLine($"  Version: {info.Version}");
        Console.WriteLine($"  Last Update: {info.LastUpdate:yyyy-MM-dd}");
        Console.WriteLine($"  Number of Records: {info.RecordCount}");
        Console.WriteLine($"  Header Length: {info.HeaderLength}");
        Console.WriteLine($"  Record Length: {info.RecordLength}");
        Console.WriteLine($"  Total Fields: {info.Fields.Count}");
        Console.WriteLine();

        Console.WriteLine("Fields:");
        foreach (var field in info.Fields)
        {
            Console.WriteLine($"  {field.Name,20} Type: {field.Type}  Length: {field.Length,3}  Decimals: {field.Decimals}");
        }
    }
}
