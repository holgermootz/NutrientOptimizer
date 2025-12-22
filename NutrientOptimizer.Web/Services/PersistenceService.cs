using Microsoft.JSInterop;
using System;
using System.Text.Json;
using NutrientOptimizer.Core.Models;

namespace NutrientOptimizer.Web.Services;

/// <summary>
/// Service to persist application state to browser localStorage
/// </summary>
public class PersistenceService
{
    private readonly IJSRuntime _jsRuntime;
    private const string SETTINGS_KEY = "nutrient_optimizer_settings";
    private IJSObjectReference? _jsModule;

    public PersistenceService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    /// <summary>
    /// Initialize the JavaScript module (must be called before using the service)
    /// </summary>
    private async Task InitializeModuleAsync()
    {
        if (_jsModule == null)
        {
            try
            {
                _jsModule = await _jsRuntime.InvokeAsync<IJSObjectReference>("import", "./js/localStorage-interop.js");
                Console.WriteLine("[PersistenceService] JavaScript module loaded successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[PersistenceService] Error loading JavaScript module: {ex.Message}");
                throw;
            }
        }
    }

    /// <summary>
    /// Save application settings to localStorage
    /// </summary>
    public async Task SaveSettingsAsync(AppSettings settings)
    {
        try
        {
            await InitializeModuleAsync();
            
            var json = JsonSerializer.Serialize(settings);
            Console.WriteLine($"[PersistenceService] Attempting to save settings...");
            Console.WriteLine($"[PersistenceService]   - Serialized JSON length: {json.Length}");
            Console.WriteLine($"[PersistenceService]   - Contains {settings.SelectedSalts.Count} salts");
            Console.WriteLine($"[PersistenceService]   - Profile Index: {settings.SelectedProfileIndex}");
            Console.WriteLine($"[PersistenceService]   - JSON preview: {json.Substring(0, System.Math.Min(100, json.Length))}...");
            
            var success = await _jsModule!.InvokeAsync<bool>("setLocalStorage", SETTINGS_KEY, json);
            
            if (success)
            {
                Console.WriteLine("[PersistenceService] ? Settings saved to localStorage successfully");
            }
            else
            {
                Console.WriteLine("[PersistenceService] ? Failed to save settings to localStorage (setLocalStorage returned false)");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[PersistenceService] ERROR saving settings: {ex.GetType().Name}: {ex.Message}");
            Console.WriteLine($"[PersistenceService] Stack trace: {ex.StackTrace}");
        }
    }

    /// <summary>
    /// Load application settings from localStorage
    /// </summary>
    public async Task<AppSettings?> LoadSettingsAsync()
    {
        try
        {
            await InitializeModuleAsync();
            
            Console.WriteLine("[PersistenceService] Attempting to load settings from localStorage...");
            
            var json = await _jsModule!.InvokeAsync<string>("getLocalStorage", SETTINGS_KEY);
            
            Console.WriteLine($"[PersistenceService] Retrieved from localStorage: json is {(json == null ? "NULL" : $"'{json}' (length: {json?.Length})")}");
            
            if (string.IsNullOrEmpty(json))
            {
                Console.WriteLine("[PersistenceService] No saved settings found in localStorage (json is null or empty)");
                return null;
            }

            Console.WriteLine($"[PersistenceService] Retrieved JSON length: {json.Length}");
            Console.WriteLine($"[PersistenceService] JSON preview: {json.Substring(0, System.Math.Min(100, json.Length))}...");
            
            var settings = JsonSerializer.Deserialize<AppSettings>(json);
            
            if (settings == null)
            {
                Console.WriteLine("[PersistenceService] Deserialization returned null");
                return null;
            }
            
            Console.WriteLine("[PersistenceService] Settings loaded from localStorage successfully");
            Console.WriteLine($"[PersistenceService]   - Profile Index: {settings.SelectedProfileIndex}");
            Console.WriteLine($"[PersistenceService]   - Selected Salts Count: {settings.SelectedSalts.Count}");
            if (settings.SelectedSalts.Count > 0)
            {
                Console.WriteLine($"[PersistenceService]   - Salt Names: {string.Join(", ", settings.SelectedSalts.Select(s => s.Name))}");
            }
            Console.WriteLine($"[PersistenceService]   - Last saved: {settings.LastSaved}");
            
            return settings;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[PersistenceService] ERROR loading settings: {ex.GetType().Name}: {ex.Message}");
            Console.WriteLine($"[PersistenceService] Stack trace: {ex.StackTrace}");
            return null;
        }
    }

    /// <summary>
    /// Clear all saved settings from localStorage
    /// </summary>
    public async Task ClearSettingsAsync()
    {
        try
        {
            await InitializeModuleAsync();
            
            var success = await _jsModule!.InvokeAsync<bool>("removeLocalStorage", SETTINGS_KEY);
            
            if (success)
            {
                Console.WriteLine("[PersistenceService] Settings cleared from localStorage");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[PersistenceService] Error clearing settings: {ex.Message}");
        }
    }
}

/// <summary>
/// Serializable model for a selected salt (used for JSON persistence)
/// </summary>
public class SelectedSaltModel
{
    public string Name { get; set; } = string.Empty;
    public string Formula { get; set; } = string.Empty;
}

/// <summary>
/// Application settings to persist across sessions
/// </summary>
public class AppSettings
{
    /// <summary>
    /// Local water parameters (Nitrate, Calcium, Magnesium, Potassium, Sulfur in ppm)
    /// </summary>
    public WaterParameters WaterParameters { get; set; } = WaterParameters.CreateDefault();

    /// <summary>
    /// Index of selected plant profile (-1 if none selected)
    /// </summary>
    public int SelectedProfileIndex { get; set; } = -1;

    /// <summary>
    /// List of selected salt identifiers
    /// </summary>
    public List<SelectedSaltModel> SelectedSalts { get; set; } = new();

    /// <summary>
    /// Timestamp of last save
    /// </summary>
    public DateTime LastSaved { get; set; } = DateTime.UtcNow;
}
