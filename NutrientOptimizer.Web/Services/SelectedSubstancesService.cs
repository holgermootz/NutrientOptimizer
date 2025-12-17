using NutrientOptimizer.Core;
using System.Collections.Generic;
using System.Linq;

namespace NutrientOptimizer.Web.Services;

/// <summary>
/// Service to manage selected substances across pages with persistence
/// </summary>
public class SelectedSubstancesService
{
    private List<Salt> _selectedSalts = new();

    /// <summary>
    /// Event fired when selection changes
    /// </summary>
    public event Action? OnSelectionChanged;

    /// <summary>
    /// Get all currently selected salts
    /// </summary>
    public List<Salt> GetSelectedSalts() => new(_selectedSalts);

    /// <summary>
    /// Check if a specific salt is selected
    /// </summary>
    public bool IsSelected(Salt salt)
    {
        return _selectedSalts.Any(s => s.Name == salt.Name && s.Formula == salt.Formula);
    }

    /// <summary>
    /// Toggle selection of a salt
    /// </summary>
    public void ToggleSalt(Salt salt)
    {
        var existing = _selectedSalts.FirstOrDefault(s => s.Name == salt.Name && s.Formula == salt.Formula);
        
        if (existing != null)
        {
            _selectedSalts.Remove(existing);
        }
        else
        {
            _selectedSalts.Add(salt);
        }
        
        NotifyChanged();
    }

    /// <summary>
    /// Add a salt to selection
    /// </summary>
    public void AddSalt(Salt salt)
    {
        if (!IsSelected(salt))
        {
            _selectedSalts.Add(salt);
            NotifyChanged();
        }
    }

    /// <summary>
    /// Remove a salt from selection
    /// </summary>
    public void RemoveSalt(Salt salt)
    {
        var existing = _selectedSalts.FirstOrDefault(s => s.Name == salt.Name && s.Formula == salt.Formula);
        if (existing != null)
        {
            _selectedSalts.Remove(existing);
            NotifyChanged();
        }
    }

    /// <summary>
    /// Clear all selections
    /// </summary>
    public void ClearSelection()
    {
        _selectedSalts.Clear();
        NotifyChanged();
    }

    /// <summary>
    /// Get count of selected salts
    /// </summary>
    public int GetSelectedCount() => _selectedSalts.Count;

    /// <summary>
    /// Set selections from a list (used when loading from storage)
    /// </summary>
    public void SetSelections(List<(string name, string formula)> saltIds, List<Salt> allSalts)
    {
        _selectedSalts.Clear();
        
        foreach (var (name, formula) in saltIds)
        {
            var salt = allSalts.FirstOrDefault(s => s.Name == name && s.Formula == formula);
            if (salt != null)
            {
                _selectedSalts.Add(salt);
            }
        }
    }

    /// <summary>
    /// Get selections as serializable format for localStorage
    /// </summary>
    public List<(string name, string formula)> GetSelectionsForStorage()
    {
        return _selectedSalts.Select(s => (s.Name, s.Formula)).ToList();
    }

    /// <summary>
    /// Notify subscribers of changes
    /// </summary>
    private void NotifyChanged()
    {
        OnSelectionChanged?.Invoke();
    }
}
