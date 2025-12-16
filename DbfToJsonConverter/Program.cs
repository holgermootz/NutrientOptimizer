using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Linq;

namespace DbfToJsonConverter;

class Program
{
    static void Main(string[] args)
    {
        var baseDir = @"C:\Source\NutrientOptimizer";
        var initDir = Path.Combine(baseDir, "data", "init");
        Directory.CreateDirectory(initDir);

        var dbfFiles = new[]
        {
            ("substances_win.dbf", "substances_win.json"),
            ("substances_win_orig.dbf", "substances_win_orig.json")
        };

        foreach (var (dbfName, jsonName) in dbfFiles)
        {
            var dbfPath = Path.Combine(baseDir, dbfName);
            var jsonPath = Path.Combine(initDir, jsonName);

            if (!File.Exists(dbfPath))
            {
                Console.WriteLine($"? {dbfPath} not found");
                continue;
            }

            Console.WriteLine($"\n{'=',60}");
            Console.WriteLine($"Processing: {dbfName}");
            Console.WriteLine($"{'=',60}");

            try
            {
                var (fields, records) = ReadDbf(dbfPath);
                WriteJson(fields, records, jsonPath);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"? Error: {ex.Message}");
                Console.WriteLine($"  {ex.StackTrace}");
            }
        }

        Console.WriteLine($"\n{'=',60}");
        Console.WriteLine($"? JSON files created in: {initDir}");
        Console.WriteLine($"{'=',60}");
    }

    static (List<FieldDef> fields, List<Dictionary<string, string>> records) ReadDbf(string filePath)
    {
        var fields = new List<FieldDef>();
        var records = new List<Dictionary<string, string>>();

        using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
        using var reader = new BinaryReader(fs);

        // Read header
        byte version = reader.ReadByte();
        byte year = reader.ReadByte();
        byte month = reader.ReadByte();
        byte day = reader.ReadByte();

        int recordCount = reader.ReadInt32();
        short headerLength = reader.ReadInt16();
        short recordLength = reader.ReadInt16();

        Console.WriteLine($"DBF Version: {version}");
        Console.WriteLine($"Last Update: {1900 + year}-{month:D2}-{day:D2}");
        Console.WriteLine($"Record Count: {recordCount}");
        Console.WriteLine($"Header Length: {headerLength}");
        Console.WriteLine($"Record Length: {recordLength}");

        // Read field definitions (starting at offset 32)
        fs.Seek(32, SeekOrigin.Begin);

        while (true)
        {
            byte[] fieldHeader = reader.ReadBytes(32);

            if (fieldHeader.Length == 0 || fieldHeader[0] == 0x0D)
                break;

            if (fieldHeader.Length < 18)
                break;

            string fieldName = Encoding.ASCII.GetString(fieldHeader, 0, 11)
                .TrimEnd('\0')
                .Trim();

            if (string.IsNullOrEmpty(fieldName))
                break;

            char fieldType = (char)fieldHeader[11];
            int fieldLength = fieldHeader[16];
            int fieldDecimals = fieldHeader[17];

            fields.Add(new FieldDef
            {
                Name = fieldName,
                Type = fieldType.ToString(),
                Length = fieldLength,
                Decimals = fieldDecimals
            });
        }

        Console.WriteLine($"Fields found: {fields.Count}");
        foreach (var field in fields)
        {
            Console.WriteLine($"  {field.Name,20} Type: {field.Type} Length: {field.Length}");
        }

        // Read records
        fs.Seek(headerLength, SeekOrigin.Begin);

        for (int i = 0; i < recordCount; i++)
        {
            byte deletionFlag = reader.ReadByte();

            if (deletionFlag == (byte)'*')
            {
                // Skip deleted record
                reader.ReadBytes(recordLength - 1);
                continue;
            }

            var record = new Dictionary<string, string>();

            foreach (var field in fields)
            {
                byte[] data = reader.ReadBytes(field.Length);
                string value = Encoding.ASCII.GetString(data)
                    .TrimEnd('\0')
                    .Trim();

                record[field.Name] = value;
            }

            records.Add(record);
        }

        return (fields, records);
    }

    static void WriteJson(List<FieldDef> fields, List<Dictionary<string, string>> records, string outputPath)
    {
        var output = new
        {
            metadata = new
            {
                timestamp = DateTime.UtcNow,
                recordCount = records.Count,
                fieldCount = fields.Count,
                fields = fields
            },
            records = records
        };

        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        };

        string json = JsonSerializer.Serialize(output, options);
        File.WriteAllText(outputPath, json, Encoding.UTF8);

        Console.WriteLine($"? Wrote {records.Count} records to {outputPath}");
        Console.WriteLine($"  File size: {new FileInfo(outputPath).Length / 1024.0:F1} KB");
    }
}

class FieldDef
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public int Length { get; set; }
    public int Decimals { get; set; }
}
