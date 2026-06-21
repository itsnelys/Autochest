// ============================================================
// SE_InventoryCollector
// Version : propre
// Auteur : ChatGPT pour Johann
// ============================================================
// Fonction :
// - Recupere les objets des inventaires connectes
// - Envoie les objets dans les conteneurs adaptes
// - Envoie la pierre en priorite vers les raffineries "raff de pierre"
// - Ignore :
//      * O2/H2 Generator
//      * Irrigation System
//      * Programmable Block
//      * Reacteurs
//      * Armes et tourelles
//      * Input des raffineries, fonderies et assembleuses
// - Supporte plusieurs conteneurs par type de stockage
// - Affiche les logs sur LCD
// ============================================================

const string MAIN_STORAGE_NAME = "Stockage Base";
const string ICE_STORAGE_NAME = "Stockage Glace";
const string RAW_STORAGE_NAME = "Stockage Brute";
const string PROCESSED_STORAGE_NAME = "Stockage Minerai";
const string STONE_REFINERY_NAME = "raff de pierre";
const string LCD_NAME = "LCD Collecteur";
const int LCD_SURFACE_INDEX = 0;
const int MAX_LCD_EVENTS = 12;

List<IMyTerminalBlock> mainStorageBlocks = new List<IMyTerminalBlock>();
List<IMyTerminalBlock> iceStorageBlocks = new List<IMyTerminalBlock>();
List<IMyTerminalBlock> rawStorageBlocks = new List<IMyTerminalBlock>();
List<IMyTerminalBlock> processedStorageBlocks = new List<IMyTerminalBlock>();
List<IMyTerminalBlock> stoneRefineryBlocks = new List<IMyTerminalBlock>();
List<IMyTerminalBlock> sourceBlocks = new List<IMyTerminalBlock>();

IMyTextSurface lcd;

int movedItems = 0;
int failedTransfers = 0;
int ignoredInputs = 0;

public Program()
{
    Runtime.UpdateFrequency = UpdateFrequency.Update100;

    SetupLCD();
}

public void Main(string argument, UpdateType updateSource)
{
    movedItems = 0;
    failedTransfers = 0;
    ignoredInputs = 0;

    mainStorageBlocks.Clear();
    iceStorageBlocks.Clear();
    rawStorageBlocks.Clear();
    processedStorageBlocks.Clear();
    stoneRefineryBlocks.Clear();
    sourceBlocks.Clear();

    SetupLCD();
    GetStorageBlocks();
    GetPriorityRefineries();
    GetSourceBlocks();

    List<string> events = new List<string>();
    string report = "";

    if (TotalStorageCount() == 0)
    {
        report = BuildReport("[ERREUR] Aucun stockage trouve.", events);
        WriteLCD(report);
        Echo(report);
        return;
    }

    foreach (var source in sourceBlocks)
    {
        try
        {
            TransferItems(source, events);
        }
        catch (Exception e)
        {
            AddEvent(events, "! " + ShortName(source.CustomName) + " : " + e.Message);
        }
    }

    string status = "Pret";
    if (failedTransfers > 0)
        status = "Attention : stockage plein ou transfert bloque";
    else if (movedItems > 0)
        status = "Transfert termine";

    report = BuildReport(status, events);
    WriteLCD(report);
    Echo(report);
}

void SetupLCD()
{
    lcd = GetLCDSurface();

    if (lcd == null)
        return;

    lcd.ContentType = VRage.Game.GUI.TextPanel.ContentType.TEXT_AND_IMAGE;
    lcd.Font = "Monospace";
    lcd.FontSize = 0.45f;
    lcd.Alignment = VRage.Game.GUI.TextPanel.TextAlignment.LEFT;
}

IMyTextSurface GetLCDSurface()
{
    List<IMyTerminalBlock> blocks = new List<IMyTerminalBlock>();

    GridTerminalSystem.GetBlocks(blocks);

    IMyTextSurface exactSurface = FindLCDSurface(blocks, true);
    if (exactSurface != null)
        return exactSurface;

    return FindLCDSurface(blocks, false);
}

IMyTextSurface FindLCDSurface(List<IMyTerminalBlock> blocks, bool exactName)
{
    foreach (var block in blocks)
    {
        if (block == null)
            continue;

        if (exactName && block.CustomName != LCD_NAME)
            continue;

        if (!exactName && !block.CustomName.Contains(LCD_NAME))
            continue;

        IMyTextSurface surface = block as IMyTextSurface;
        if (surface != null)
            return surface;

        IMyTextSurfaceProvider provider = block as IMyTextSurfaceProvider;
        if (provider != null && provider.SurfaceCount > LCD_SURFACE_INDEX)
            return provider.GetSurface(LCD_SURFACE_INDEX);
    }

    return null;
}

void GetStorageBlocks()
{
    GridTerminalSystem.GetBlocksOfType<IMyCargoContainer>(
        mainStorageBlocks,
        block => block.CustomName.Contains(MAIN_STORAGE_NAME)
    );

    GridTerminalSystem.GetBlocksOfType<IMyCargoContainer>(
        iceStorageBlocks,
        block => block.CustomName.Contains(ICE_STORAGE_NAME)
    );

    GridTerminalSystem.GetBlocksOfType<IMyCargoContainer>(
        rawStorageBlocks,
        block => block.CustomName.Contains(RAW_STORAGE_NAME)
    );

    GridTerminalSystem.GetBlocksOfType<IMyCargoContainer>(
        processedStorageBlocks,
        block => block.CustomName.Contains(PROCESSED_STORAGE_NAME)
    );
}

void GetPriorityRefineries()
{
    GridTerminalSystem.GetBlocksOfType<IMyRefinery>(
        stoneRefineryBlocks,
        block => block.CustomName.ToLower().Contains(STONE_REFINERY_NAME)
    );
}

void GetSourceBlocks()
{
    List<IMyTerminalBlock> allBlocks = new List<IMyTerminalBlock>();

    GridTerminalSystem.GetBlocks(allBlocks);

    foreach (var block in allBlocks)
    {
        if (ShouldIgnoreBlock(block))
            continue;

        if (block.HasInventory)
            sourceBlocks.Add(block);
    }
}

bool ShouldIgnoreBlock(IMyTerminalBlock block)
{
    if (block == null)
        return true;

    if (IsStorageBlock(block))
        return true;

    if (block is IMyProgrammableBlock)
        return true;

    if (block is IMyGasGenerator)
        return true;

    if (block is IMyReactor)
        return true;

    if (IsWeaponBlock(block))
        return true;

    if (block.BlockDefinition.SubtypeName.Contains("Irrigation"))
        return true;

    return false;
}

bool IsStorageBlock(IMyTerminalBlock block)
{
    if (block.CustomName.Contains(MAIN_STORAGE_NAME))
        return true;

    if (block.CustomName.Contains(ICE_STORAGE_NAME))
        return true;

    if (block.CustomName.Contains(RAW_STORAGE_NAME))
        return true;

    if (block.CustomName.Contains(PROCESSED_STORAGE_NAME))
        return true;

    return false;
}

bool IsWeaponBlock(IMyTerminalBlock block)
{
    if (block is IMyUserControllableGun)
        return true;

    string typeName = block.BlockDefinition.TypeIdString;
    string subtypeName = block.BlockDefinition.SubtypeName;

    if (typeName.Contains("Turret") || subtypeName.Contains("Turret"))
        return true;

    if (typeName.Contains("Gatling") || subtypeName.Contains("Gatling"))
        return true;

    if (typeName.Contains("Missile") || subtypeName.Contains("Missile"))
        return true;

    if (typeName.Contains("Weapon") || subtypeName.Contains("Weapon"))
        return true;

    return false;
}

void TransferItems(IMyTerminalBlock source, List<string> events)
{
    for (int invIndex = 0; invIndex < source.InventoryCount; invIndex++)
    {
        if (ShouldIgnoreInventory(source, invIndex))
        {
            ignoredInputs++;
            continue;
        }

        IMyInventory sourceInv = source.GetInventory(invIndex);

        List<MyInventoryItem> items = new List<MyInventoryItem>();
        sourceInv.GetItems(items);

        for (int i = items.Count - 1; i >= 0; i--)
        {
            var item = items[i];

            bool transferred = false;
            List<IMyTerminalBlock> targetStorages = GetTargetStorages(item);

            foreach (var storage in targetStorages)
            {
                IMyInventory destInv = storage.GetInventory();

                if (sourceInv.TransferItemTo(destInv, i, null, true))
                {
                    movedItems++;

                    AddEvent(events, "+ " + item.Type.SubtypeId + " -> " + GetRouteName(item));

                    transferred = true;
                    break;
                }
            }

            if (!transferred)
            {
                failedTransfers++;

                AddEvent(events, "! " + item.Type.SubtypeId + " bloque -> " + GetRouteName(item));
            }
        }
    }
}

List<IMyTerminalBlock> GetTargetStorages(MyInventoryItem item)
{
    if (IsIce(item))
        return iceStorageBlocks;

    if (IsStone(item))
        return CombineTargets(stoneRefineryBlocks, rawStorageBlocks);

    if (IsRawResource(item))
        return rawStorageBlocks;

    if (IsProcessedResource(item))
        return processedStorageBlocks;

    return mainStorageBlocks;
}

string GetRouteName(MyInventoryItem item)
{
    if (IsIce(item))
        return ICE_STORAGE_NAME;

    if (IsStone(item))
        return STONE_REFINERY_NAME + " / " + RAW_STORAGE_NAME;

    if (IsRawResource(item))
        return RAW_STORAGE_NAME;

    if (IsProcessedResource(item))
        return PROCESSED_STORAGE_NAME;

    return MAIN_STORAGE_NAME;
}

bool IsIce(MyInventoryItem item)
{
    return item.Type.SubtypeId.ToString() == "Ice";
}

bool IsRawResource(MyInventoryItem item)
{
    string typeId = item.Type.TypeId.ToString();

    if (IsIce(item))
        return false;

    return typeId.Contains("Ore");
}

bool IsProcessedResource(MyInventoryItem item)
{
    string typeId = item.Type.TypeId.ToString();

    return typeId.Contains("Ingot");
}

bool IsStone(MyInventoryItem item)
{
    return item.Type.SubtypeId.ToString() == "Stone";
}

List<IMyTerminalBlock> CombineTargets(List<IMyTerminalBlock> firstTargets, List<IMyTerminalBlock> fallbackTargets)
{
    List<IMyTerminalBlock> combinedTargets = new List<IMyTerminalBlock>();

    foreach (var target in firstTargets)
        combinedTargets.Add(target);

    foreach (var target in fallbackTargets)
        combinedTargets.Add(target);

    return combinedTargets;
}

string BuildReport(string status, List<string> events)
{
    string report = "";
    report += "AUTO-CHEST / COLLECTEUR\n";
    report += "=======================\n";
    report += "Etat      : " + status + "\n\n";

    report += "TRANSFERTS\n";
    report += "  Objets deplaces : " + movedItems + "\n";
    report += "  Echecs          : " + failedTransfers + "\n";
    report += "  Inputs ignores  : " + ignoredInputs + "\n\n";

    report += "RESEAU\n";
    report += "  Sources scannees : " + sourceBlocks.Count + "\n";
    report += "  Stockage base    : " + mainStorageBlocks.Count + "\n";
    report += "  Stockage glace   : " + iceStorageBlocks.Count + "\n";
    report += "  Stockage brute   : " + rawStorageBlocks.Count + "\n";
    report += "  Stockage minerai : " + processedStorageBlocks.Count + "\n";
    report += "  Raff pierre      : " + stoneRefineryBlocks.Count + "\n\n";

    report += "ROUTAGE\n";
    report += "  Glace       -> " + ICE_STORAGE_NAME + "\n";
    report += "  Pierre      -> " + STONE_REFINERY_NAME + " puis " + RAW_STORAGE_NAME + "\n";
    report += "  Minerais    -> " + RAW_STORAGE_NAME + "\n";
    report += "  Traites     -> " + PROCESSED_STORAGE_NAME + "\n";
    report += "  Autres      -> " + MAIN_STORAGE_NAME + "\n\n";

    report += "PROTEGE\n";
    report += "  Raffineries / fonderies : Input\n";
    report += "  Assembleuses            : Input\n";
    report += "  Reacteurs               : Inventaire\n";
    report += "  Armes / tourelles       : Inventaire\n\n";

    report += "DERNIERES ACTIONS\n";
    if (events.Count == 0)
    {
        report += "  Rien a transferer.\n";
    }
    else
    {
        foreach (string line in events)
        {
            report += "  " + line + "\n";
        }
    }

    return report;
}

int TotalStorageCount()
{
    return mainStorageBlocks.Count + iceStorageBlocks.Count + rawStorageBlocks.Count + processedStorageBlocks.Count;
}

void AddEvent(List<string> events, string text)
{
    events.Add(text);

    if (events.Count > MAX_LCD_EVENTS)
        events.RemoveAt(0);
}

string ShortName(string name)
{
    if (name.Length <= 24)
        return name;

    return name.Substring(0, 21) + "...";
}

bool ShouldIgnoreInventory(IMyTerminalBlock block, int inventoryIndex)
{
    // Ces blocs ont leur inventaire d'entree en index 0.
    // On le laisse intact pour eviter de retirer les ressources en cours d'utilisation.
    if (inventoryIndex == 0 && (block is IMyRefinery || block is IMyAssembler))
        return true;

    return false;
}

void WriteLCD(string text)
{
    if (lcd != null)
    {
        lcd.WriteText(text);
    }
}
