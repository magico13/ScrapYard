using KSP.UI.Screens;
using RUI.Icons.Selectable;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScrapYard.Utilities
{
    public class PartCategoryFilter
    {
        public PartCategoryFilter()
        {

        }


        public void CreateInventoryPartCategory()
        {
            IconLoader loader = UnityEngine.Object.FindObjectOfType<IconLoader>();
            Icon icon = loader.icons.FirstOrDefault(i => string.Equals(i.name, "stockIcon_structural", StringComparison.Ordinal));
            PartCategorizer.Category category =  PartCategorizer.AddCustomFilter("ScrapYard", "ScrapYard", icon, new UnityEngine.Color(0.75f, 0.75f, 0.75f));
            category.displayType = EditorPartList.State.PartsList;
            category.exclusionFilter = PartCategorizer.Instance.filterGenericNothing;

            PartCategorizer.AddCustomSubcategoryFilter(category, "Inventory", "Inventory", icon, Filter);
        }


        public bool Filter(AvailablePart part)
        {
            return (ScrapYard.Instance.TheInventory.FindPartsByName(part.name)?.Any() == true);
        }

    }
}

/*
 * This was made possible by using Filter Extensions by Crzyrndm as a reference
 * Filter Extensions is licensed GPLv3
 * https://forum.kerbalspaceprogram.com/index.php?/topic/93955-130-filter-extensions-304-jul-11/
 * https://github.com/Crzyrndm/FilterExtension
 */