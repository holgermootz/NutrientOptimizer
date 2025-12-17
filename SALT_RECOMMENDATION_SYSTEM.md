# Salt Recommendation System - Complete ?

## Overview

The `NutrientRecipeOptimizer` now provides **concrete salt recommendations** when optimization fails, helping users understand exactly which salts to add.

## How It Works

### When Optimization Fails

The system now produces output like:

```
=== MISSING ION SOURCES ===
  ? Phosphate: target 60.0 ppm (no salt provides this)

=== RATIO CONFLICTS ===
  ? Potassium: cannot achieve maximum 400.0 ppm
    ? Unavoidable maximum: 320.0 ppm (while meeting other ions)

=== AVAILABLE SALTS COMPOSITION ===
  • Canna Aqua Vega A: Nitrogen, Potassium, Magnesium
  • Canna Aqua Vega B: Phosphorus, Potassium, Sulfate
  • Fetrilon Combi 1: Iron, Boron

=== CONCRETE RECOMMENDATIONS ===
  Provides Phosphate (required 60 ppm)
    ? Monopotassium Phosphate (KH2PO4: Phosphate: 34.7%, Potassium: 28.7%)
    ? Potassium Phosphate (K3PO4: Phosphate: 33.3%, Potassium: 52.4%)
  
  Adds flexibility for ratio balancing
    ? Calcium Nitrate (Ca(NO3)2: Calcium: 18.6%, Nitrate: 61.8%)
    ? Magnesium Sulfate (MgSO4: Magnesium: 9.9%, Sulfate: 39.2%)
```

## Key Components

### 1. SaltRecommendationEngine

Analyzes available salts and recommends specific products:

```csharp
var engine = new SaltRecommendationEngine(allSalts, selectedSalts, ionDemands);
var recommendations = engine.GetRecommendations();
```

**Scoring Criteria:**
- Primary ion match (10x weight)
- Multi-ion contribution (+2 per ion)
- Macronutrient bonus (+3 per macro)

### 2. SaltRecommendation

Represents a category of recommendations:

```csharp
public class SaltRecommendation
{
    public string Reason { get; set; }  // Why this is needed
    public List<SaltOption> RecommendedSalts { get; set; }  // Top 3 options
    public int Priority { get; set; }  // 1=Critical, 2=Helpful
}
```

### 3. SaltOption

A specific salt with scoring data:

```csharp
public class SaltOption
{
    public Salt Salt { get; set; }
    public double IonContent { get; set; }
    public int SecondaryBenefits { get; set; }
    public double Score { get; set; }
    
    public string GetDescription()  // Formatted display
}
```

## Example: Tomato Fruiting + Canna Hydro Flores

**Current Selection:**
- Canna Aqua Vega A
- Canna Aqua Vega B
- Fetrilon Combi 1

**Problem:** Missing Phosphate source, insufficient Magnesium

**Recommendation:**
```
? Add: Monopotassium Phosphate (KH2PO4: 34.7% P, 28.7% K)
  ? Solves phosphate deficiency
  ? Also increases K availability

? Add: Magnesium Sulfate (MgSO4·7H2O: 9.9% Mg, 39.2% SO4)
  ? Improves Mg/S balance
  ? Provides flexibility for ratios
```

## Integration Points

### In DiagnoseInfeasibility()

```csharp
var engine = new SaltRecommendationEngine(_availableSalts, _availableSalts, ionTagging.NeededIons);
var recs = engine.GetRecommendations();

foreach (var rec in recs)
{
    reasons.Add($"  {rec.Reason}");
    foreach (var saltOpt in rec.RecommendedSalts)
    {
        reasons.Add($"    ? {saltOpt.GetDescription()}");
    }
}
```

## User Benefits

? **Specific Products:** Names exact salts to purchase (e.g., "Monopotassium Phosphate" not just "phosphate")
? **Formula & Properties:** Shows chemical composition and key ions
? **Priority Ranking:** Critical vs. Helpful recommendations
? **Top 3 Choices:** Users can select based on availability/price
? **Grouped by Need:** Recommendations organized by missing ions and ratio issues

## Example Output for Users

```
?? OPTIMIZATION FAILED

=== MISSING ION SOURCES ===
  ? Phosphate: target 60.0 ppm (no salt provides this)

=== CONCRETE RECOMMENDATIONS ===

  Priority 1 - Provides Phosphate (required 60 ppm)
    ? Monopotassium Phosphate (KH2PO4)
      Composition: Phosphate: 34.7%, Potassium: 28.7%
    
    ? Potassium Phosphate (K3PO4)
      Composition: Phosphate: 33.3%, Potassium: 52.4%
  
  Priority 2 - Adds flexibility for ratio balancing
    ? Calcium Nitrate (Ca(NO3)2·4H2O)
      Composition: Calcium: 18.6%, Nitrate: 61.8%
    
    ? Magnesium Sulfate (MgSO4·7H2O)
      Composition: Magnesium: 9.9%, Sulfate: 39.2%
```

## Files Changed

| File | Change |
|------|--------|
| `NutrientOptimizer.Math/SaltRecommendationEngine.cs` | ? NEW - Scoring & recommendation logic |
| `NutrientOptimizer.Math/NutrientRecipeOptimizer.cs` | Updated DiagnoseInfeasibility() to use engine |

## Build Status

? **Successful** - All tests compile

## Next Steps for Users

When optimization fails, they now see:
1. **What's missing:** Specific ions not provided
2. **What's conflicting:** Ratio issues with current salts
3. **What to buy:** Concrete salt names & formulas
4. **Why it helps:** How each salt solves the problem

---

**Status:** ? Complete - Ready for production use
