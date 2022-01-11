using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Revitalize.Framework.Constants.ItemIds.Resources.EarthenResources;
using StardewValley.Menus;

namespace Revitalize.Framework.World.WorldUtilities.Shops
{
    public static class ClintsShopUtilities
    {

        /// <summary>
        /// Adds in ore to clint's shop.
        /// </summary>
        public static void AddOreToClintsShop(ShopMenu Menu)
        {
            ShopUtilities.AddItemToShop(Menu, ModCore.ObjectManager.GetItem(Ores.TinOre, 1), ModCore.Configs.shopsConfigManager.blacksmithShopsConfig.tinOreSellPrice, -1);
            ShopUtilities.AddItemToShop(Menu, ModCore.ObjectManager.GetItem(Ores.BauxiteOre, 1), ModCore.Configs.shopsConfigManager.blacksmithShopsConfig.bauxiteOreSellPrice, -1);
            ShopUtilities.AddItemToShop(Menu, ModCore.ObjectManager.GetItem(Ores.LeadOre, 1), ModCore.Configs.shopsConfigManager.blacksmithShopsConfig.leadOreSellPrice, -1);
            ShopUtilities.AddItemToShop(Menu, ModCore.ObjectManager.GetItem(Ores.SilverOre, 1), ModCore.Configs.shopsConfigManager.blacksmithShopsConfig.silverOreSellPrice, -1);
            ShopUtilities.AddItemToShop(Menu, ModCore.ObjectManager.GetItem(Ores.TitaniumOre, 1), ModCore.Configs.shopsConfigManager.blacksmithShopsConfig.titaniumOreSellPrice, -1);
        }

    }
}
