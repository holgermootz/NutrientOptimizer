#  Nutrient Optimizer

> **Precision Hydroponic Nutrition for Sustainable Plant Growth**

A sophisticated nutrient recipe optimization system built with .NET 10 and Blazor. This application uses advanced linear programming to calculate precise salt combinations that meet exact plant nutrient requirements—because plant lives matter, and they deserve the best.

##  Our Mission

Hydroponic farming represents one of humanity's most promising paths toward sustainable agriculture. By growing plants without soil, we can:

- **Conserve water** - Use up to 95% less water than traditional farming
- **Reduce chemical runoff** - Eliminate pesticide contamination of soil and groundwater
- **Maximize yields** - Grow more food in less space with fewer resources
- **Reclaim marginal lands** - Farm in deserts, cities, and degraded areas

But hydroponic systems demand precision. Plants are living organisms with exacting nutritional needs. A single imbalance in ion concentration can stunt growth, reduce yields, or compromise plant health. This is where Nutrient Optimizer comes in.

##  Why This Matters

Traditional nutrient recipe development relies on:
- **Guesswork and approximation** - Leading to suboptimal plant performance
- **Wasted resources** - Excess nutrients that don't benefit plants
- **Trial and error** - Expensive experiments with living systems
- **Inconsistent results** - Difficulty scaling proven recipes

**Nutrient Optimizer eliminates guesswork.** Using rigorous mathematical optimization, it calculates the exact combination of salts needed to deliver every nutrient within its target range. The result: healthier plants, better yields, and responsible resource stewardship.

## ? Features

###  Advanced Optimization Engine
- **Linear programming solver** (Google OR-Tools) for mathematically optimal solutions
- **Multi-ion constraints** - Simultaneously balances macronutrients (N, P, K, Ca, Mg, S) and micronutrients (Fe, Mn, Zn, Cu, B, Mo)
- **Infeasibility detection** - Automatically identifies nutrient ratios that cannot be achieved with available salts
- **Precision calculations** - Minimizes deviation from target concentrations to <0.1 ppm

###  Comprehensive Plant Profiles
- Pre-configured profiles for common crops (Lettuce, Tomato, Basil)
- Extensible library for custom plant species and growth stages
- Target-based formulation (set minimum, maximum, and ideal concentrations)
- EC (Electrical Conductivity) bounds for system monitoring

###  Extensive Salt Library
- Pre-loaded common salts with precise molecular weights and ion compositions
- Easy addition of regional or specialty salt sources
- Support for fertilizers with complex ion profiles

###  Clean, Intuitive Interface
- ISO/DIN design philosophy for clarity and professionalism
- Sage/offwhite/black minimalist color scheme
- Responsive Material Design (MudBlazor)
- Accessible form controls for all users

###  Transparent Results
- Detailed recipe output (salt amounts in g/L)
- Resulting ion concentrations in ppm
- Deviation metrics showing optimization quality
- Warnings for unsupplyable nutrients

###  Production-Ready Deployment
- Docker support for Windows x64 and ARM64 (Raspberry Pi)
- SQLite for local data persistence
- Entity Framework Core for extensible data management

## ? Technology Stack

| Layer | Technology |
|-------|-----------|
| **Frontend** | Blazor Server, MudBlazor |
| **Backend** | ASP.NET Core, Entity Framework Core |
| **Optimization** | Google OR-Tools (SCIP solver) |
| **Runtime** | .NET 10 |
| **Database** | SQLite |
| **Deployment** | Docker (multi-arch) |

##  Quick Start

### Prerequisites
- .NET 10 SDK ([Download](https://dotnet.microsoft.com/download/dotnet/10.0))
- Git

### Installation

```bash
# Clone the repository
git clone https://github.com/holgermootz/NutrientOptimizer.git
cd NutrientOptimizer

# Build the solution
dotnet build

# Run the web application
cd NutrientOptimizer.Web
dotnet run
```

Navigate to `https://localhost:5001` in your browser.

### Run Tests

```bash
dotnet test NutrientOptimizer.Tests
```

##  Usage

1. **Select a Plant Profile**
   - Choose from Lettuce, Tomato, or test profiles
   - Each profile defines precise ion concentration targets

2. **Select Available Salts**
   - Check the salts you have on hand
   - The system supports all common hydroponic salts

3. **Calculate Optimal Recipe**
   - Click "CALCULATE RECIPE"
   - The optimizer computes the exact combination

4. **Review Results**
   - See salt amounts in g/L
   - Verify resulting ion concentrations
   - Check total deviation from targets

### Example Output

```
=== OPTIMAL RECIPE FOUND ===

Recipe (g/L):
  Calcium Nitrate Tetrahydrate: 0.86 g/L
  Potassium Nitrate: 0.33 g/L
  Magnesium Sulfate Heptahydrate: 0.30 g/L
  Monopotassium Phosphate: 0.26 g/L

=== RESULTING ION CONCENTRATIONS (ppm) ===
  Calcium: 150.00 ppm ?
  Potassium: 200.00 ppm ?
  Magnesium: 30.00 ppm ?
  Nitrate: 450.00 ppm ?
  Phosphate: 58.53 ppm ?
  Sulfate: 118.57 ppm ?

Total deviation from targets: 0.0%
```

##  Project Structure

```
NutrientOptimizer/
? NutrientOptimizer.Web/
?   ? Components/Pages/Home.razor       # Main optimization UI
?   ? Layout/MainLayout.razor           # App shell
?   ? Program.cs                        # Dependency injection
?   ? appsettings.json                  # Configuration
?
? NutrientOptimizer.Core/
?   ? Models/
?   ?   ? Salt.cs                       # Salt definition
?   ?   ? Ion.cs                        # Ion enumeration
?   ?   ? PlantProfile.cs               # Plant nutrient targets
?   ?   ? SaltLibrary.cs                # Common salts database
?   ? Data/
?   ?   ? NutrientDbContext.cs          # Entity Framework context
?   ?   ? SaltRepository.cs             # Data access layer
?   ? NutrientCalculator.cs             # Ion concentration math
?   ? SolutionValidator.cs              # Target validation
?
? NutrientOptimizer.Math/
?   ? NutrientRecipeOptimizer.cs        # LP solver wrapper
?   ? OptimizationResult.cs             # Result data model
?
? NutrientOptimizer.Tests/
?   ? NutrientCalculatorTests.cs        # Unit & integration tests
?
? Dockerfile                            # Multi-arch container config
```

##  Configuration

### Database

Edit `appsettings.json` to change the database location:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=NutrientOptimizer.db"
  }
}
```

### Custom Plant Profiles

Add profiles to `NutrientOptimizer.Core/Models/PlantProfileLibrary.cs`:

```csharp
new PlantProfile
{
    Name = "Basil - Vegetative",
    Description = "Culinary basil growth stage",
    IonTargets = new List<IonTarget>
    {
        new(Ion.Nitrate, 150, 250, 200),
        new(Ion.Potassium, 150, 300, 200),
        new(Ion.Calcium, 100, 200, 150),
        new(Ion.Magnesium, 30, 80, 50),
        // ... add more ions as needed
    },
    MinEC = 1.2,
    MaxEC = 2.0
}
```

### Custom Salts

Add salts to `NutrientOptimizer.Core/Models/SaltLibrary.cs`:

```csharp
new Salt
{
    Name = "Your Salt Name",
    Formula = "Chemical formula",
    MolecularWeight = 123.45,
    IonContributions = new Dictionary<Ion, double>
    {
        [Ion.Calcium] = 40.078,
        [Ion.Nitrate] = 62.005
    }
}
```

##  Docker Deployment

### Build

```bash
docker build -f NutrientOptimizer.Web/Dockerfile -t nutrient-optimizer .
```

### Run

```bash
docker run -p 8080:80 nutrient-optimizer
```

Access at `http://localhost:8080`

**Multi-architecture:** Supports Windows x64 and Raspberry Pi ARM64

##  How It Works

### The Optimization Problem

Nutrient Optimizer solves a **Linear Programming problem**:

**Minimize:** Total absolute deviation from target ion concentrations

**Subject to:**
- Each ion concentration must be between min and max bounds
- Salt amounts must be non-negative
- All ions must be satisfied simultaneously

**Variables:** Amount (g/L) of each selected salt

### The Algorithm

1. **Parse input** - User selects profile and salts
2. **Build LP model** - Translate constraints into mathematical form
3. **Solve** - SCIP solver finds optimal solution
4. **Calculate results** - Determine actual ion concentrations
5. **Display** - Show recipe and validation metrics

##  Sustainable Agriculture & Resource Stewardship

This project embodies principles of environmental responsibility:

- **Water Conservation** - Hydroponics uses <5% of water vs. traditional farming
- **Precision Nutrition** - Every nutrient is calculated; nothing is wasted
- **Soil Protection** - No agricultural runoff or contamination
- **Year-round Production** - Grow food anywhere, anytime
- **Reduced Chemical Use** - Controlled environment = fewer pest interventions

By optimizing nutrient solutions to mathematical precision, we ensure every drop of water and every atom of fertilizer contributes meaningfully to plant growth. **Plant lives matter.** They deserve our best efforts.

##  Contributing

We welcome contributions from farmers, researchers, and developers! 

To contribute:

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit changes (`git commit -m 'Add AmazingFeature'`)
4. Push to branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

##  License

MIT License - See LICENSE file for details

##  Acknowledgments

- [.NET 10](https://dotnet.microsoft.com/) - Modern cross-platform development
- [MudBlazor](https://mudblazor.com/) - Beautiful Material Design UI
- [Google OR-Tools](https://developers.google.com/optimization) - World-class optimization solver
- The hydroponic farming community - For inspiring sustainable agriculture

---

**Version:** 1.0.0  
**Status:** ? Production Ready  
**Last Updated:** December 2024  

**Made with  for plants and people.**
