# Localization Quick Reference

## Using Localization in Your Components

### 1. Inject the Service
```razor
@inject ILocalizationService LocalizationService
```

### 2. Get Translated Strings

#### Simple text
```razor
<h1>@LocalizationService.GetString("home.title")</h1>
```

#### With parameters
```razor
<p>@LocalizationService.GetString("diagnostics.unavoidableMin", 95.5)</p>
```

#### In C# code
```csharp
string message = LocalizationService.GetString("results.success");
```

## Common Translation Keys

### Navigation
```
"nav.home"        = Current page indicator
"nav.substances"  = Alternative salts/fertilizers page
"nav.about"       = About/Information page
```

### Buttons
```
"home.calculateRecipe"     = Main calculate button
"home.clearAll"            = Clear selection
"home.manageSalts"         = Switch to salt selector
"catalog.clearSelection"   = Clear salt selection
```

### Headers/Titles
```
"home.plantProfile"    = Plant selection section
"home.saltCatalog"     = Available salts browser
"home.mySalts"         = Selected salts inventory
"home.calculationResults" = Results section
```

### Ion Names
```
ion.nitrate     ? Nitrate / Nitrat / Nitrate / Nitrato
ion.potassium   ? Potassium / Kalium / Potassium / Potasio
ion.calcium     ? Calcium / Kalzium / Calcium / Calcio
ion.magnesium   ? Magnesium / Magnesium / Magnésium / Magnesio
ion.phosphate   ? Phosphate / Phosphat / Phosphate / Fosfato
ion.sulfate     ? Sulfate / Sulfat / Sulfate / Sulfato
... see LocalizationService.cs for all 16 ion types
```

### Plant Profiles
```
profile.lettuceVegetative   ? Lettuce - Vegetative
profile.tomatoFruiting      ? Tomato - Fruiting
profile.basilVegetative     ? Basil - Vegetative
... see LocalizationService.cs for all plant types
```

### Results
```
"results.success"           = "? SUCCESS"
"results.failed"            = "? OPTIMIZATION FAILED"
"results.recipe"            = "Recipe (g/L):"
"results.ionConcentrations" = "Ion Concentrations:"
"results.notes"             = "?? NOTES"
```

### Approximate Solutions
```
"results.approximateSolution"  = Show when solver relaxed constraints
"results.relaxedNote"          = 20% constraint relaxation notice
"results.relaxedExplanation"   = Explain what was relaxed
```

### Error Messages
```
"error.noDatabase"            = Database not initialized
"error.calculationError"      = Calculation failed
"error.solverNotAvailable"    = SCIP solver missing
"error.selectValidProfile"    = No profile selected
```

### Diagnostics
```
"diagnostics.missingIonSources"    = Can't provide an ion
"diagnostics.ratioConflicts"       = Conflicting ion ratios
"diagnostics.cannotAchieveMin"     = Can't reach minimum
"diagnostics.cannotAchieveMax"     = Can't reach maximum
"diagnostics.concreteRecommendations" = Suggested salts
```

## Language Codes

| Code | Language | Flag |
|------|----------|------|
| `en` | English | ???? |
| `de` | German | ???? |
| `fr` | French | ???? |
| `es` | Spanish | ???? |

## Changing Language Programmatically

```csharp
if (LocalizationService is LocalizationService locService)
{
    locService.SetLanguage("de");  // Switch to German
}
```

## Subscribe to Language Changes

```razor
@implements IAsyncDisposable

@code {
    protected override void OnInitialized()
    {
        if (LocalizationService is LocalizationService locService)
        {
            locService.OnLanguageChanged += StateHasChanged;
        }
    }

    async ValueTask IAsyncDisposable.DisposeAsync()
    {
        if (LocalizationService is LocalizationService locService)
        {
            locService.OnLanguageChanged -= StateHasChanged;
        }
    }
}
```

## Adding New Translations

### Step 1: Update LocalizationService.cs

In the appropriate language method (e.g., `GetEnglishTranslations()`):

```csharp
private Dictionary<string, string> GetEnglishTranslations()
{
    return new Dictionary<string, string>
    {
        // ... existing entries ...
        { "new.key", "English text here" },
    };
}
```

Repeat for German, French, and Spanish methods.

### Step 2: Use in Component

```razor
<label>@LocalizationService.GetString("new.key")</label>
```

## Translation Examples

### Simple Label
**Key:** `home.plantProfile`

| Language | Translation |
|----------|-------------|
| English | ?? PLANT PROFILE |
| German | ?? PFLANZENPROFIL |
| French | ?? PROFIL DE PLANTE |
| Spanish | ?? PERFIL DE PLANTA |

### With Parameters
**Key:** `diagnostics.unavoidableMin`  
**Usage:** `LocalizationService.GetString("diagnostics.unavoidableMin", 95.5)`

| Language | Template | Output |
|----------|----------|--------|
| English | ? Unavoidable minimum: {0:F1} ppm | ? Unavoidable minimum: 95.5 ppm |
| German | ? Unvermeidliches Minimum: {0:F1} ppm | ? Unvermeidliches Minimum: 95.5 ppm |
| French | ? Minimum inévitable: {0:F1} ppm | ? Minimum inévitable: 95.5 ppm |
| Spanish | ? Mínimo inevitable: {0:F1} ppm | ? Mínimo inevitable: 95.5 ppm |

## Component: LanguageSwitcher

Shows flag buttons in the app header.

```razor
<LanguageSwitcher />
```

Displays:
- ???? English (or "English" if not current)
- ???? Deutsch
- ???? Français
- ???? Español

Current language is highlighted and can be clicked to switch.

## Files

| Location | Purpose |
|----------|---------|
| `Services/LocalizationService.cs` | All translation dictionaries |
| `Components/Controls/LanguageSwitcher.razor` | Language selector UI |
| `Components/Layout/MainLayout.razor` | Header with switcher |
| `Program.cs` | Service registration |

## Troubleshooting

### Translation not appearing
- Check key spelling matches exactly
- Verify key exists in all 4 language dictionaries
- Fallback to English will be used if missing

### Language not changing
- Ensure component implements `IAsyncDisposable`
- Check that `StateHasChanged()` is called in event handler
- Verify language code is valid (en, de, fr, es)

### Missing ion name
- Add to `ion.*` keys in all 4 language dictionaries
- Use proper scientific terminology for each language

---

**Updated:** 2025  
**Version:** 1.0  
**Status:** ? Complete

