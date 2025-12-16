using System;
using System.IO;
using System.Linq;
using NutrientOptimizer.Core.Data;
using NutrientOptimizer.Core.DbfReader;

namespace NutrientOptimizer.Utilities;

/// <summary>
/// Command-line utility to import and inspect substance data from DBF files
/// </summary>
public class DbfImportUtility
{
    public static void Main(string[] args)
    {
        string dbfPath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "substances_win.dbf");

        if (!File.Exists(dbfPath))
        {
            Console.WriteLine("Error: substances_win.dbf not found at expected location.");
            Console.WriteLine($"Looked for: {dbfPath}");
            return;
        }

        Console.WriteLine("=== DBF File Information ===\n");

        try
        {
            // Read DBF structure
            var (fileInfo, records) = DbfFileReader.ReadDbfFile(dbfPath);
            DbfFileReader.PrintFileInfo(fileInfo);

            Console.WriteLine($"\n=== Imported {records.Count} Records ===\n");

            // Convert to Salt objects
            var salts = SubstanceImporter.ImportFromDbf(dbfPath);

            Console.WriteLine($"Successfully converted {salts.Count} substances to Salt objects.\n");

            // Group by category
            var grouped = salts.GroupBy(s => s.Category).ToList();

            foreach (var group in grouped)
            {
                Console.WriteLine($"\n{group.Key} ({group.Count()} items):");
                Console.WriteLine(new string('-', 60));

                foreach (var salt in group.OrderBy(s => s.Name))
                {
                    Console.WriteLine($"  {salt.Name}");
                    Console.WriteLine($"    Formula: {salt.Formula}");
                    Console.WriteLine($"    Group: {salt.Group}");
                    if (salt.IonContributions.Count > 0)
                    {
                        Console.WriteLine($"    Ions: {salt.GetIonSummary()}");
                    }
                }
            }

            // Save to JSON for external use
            string jsonPath = Path.Combine(
                Path.GetDirectoryName(dbfPath) ?? ".",
                "substances_data.json"
            );

            var json = System.Text.Json.JsonSerializer.Serialize(
                salts,
                new System.Text.Json.JsonSerializerOptions { WriteIndented = true }
            );

            File.WriteAllText(jsonPath, json);
            Console.WriteLine($"\n\nData exported to: {jsonPath}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            Console.WriteLine(ex.StackTrace);
        }
    }
}
