using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StardewValley;
using StardewValley.Menus;

namespace Revitalize.Framework.World.WorldUtilities.Shops
{
    public static class DwarfShopUtilities
    {

        public static int DwarfShop_NormalGeodesRemainingToday;
        public static int DwarfShop_FrozenGeodesRemainingToday;
        public static int DwarfShop_MagmaGeodesRemainingToday;
        public static int DwarfShop_OmniGeodesRemainingToday;

        public static Func<ISalable, Farmer, int, bool> DwarfShop_DefaultOnPurchaseMethod;

        public static void OnNewDay(object sender, StardewModdingAPI.Events.DayStartedEventArgs args)
        {
            DwarfShop_NormalGeodesRemainingToday = ModCore.Configs.shopsConfigManager.dwarfShopConfig.NumberOfNormalGeodesToSell;
            if (Game1.player.deepestMineLevel >= 40)
            {
                DwarfShop_FrozenGeodesRemainingToday = ModCore.Configs.shopsConfigManager.dwarfShopConfig.NumberOfFrozenGeodesToSell;
            }
            if (Game1.player.deepestMineLevel >= 80)
            {
                DwarfShop_MagmaGeodesRemainingToday = ModCore.Configs.shopsConfigManager.dwarfShopConfig.NumberOfMagmaGeodesToSell;
            }
            if (Game1.player.hasSkullKey && (Game1.dayOfMonth % 7 == 0 || ModCore.Configs.shopsConfigManager.dwarfShopConfig.SellOmniGeodesEveryDayInsteadOnJustSundays))
            {
                //Add 1 omni geode on sundays.
                DwarfShop_OmniGeodesRemainingToday = ModCore.Configs.shopsConfigManager.dwarfShopConfig.NumberOfOmniGeodesToSell;
            }
            else
            {
                DwarfShop_OmniGeodesRemainingToday = 0;
            }
        }

        public static void AddGeodesToDwarfShop(ShopMenu Menu)
        {
            DwarfShop_DefaultOnPurchaseMethod = Menu.onPurchase;
            Menu.onPurchase = OnPurchaseFromDwarfShop;

            if (DwarfShop_NormalGeodesRemainingToday > 0)
            {
               ShopUtilities.AddItemToShop(Menu, ModCore.ObjectManager.GetItem(Enums.SDVObject.Geode, DwarfShop_NormalGeodesRemainingToday), ModCore.Configs.shopsConfigManager.dwarfShopConfig.NormalGeodePrice, DwarfShop_NormalGeodesRemainingToday);
            }
            if (DwarfShop_FrozenGeodesRemainingToday > 0)
            {
                ShopUtilities.AddItemToShop(Menu, ModCore.ObjectManager.GetItem(Enums.SDVObject.FrozenGeode, DwarfShop_FrozenGeodesRemainingToday), ModCore.Configs.shopsConfigManager.dwarfShopConfig.FrozenGeodePrice, DwarfShop_FrozenGeodesRemainingToday);
            }
            if (DwarfShop_MagmaGeodesRemainingToday > 0)
            {
                ShopUtilities.AddItemToShop(Menu, ModCore.ObjectManager.GetItem(Enums.SDVObject.MagmaGeode, DwarfShop_MagmaGeodesRemainingToday), ModCore.Configs.shopsConfigManager.dwarfShopConfig.MagmaGeodePrice, DwarfShop_MagmaGeodesRemainingToday);
            }
            if (DwarfShop_OmniGeodesRemainingToday > 0)
            {
                ShopUtilities.AddItemToShop(Menu, ModCore.ObjectManager.GetItem(Enums.SDVObject.OmniGeode, DwarfShop_OmniGeodesRemainingToday), ModCore.Configs.shopsConfigManager.dwarfShopConfig.OmniGeodePrice, DwarfShop_OmniGeodesRemainingToday);
            }
        }


        private static bool OnPurchaseFromDwarfShop(ISalable purchasedItem, Farmer who, int AmountPurchased)
        {
            if (purchasedItem is StardewValley.Object)
            {
                StardewValley.Object itemForSale = (purchasedItem as StardewValley.Object);
                if (itemForSale.parentSheetIndex == (int)Enums.SDVObject.Geode)
                {
                    DwarfShop_NormalGeodesRemainingToday -= AmountPurchased;
                    return false;
                }
                if (itemForSale.parentSheetIndex == (int)Enums.SDVObject.FrozenGeode)
                {
                    DwarfShop_FrozenGeodesRemainingToday -= AmountPurchased;
                    return false;
                }
                if (itemForSale.parentSheetIndex == (int)Enums.SDVObject.MagmaGeode)
                {
                    DwarfShop_MagmaGeodesRemainingToday -= AmountPurchased;
                    return false;
                }
                if (itemForSale.parentSheetIndex == (int)Enums.SDVObject.OmniGeode)
                {
                    DwarfShop_OmniGeodesRemainingToday -= AmountPurchased;
                    return false;
                }
            }

            if (DwarfShop_DefaultOnPurchaseMethod != null)
            {
                return DwarfShop_DefaultOnPurchaseMethod.Invoke(purchasedItem, who, AmountPurchased);
            }

            return false;
        }

    }
}
