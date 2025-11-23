# Anbefalinger for Teststruktur

Dette dokumentet beskriver hvor tester bør plasseres i prosjektet og hva som bør testes.

## Nåværende Teststruktur

Prosjektet har allerede:
- Et testprosjekt: `Kartverket.Tests/`
- XUnit som testrammeverk
- Moq for mocking
- En eksisterende test: `HomeControllersTest.cs`

## Anbefalt Mappestruktur

Testprosjektet bør organiseres slik at det speiler hovedapplikasjonens struktur:

```
Kartverket.Tests/
├── Controllers/              # Test for alle controllers
│   ├── HomeControllerTest.cs          ✅ (eksisterer)
│   ├── AccountControllerTest.cs       ⚠️ (mangler)
│   ├── AdminControllerTest.cs         ⚠️ (mangler)
│   ├── AdviceControllerTest.cs        ⚠️ (mangler)
│   ├── ObstacleControllerTest.cs      ⚠️ (mangler)
│   ├── PilotControllerTest.cs         ⚠️ (mangler)
│   ├── RegistrarControllerTest.cs     ⚠️ (mangler)
│   ├── MapControllerTest.cs           ⚠️ (mangler)
│   └── VarslingControllerTest.cs      ⚠️ (mangler)
│
├── Repository/              # Test for alle repositories
│   ├── ObstacleRepositoryTest.cs      ⚠️ (mangler)
│   ├── AdviceRepositoryTest.cs        ⚠️ (mangler)
│   ├── RegistrarRepositoryTest.cs     ⚠️ (mangler)
│   ├── UserRepositoryTest.cs           ⚠️ (mangler)
│   └── ArchiveRepositoryTest.cs        ⚠️ (mangler)
│
├── Models/                  # Test for modeller og validering
│   ├── ObstacleDataTest.cs             ⚠️ (kommentert ut)
│   ├── AdviceTest.cs                   ⚠️ (mangler)
│   ├── RapportDataTest.cs              ⚠️ (mangler)
│   └── ViewModel/                      # Test for ViewModels
│       ├── ObstacleDataViewModelTest.cs    ⚠️ (mangler)
│       ├── RegisterViewModelTest.cs        ⚠️ (mangler)
│       └── LoginViewModelTest.cs           ⚠️ (mangler)
│
├── DataContext/             # Test for database-operasjoner
│   ├── ApplicationDBContextTest.cs     ⚠️ (mangler)
│   └── Seeders/
│       └── AuthDbSeederTest.cs         ⚠️ (mangler)
│
└── Helpers/                 # Test-hjelpeklasser og utilities
    ├── TestDataBuilder.cs              ⚠️ (anbefalt)
    └── DatabaseFixture.cs              ⚠️ (anbefalt for integrasjonstester)
```

## Hva Bør Testes?

### 1. Controllers (Høy prioritet)

**Hvorfor:** Controllers inneholder forretningslogikk og håndterer HTTP-forespørsler.

**Hva som bør testes:**
- ✅ HTTP-metoder (GET, POST, PUT, DELETE)
- ✅ Autentisering og autorisasjon
- ✅ Validering av input
- ✅ Redirect-logikk
- ✅ ViewBag/ViewData-verdier
- ✅ Feilhåndtering

**Eksempel på struktur:**
```csharp
// Kartverket.Tests/Controllers/ObstacleControllerTest.cs
public class ObstacleControllerTest
{
    [Fact]
    public void Create_ShouldReturnView_WhenGet()
    {
        // Arrange
        // Act
        // Assert
    }
    
    [Fact]
    public void Create_ShouldReturnViewWithErrors_WhenModelInvalid()
    {
        // Test validering
    }
    
    [Fact]
    public void Create_ShouldRedirect_WhenModelValid()
    {
        // Test suksess-scenario
    }
}
```

### 2. Repositories (Høy prioritet)

**Hvorfor:** Repositories håndterer dataaksess og bør testes isolert fra databasen.

**Hva som bør testes:**
- ✅ CRUD-operasjoner (Create, Read, Update, Delete)
- ✅ Query-logikk (GetAll, GetById, GetByOwner)
- ✅ Feilhåndtering ved database-feil
- ✅ Null-håndtering

**Eksempel:**
```csharp
// Kartverket.Tests/Repository/ObstacleRepositoryTest.cs
public class ObstacleRepositoryTest : IDisposable
{
    private ApplicationDBContext _context;
    
    [Fact]
    public async Task AddObstacle_ShouldAddToDatabase()
    {
        // Bruk InMemory-database for testing
    }
    
    [Fact]
    public async Task GetObstaclesByOwner_ShouldReturnOnlyOwnersObstacles()
    {
        // Test filtrering
    }
}
```

### 3. Models og ViewModels (Medium prioritet)

**Hvorfor:** Validering av data er kritisk for applikasjonens integritet.

**Hva som bør testes:**
- ✅ Data annotations (Required, Range, MaxLength)
- ✅ Validering av gyldige verdier
- ✅ Validering av ugyldige verdier
- ✅ Edge cases (null, tomme strenger, ekstreme verdier)

**Eksempel:**
```csharp
// Kartverket.Tests/Models/ObstacleDataTest.cs
public class ObstacleDataTest
{
    [Fact]
    public void ObstacleData_ShouldBeValid_WhenAllFieldsAreCorrect()
    {
        // Test gyldig modell
    }
    
    [Fact]
    public void ObstacleData_ShouldFail_WhenNameIsEmpty()
    {
        // Test Required-attributt
    }
    
    [Fact]
    public void ObstacleData_ShouldFail_WhenHeightOutOfRange()
    {
        // Test Range-attributt
    }
}
```

### 4. DataContext og Seeders (Lav prioritet)

**Hvorfor:** Database-migrasjoner og seeding bør testes for å sikre korrekt oppsett.

**Hva som bør testes:**
- ✅ Database-migrasjoner
- ✅ Seeding av initial data
- ✅ Roller og brukere opprettes korrekt

### 5. Integrasjonstester (Valgfritt, men anbefalt)

**Hvorfor:** Teste hele flyten fra controller til database.

**Hvor:** Opprett en egen mappe `IntegrationTests/` eller bruk `[Collection]`-attributt i XUnit.

**Eksempel:**
```csharp
// Kartverket.Tests/IntegrationTests/ObstacleFlowTest.cs
[Collection("Database")]
public class ObstacleFlowTest
{
    [Fact]
    public async Task CreateObstacle_EndToEnd_ShouldWork()
    {
        // Test hele flyten: Controller -> Repository -> Database
    }
}
```

## Best Practices

### 1. Testnavngivning
Bruk beskrivende navn som forklarer hva som testes:
```csharp
// ✅ Godt
[Fact]
public void AddObstacle_ShouldReturnObstacleWithId_WhenValidDataProvided()

// ❌ Dårlig
[Fact]
public void Test1()
```

### 2. Arrange-Act-Assert Pattern
Strukturér alle tester med tydelig separasjon:
```csharp
[Fact]
public void ExampleTest()
{
    // Arrange - sett opp testdata
    var obstacle = new ObstacleData { ... };
    
    // Act - utfør operasjonen
    var result = await repository.AddObstacle(obstacle);
    
    // Assert - verifiser resultatet
    Assert.NotNull(result);
    Assert.True(result.ObstacleId > 0);
}
```

### 3. Mocking av Dependencies
Bruk Moq for å isolere enheter:
```csharp
var mockRepository = new Mock<IObstacleRepository>();
var mockLogger = new Mock<ILogger<ObstacleController>>();
var controller = new ObstacleController(mockRepository.Object, mockLogger.Object);
```

### 4. InMemory Database for Repository-tester
Bruk `Microsoft.EntityFrameworkCore.InMemory` for å teste repositories uten ekte database:
```csharp
var options = new DbContextOptionsBuilder<ApplicationDBContext>()
    .UseInMemoryDatabase(databaseName: "TestDb")
    .Options;
```

### 5. Test Data Builders
Opprett hjelpeklasser for å generere testdata:
```csharp
// Kartverket.Tests/Helpers/TestDataBuilder.cs
public static class TestDataBuilder
{
    public static ObstacleData CreateValidObstacle() => new()
    {
        ObstacleName = "Test Hindring",
        ObstacleHeight = 50,
        // ...
    };
}
```

## Prioritering

### Fase 1: Kritisk funksjonalitet (Start her)
1. ✅ `ObstacleControllerTest` - Kjernefunksjonalitet
2. ✅ `ObstacleRepositoryTest` - Dataaksess
3. ✅ `ObstacleDataTest` - Validering

### Fase 2: Autentisering og autorisasjon
1. ✅ `AccountControllerTest` - Login/logout
2. ✅ `UserRepositoryTest` - Brukerhåndtering

### Fase 3: Administrasjon
1. ✅ `AdminControllerTest` - Admin-funksjonalitet
2. ✅ `RegistrarControllerTest` - Registerfører-funksjonalitet

### Fase 4: Resten
1. ✅ Andre controllers
2. ✅ Andre repositories
3. ✅ ViewModels

## Eksisterende Tester

### HomeControllerTest.cs
✅ Eksisterer og tester grunnleggende funksjonalitet. Vurder å utvide med:
- Test av `DataForm` GET/POST
- Test av `Privacy` og `OmOss`
- Test av `Error`-metoden

### ObstacleDataTest.cs
⚠️ Er kommentert ut. Bør aktiveres og utvides med flere testscenarier.

## Neste Steg

1. **Opprett mappestruktur** i `Kartverket.Tests/`
2. **Start med ObstacleControllerTest** (høyest prioritet)
3. **Aktiver ObstacleDataTest** og fikse eventuelle problemer
4. **Legg til Repository-tester** med InMemory-database
5. **Utvid gradvis** med flere tester etter prioritet

## Ressurser

- [XUnit dokumentasjon](https://xunit.net/)
- [Moq dokumentasjon](https://github.com/moq/moq4)
- [EF Core InMemory Provider](https://learn.microsoft.com/en-us/ef/core/testing/testing-with-the-database#in-memory-provider)

