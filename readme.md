# Space Engineers Autochest System - Script Documentation

This *Space Engineers* programmable block script implements an **automatic inventory collection system** that scans connected inventories, routes items into dedicated storage containers, and protects production or utility blocks that should not be emptied.

---

## Table of Contents

* [Initialization and Configuration](#initialization-and-configuration)
* [Inventory Collection Logic](#inventory-collection-logic)
* [Protected Blocks and Inventories](#protected-blocks-and-inventories)
* [LCD Display Logic](#lcd-display-logic)
* [Troubleshooting](#troubleshooting)

---

## Initialization and Configuration

The system initializes by scanning the grid for storage containers and source inventories. Destination containers are detected by name, while the LCD is detected from a configurable display name.

### **Script Constants**
| Constant | Default Value | Description |
| --- | --- | --- |
| `MAIN_STORAGE_NAME` | `Stockage Base` | Cargo containers for regular items. Ice and raw refinery resources are excluded from this route. |
| `ICE_STORAGE_NAME` | `Stockage Glace` | Cargo containers dedicated to ice. |
| `RAW_STORAGE_NAME` | `Stockage Brute` | Cargo containers dedicated to raw refinery resources such as ores. |
| `LCD_NAME` | `LCD Collecteur` | LCD panel or display-capable block used for the status screen. |
| `LCD_SURFACE_INDEX` | `0` | Screen surface used when the named block has multiple displays. |
| `MAX_LCD_EVENTS` | `12` | Maximum number of recent transfer events shown on the LCD. |

### **Required Blocks**
| Block | Required Name / Rule | Description |
| --- | --- | --- |
| Cargo Container | Contains `Stockage Base` | Destination storage for regular items. Multiple containers are supported. |
| Cargo Container | Contains `Stockage Glace` | Destination storage for ice only. |
| Cargo Container | Contains `Stockage Brute` | Destination storage for raw refinery resources only. |
| LCD Panel or Screen Block | Contains `LCD Collecteur` | Optional status display. Can be a normal LCD or a cockpit/control-seat screen. |
| Programmable Block | Any name | Runs the script automatically every `Update100`. |

* **Storage naming:** Examples: `Stockage Base 1`, `Stockage Base Minerais`, `Large Cargo Stockage Base`.
* **Ice storage:** Ice is routed only to containers containing `Stockage Glace`.
* **Raw storage:** Ore and other raw refinery resources are routed only to containers containing `Stockage Brute`.
* **LCD naming:** The script first checks for the exact name `LCD Collecteur`, then falls back to any block name containing `LCD Collecteur`.
* **Multi-screen blocks:** If the wrong screen is used, change `LCD_SURFACE_INDEX` to `1`, `2`, or another valid surface number.

---

## Inventory Collection Logic

The script collects movable items from connected inventory blocks and sends them into the configured storage route.

### **Operating Logic**
1.  **Scan storage:** Finds all cargo containers whose name contains `Stockage Base`, `Stockage Glace`, or `Stockage Brute`.
2.  **Scan sources:** Finds connected blocks with inventories, excluding protected blocks.
3.  **Route:** Selects the correct destination type for each item.
4.  **Transfer:** Moves items from source inventories into the first available container for that route.
5.  **Report:** Writes a structured status report to the LCD and programmable block output.

If a destination container is full or cannot accept an item, the script tries the next configured container in the same route. If all transfers fail, the item is counted as an error. Ice and raw resources never fall back into `Stockage Base`.

### **Storage Routes**
| Item Type | Destination |
| --- | --- |
| Ice | `Stockage Glace` |
| Ores / raw refinery resources | `Stockage Brute` |
| Components, ingots, tools, bottles, ammunition, and other regular items | `Stockage Base` |

---

## Protected Blocks and Inventories

The script deliberately ignores blocks and inventories that should keep their contents.

### **Ignored Blocks**
| Block Type / Rule | Reason |
| --- | --- |
| Destination storage containers | Prevents moving items out of the final storage. |
| Programmable blocks | Avoids unnecessary inventory scans. |
| O2/H2 generators | Keeps ice and gas production stable. |
| Irrigation systems | Protects modded or utility inventories. |
| Weapons and turrets | Prevents ammo from being removed. |

### **Ignored Input Inventories**
| Block Type | Ignored Inventory | Reason |
| --- | --- | --- |
| Refineries / furnaces | Input inventory `0` | Keeps ores in processing. |
| Assemblers | Input inventory `0` | Keeps components and materials in production. |

The output inventories of refineries, furnaces, and assemblers can still be collected normally.

---

## LCD Display Logic

The LCD interface provides a compact overview of the collector state.

### **Displayed Information**
* **State:** Shows whether the collector is ready, completed transfers, or encountered blocked transfers.
* **Transfers:** Displays moved item count, failed transfer count, and ignored input count.
* **Network:** Displays scanned source count and configured storage count.
* **Routing:** Shows the active destination names for ice, raw resources, and regular items.
* **Protection:** Reminds which production inputs are protected.
* **Recent Actions:** Shows the latest transfer or blocked item events, limited by `MAX_LCD_EVENTS`.

The script automatically configures the LCD to:
* `TEXT_AND_IMAGE` mode
* `Monospace` font
* left alignment
* compact font size for cleaner dashboards

---

## Troubleshooting

| Problem | Fix |
| --- | --- |
| No storage is found | Rename at least one cargo container so its name contains `Stockage Base`, `Stockage Glace`, or `Stockage Brute`. |
| Ice is not moved | Add or free space in a cargo container containing `Stockage Glace`. |
| Ore is not moved | Add or free space in a cargo container containing `Stockage Brute`. |
| LCD is not detected | Rename the LCD or display-capable block so its name contains `LCD Collecteur`. |
| Wrong screen is used on a cockpit/control seat | Change `LCD_SURFACE_INDEX` to another surface number. |
| Items stay in refinery or assembler inputs | This is intentional; input inventories are protected. |
| Ammo is not moved | This is intentional; weapons and turrets are protected. |
| Some transfers fail | Add more storage space or check whether the item type is accepted by the destination inventory. |

---

## License & Author

Provided by **soraanoir/sundae https://github.com/soraanoir**.  
Free to use and modify, provided that credit is given to the author.
