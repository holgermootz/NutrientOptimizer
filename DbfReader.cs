using System;
using System.IO;
using System.Text;
using System.Collections.Generic;

namespace DbfReader
{
    class Program
    {
        static void Main(string[] args)
        {
            string filePath = @"C:\Source\NutrientOptimizer\substances_win.dbf";
            
            if (!File.Exists(filePath))
            {
                Console.WriteLine($"File not found: {filePath}");
                return;
            }

            try
            {
                var records = ReadDbf(filePath);
                Console.WriteLine($"Successfully read {records.Count} records from DBF file\n");

                // Print records as JSON-like format for easy copying
                foreach (var record in records)
                {
                    Console.WriteLine(record);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        static List<string> ReadDbf(string filePath)
        {
            var records = new List<string>();
            
            using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            using (var reader = new BinaryReader(fs))
            {
                // Read header
                byte version = reader.ReadByte();
                byte lastUpdate1 = reader.ReadByte();
                byte lastUpdate2 = reader.ReadByte();
                byte lastUpdate3 = reader.ReadByte();
                
                int numRecords = reader.ReadInt32();
                short headerLength = reader.ReadInt16();
                short recordLength = reader.ReadInt16();

                Console.WriteLine($"DBF Version: {version}");
                Console.WriteLine($"Number of Records: {numRecords}");
                Console.WriteLine($"Header Length: {headerLength}");
                Console.WriteLine($"Record Length: {recordLength}");
                Console.WriteLine();

                // Read field descriptors
                var fields = new List<(string name, char type, int length)>();
                
                fs.Seek(32, SeekOrigin.Begin);
                
                while (true)
                {
                    byte[] fieldHeader = reader.ReadBytes(32);
                    
                    if (fieldHeader[0] == 0x0D) // End of field descriptor
                        break;

                    // Field name (first 11 bytes, null-terminated)
                    string fieldName = Encoding.ASCII.GetString(fieldHeader, 0, 11).TrimEnd('\0');
                    char fieldType = (char)fieldHeader[11];
                    int fieldLength = fieldHeader[16];
                    
                    fields.Add((fieldName, fieldType, fieldLength));
                    Console.WriteLine($"Field: {fieldName} ({fieldType}) - Length: {fieldLength}");
                }
                
                Console.WriteLine($"\nTotal fields: {fields.Count}\n");

                // Read records
                fs.Seek(headerLength, SeekOrigin.Begin);

                for (int i = 0; i < numRecords; i++)
                {
                    byte deletedFlag = reader.ReadByte();
                    
                    if (deletedFlag == 0x2A) // Record marked as deleted
                        continue;

                    var record = new StringBuilder();
                    record.Append("{ ");

                    for (int j = 0; j < fields.Count; j++)
                    {
                        var (name, type, length) = fields[j];
                        byte[] data = reader.ReadBytes(length);
                        string value = Encoding.ASCII.GetString(data).Trim();

                        if (j > 0) record.Append(", ");
                        record.Append($"\"{name}\": \"{value}\"");
                    }

                    record.Append(" }");
                    records.Add(record.ToString());
                }
            }

            return records;
        }
    }
}
