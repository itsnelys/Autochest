# Space Engineers Autochest System

## FR

Script de Programmable Block pour *Space Engineers* qui collecte automatiquement les objets des inventaires connectes et les range dans des conteneurs specialises.

Le script protege les blocs de production et les blocs utilitaires qui ne doivent pas etre vides, puis affiche un etat compact sur LCD.

### Installation

1. Place un `Programmable Block`.
2. Copie `Script_Autochest.cs` dans le Programmable Block.
3. Cree les conteneurs de stockage avec les noms requis.
4. Ajoute un LCD optionnel nomme `LCD Collecteur`.
5. Compile et laisse le script tourner en automatique.

### Blocs et noms requis

| Bloc | Nom / Regle | Description |
| --- | --- | --- |
| Conteneur cargo | Contient `Stockage Base` | Stockage principal pour les objets standards. |
| Conteneur cargo | Contient `Stockage Glace` | Stockage dedie a la glace. |
| Conteneur cargo | Contient `Stockage Brute` | Stockage dedie aux minerais et ressources brutes. |
| Conteneur cargo | Contient `Stockage Minerai` | Stockage dedie aux ressources traitees / lingots. |
| Raffinerie | Contient `raff de pierre` | Raffinerie prioritaire pour la pierre brute. |
| LCD ou ecran | Contient `LCD Collecteur` | Affichage optionnel de l'etat du collecteur. |
| Programmable Block | N'importe quel nom | Execute le script. |

Plusieurs conteneurs par type sont supportes. Exemples : `Stockage Base 1`, `Stockage Glace Nord`, `Stockage Brute Minerais`, `Stockage Minerai Lingots`.

### Reglages

| Reglage | Valeur par defaut | Description |
| --- | --- | --- |
| `MAIN_STORAGE_NAME` | `Stockage Base` | Destination des objets standards. |
| `ICE_STORAGE_NAME` | `Stockage Glace` | Destination de la glace. |
| `RAW_STORAGE_NAME` | `Stockage Brute` | Destination des ressources brutes. |
| `PROCESSED_STORAGE_NAME` | `Stockage Minerai` | Destination des ressources traitees / lingots. |
| `STONE_REFINERY_NAME` | `raff de pierre` | Nom cherche pour les raffineries qui recoivent la pierre en priorite. |
| `LCD_NAME` | `LCD Collecteur` | Nom cherche pour l'ecran de statut. |
| `LCD_SURFACE_INDEX` | `0` | Surface utilisee sur les blocs multi-ecrans. |
| `MAX_LCD_EVENTS` | `12` | Nombre maximum d'evenements affiches. |

### Logique de tri

| Type d'objet | Destination |
| --- | --- |
| Glace | `Stockage Glace` |
| Pierre brute | Raffineries contenant `raff de pierre`, puis `Stockage Brute` si aucune ne peut recevoir |
| Minerais / ressources brutes | `Stockage Brute` |
| Ressources traitees / lingots | `Stockage Minerai` |
| Composants, outils, bouteilles, munitions et autres objets | `Stockage Base` |

La glace, les ressources brutes et les ressources traitees ne retombent pas dans `Stockage Base` si leur stockage dedie est plein.

### Inventaires proteges

Le script ignore :

- les conteneurs de destination ;
- les Programmable Blocks ;
- les generateurs O2/H2 ;
- les reacteurs ;
- les systemes d'irrigation ;
- les armes et tourelles ;
- les entrees des raffineries, fonderies et assembleuses.

Les sorties des raffineries, fonderies et assembleuses peuvent etre collectees normalement.
L'uranium traite reste dans les reacteurs, et les munitions restent dans les armes et tourelles.

### Depannage

| Probleme | Solution |
| --- | --- |
| Aucun stockage trouve | Renomme au moins un conteneur avec `Stockage Base`, `Stockage Glace` ou `Stockage Brute`. |
| La glace ne bouge pas | Ajoute ou libere un conteneur `Stockage Glace`. |
| Les minerais ne bougent pas | Ajoute ou libere un conteneur `Stockage Brute`. |
| Les lingots ne bougent pas | Ajoute ou libere un conteneur `Stockage Minerai`. |
| La pierre ne va pas en raffinerie | Renomme au moins une raffinerie avec `raff de pierre` et verifie son inventaire d'entree. |
| Le LCD ne s'affiche pas | Renomme l'ecran avec `LCD Collecteur`. |
| Le mauvais ecran est utilise | Change `LCD_SURFACE_INDEX`. |

### Note de patch 0.1

- Premiere version stable documentee.
- Tri automatique entre stockage principal, glace et ressources brutes.
- Protection des inventaires critiques de production et de combat.
- Affichage LCD des transferts, blocages et derniers evenements.

---

## EN

*Space Engineers* Programmable Block script that automatically collects items from connected inventories and routes them into dedicated storage containers.

The script protects production and utility blocks that should keep their contents, then displays a compact status report on an LCD.

### Installation

1. Place a `Programmable Block`.
2. Copy `Script_Autochest.cs` into the Programmable Block.
3. Create the required named storage containers.
4. Optionally add an LCD named `LCD Collecteur`.
5. Compile and let the script run automatically.

### Required Blocks and Names

| Block | Name / Rule | Description |
| --- | --- | --- |
| Cargo container | Contains `Stockage Base` | Main storage for regular items. |
| Cargo container | Contains `Stockage Glace` | Dedicated ice storage. |
| Cargo container | Contains `Stockage Brute` | Dedicated raw resource and ore storage. |
| Cargo container | Contains `Stockage Minerai` | Dedicated processed resource / ingot storage. |
| Refinery | Contains `raff de pierre` | Priority refinery for raw stone. |
| LCD or screen | Contains `LCD Collecteur` | Optional collector status display. |
| Programmable Block | Any name | Runs the script. |

Multiple containers per storage type are supported. Examples: `Stockage Base 1`, `Stockage Glace Nord`, `Stockage Brute Minerais`, `Stockage Minerai Lingots`.

### Settings

| Setting | Default Value | Description |
| --- | --- | --- |
| `MAIN_STORAGE_NAME` | `Stockage Base` | Destination for regular items. |
| `ICE_STORAGE_NAME` | `Stockage Glace` | Destination for ice. |
| `RAW_STORAGE_NAME` | `Stockage Brute` | Destination for raw resources. |
| `PROCESSED_STORAGE_NAME` | `Stockage Minerai` | Destination for processed resources / ingots. |
| `STONE_REFINERY_NAME` | `raff de pierre` | Name searched for refineries that receive stone first. |
| `LCD_NAME` | `LCD Collecteur` | Name searched for the status display. |
| `LCD_SURFACE_INDEX` | `0` | Surface used on multi-screen blocks. |
| `MAX_LCD_EVENTS` | `12` | Maximum number of displayed events. |

### Routing Logic

| Item Type | Destination |
| --- | --- |
| Ice | `Stockage Glace` |
| Raw stone | Refineries containing `raff de pierre`, then `Stockage Brute` if none can receive it |
| Ores / raw refinery resources | `Stockage Brute` |
| Processed resources / ingots | `Stockage Minerai` |
| Components, tools, bottles, ammunition, and other items | `Stockage Base` |

Ice, raw resources, and processed resources do not fall back into `Stockage Base` if their dedicated storage is full.

### Protected Inventories

The script ignores:

- destination storage containers;
- Programmable Blocks;
- O2/H2 generators;
- reactors;
- irrigation systems;
- weapons and turrets;
- input inventories of refineries, furnaces, and assemblers.

Output inventories from refineries, furnaces, and assemblers can still be collected normally.
Processed uranium stays in reactors, and ammunition stays in weapons and turrets.

### Troubleshooting

| Problem | Fix |
| --- | --- |
| No storage is found | Rename at least one container with `Stockage Base`, `Stockage Glace`, or `Stockage Brute`. |
| Ice is not moved | Add or free space in a `Stockage Glace` container. |
| Ore is not moved | Add or free space in a `Stockage Brute` container. |
| Ingots are not moved | Add or free space in a `Stockage Minerai` container. |
| Stone is not sent to refineries | Rename at least one refinery with `raff de pierre` and check its input inventory. |
| LCD is not detected | Rename the screen with `LCD Collecteur`. |
| Wrong screen is used | Change `LCD_SURFACE_INDEX`. |

### Patch Note 0.1

- First documented stable release.
- Automatic routing between main, ice, and raw resource storage.
- Protection for critical production and combat inventories.
- LCD report for transfers, blocked moves, and recent events.

---

## License & Author

Provided by **soraanoir/sundae https://github.com/soraanoir**.  
Free to use and modify, provided that credit is given to the author.
