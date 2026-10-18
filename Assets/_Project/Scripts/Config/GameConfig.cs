using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

namespace PlanErfuellt
{
    /// <summary>
    /// Alle Spielwerte. Wird vom BalancingImporter aus Data/Json/Balancing.json erzeugt –
    /// Werte nicht im Inspector ändern, sondern in der JSON-Datei.
    /// </summary>
    [CreateAssetMenu(menuName = "Plan erfüllt/Game Config", fileName = "GameConfig")]
    public class GameConfig : ScriptableObject
    {
        public DayConfig day = new DayConfig();
        public StartConfig start = new StartConfig();
        public PricesConfig prices = new PricesConfig();
        public SubstanceConfig[] substances = new SubstanceConfig[0];
        public MixingConfig mixing = new MixingConfig();
        public AnimalsConfig animals = new AnimalsConfig();
        public FormulaConfig formulas = new FormulaConfig();
        public ComboConfig combo = new ComboConfig();
        public RisksConfig risks = new RisksConfig();
        public WarningsConfig warnings = new WarningsConfig();
        public ProcessingConfig processing = new ProcessingConfig();
        public AngerConfig anger = new AngerConfig();
        public AuthorityConfig authority = new AuthorityConfig();
        public InspectionConfig inspection = new InspectionConfig();
        public InspectorConfig inspector = new InspectorConfig();
        public MachinesFixedConfig machinesFixed = new MachinesFixedConfig();
        public BuildMachinesConfig buildMachines = new BuildMachinesConfig();
        public WishesConfig wishes = new WishesConfig();
        public ContractsConfig contracts = new ContractsConfig();
        public InfectionConfig infection = new InfectionConfig();
        public WorldConfig world = new WorldConfig();
        public NewsConfig news = new NewsConfig();
        public EndingsConfig endings = new EndingsConfig();
        public LayoutConfig layout = new LayoutConfig();
        public PlayerConfig player = new PlayerConfig();
        public InteractionConfig interaction = new InteractionConfig();
        public UiConfig ui = new UiConfig();
        public PultConfig pult = new PultConfig();
        public AnimalConfig animal = new AnimalConfig();
        public AnimalVisualsConfig animalVisuals = new AnimalVisualsConfig();
        public ChamberDoorsConfig chamberDoors = new ChamberDoorsConfig();
        public ChamberFireConfig chamberFire = new ChamberFireConfig();
        public FxConfig fx = new FxConfig();
        public ClothFxConfig clothFx = new ClothFxConfig();
        public MachineFxConfig machineFx = new MachineFxConfig();
        public CinematicConfig cinematic = new CinematicConfig();
        public BuildViewConfig buildView = new BuildViewConfig();
        public RecordsConfig records = new RecordsConfig();
        public NewsStudioConfig newsStudio = new NewsStudioConfig();

        // Diese Teile sind in der JSON als Objekte mit Schlüsseln abgelegt und werden vom Importer gesondert übertragen.
        [JsonIgnore] public List<UltimateSerum> ultimates = new List<UltimateSerum>();
        [JsonIgnore] public List<FunRecipe> funRecipes = new List<FunRecipe>();
        [JsonIgnore] public List<BuildMachineType> buildMachineTypes = new List<BuildMachineType>();
        [JsonIgnore] public List<ComboNameEntry> comboNames = new List<ComboNameEntry>();

        Dictionary<string, string> comboNameLookup;

        /// <summary>Grundwerte einer Tierart.</summary>
        public AnimalBaseConfig Animal(AnimalKind kind) => kind == AnimalKind.Hen ? animals.hen : animals.cow;

        /// <summary>Fester Kombinationsname oder null.</summary>
        public string FixedComboName(string key)
        {
            if (comboNameLookup == null)
            {
                comboNameLookup = new Dictionary<string, string>();
                foreach (var e in comboNames) comboNameLookup[e.key] = e.displayName;
            }
            return comboNameLookup.TryGetValue(key, out var n) ? n : null;
        }

        /// <summary>Ultimatives Serum, dessen Rezept genau dieser Dosis entspricht, sonst null.</summary>
        public UltimateSerum UltimateForRecipe(Dose d)
        {
            foreach (var u in ultimates) if (u.Recipe == d) return u;
            return null;
        }

        /// <summary>Ultimatives Serum nach Id (omega/koloss), sonst null.</summary>
        public UltimateSerum UltimateById(string id)
        {
            foreach (var u in ultimates) if (u.id == id) return u;
            return null;
        }

        /// <summary>Kuriosität, die bei genau dieser Gesamtdosis und Tierart greift, sonst null.</summary>
        public FunRecipe FunRecipeFor(AnimalKind kind, Dose total)
        {
            foreach (var f in funRecipes) if (f.animal == kind && f.Total == total) return f;
            return null;
        }

        void OnValidate() => comboNameLookup = null;
    }
}
