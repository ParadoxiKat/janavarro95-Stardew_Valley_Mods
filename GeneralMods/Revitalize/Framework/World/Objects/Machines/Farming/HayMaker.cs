using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using Omegasis.Revitalize.Framework.Constants;
using Omegasis.Revitalize.Framework.Constants.ItemIds.Items;
using Omegasis.Revitalize.Framework.Constants.PathConstants;
using Omegasis.Revitalize.Framework.Crafting;
using Omegasis.Revitalize.Framework.Player;
using Omegasis.Revitalize.Framework.Utilities;
using Omegasis.Revitalize.Framework.Utilities.JsonContentLoading;
using Omegasis.Revitalize.Framework.World.Objects.InformationFiles;
using Omegasis.Revitalize.Framework.World.Objects.Items.Utilities;
using Omegasis.Revitalize.Framework.World.Objects.Machines;
using Omegasis.Revitalize.Framework.World.WorldUtilities;
using Omegasis.Revitalize.Framework.World.WorldUtilities.Shops.RevitalizeShops;
using StardewValley;
using StardewValley.Menus;

namespace Omegasis.Revitalize.Framework.World.Objects.Farming
{
    [XmlType("Mods_Revitalize.Framework.World.Objects.Farming.HayMaker")]
    public class HayMaker : Machine
    {
        public readonly NetRef<ItemReference> feedType = new NetRef<ItemReference>(new ItemReference());

        public const string AmaranthAnimation = "Amaranth";
        public const string CornAnimation = "Corn";
        public const string HayAnimation = "Hay";
        public const string FiberAnimation = "Fiber";
        public const string WheatAnimation = "Wheat";

        public NetBool isUsedForBuyingHayAtAnyTime = new NetBool();

        public HayMaker()
        {

        }

        public HayMaker(BasicItemInformation info, bool isUsedForBuyingHayAtAnyTime = false) : base(info)
        {
            this.isUsedForBuyingHayAtAnyTime.Value = isUsedForBuyingHayAtAnyTime;
            if (this.isUsedForBuyingHayAtAnyTime.Value == true && RevitalizeModCore.Configs.shopsConfigManager.hayMakerShopConfig.IsHayMakerShopUpAgainstAWall)
                this.basicItemInformation.boundingBoxTileDimensions.Value = new Vector2(1, 1);
            if (this.isUsedForBuyingHayAtAnyTime.Value == true)
                this.AnimationManager.playAnimation(HayAnimation);

        }

        protected override void initializeNetFieldsPostConstructor()
        {
            base.initializeNetFieldsPostConstructor();
            this.NetFields.AddField(this.feedType);
            this.NetFields.AddField(this.isUsedForBuyingHayAtAnyTime);
        }

        /// <summary>
        /// What happens when a tool is used on this machine.
        /// </summary>
        /// <param name="t"></param>
        /// <param name="location"></param>
        /// <returns></returns>
        public override bool performToolAction(Tool t, GameLocation location)
        {
            if (this.isUsedForBuyingHayAtAnyTime.Value == true)
                return false;

            return base.performToolAction(t, location);
        }


        public override bool canBePlacedHere(GameLocation l, Vector2 tile)
        {
            if (this.exceptionForPlacementIsValidForMarniesRanch(l, tile) == true) return true;
            return base.canBePlacedHere(l, tile);
        }

        /// <summary>
        /// Checks to see if this is special handling fopr the machine outside of marnies ranch.
        /// </summary>
        /// <param name="location"></param>
        /// <param name="tile"></param>
        /// <returns></returns>
        protected virtual bool exceptionForPlacementIsValidForMarniesRanch(GameLocation location, Vector2 tile)
        {
            if (this.isUsedForBuyingHayAtAnyTime.Value == true && tile.Equals(RevitalizeModCore.Configs.shopsConfigManager.hayMakerShopConfig.HayMakerTileLocation) && location.Name.Equals("Forest"))
                return true;
            return false;
        }

        public override bool canBeRemoved(Farmer who)
        {
            if (this.isUsedForBuyingHayAtAnyTime.Value == true)
                return false;

            return base.canBeRemoved(who);
        }

        public override bool rightClicked(Farmer who)
        {

            if (this.isUsedForBuyingHayAtAnyTime.Value == true)
            {
                if (Game1.activeClickableMenu == null)
                {
                    
                    ShopMenu shopMenu = new ShopMenu(new Dictionary<ISalable, int[]>());
                    shopMenu.storeContext = HayMakerShopUtilities.StoreContext;
                    shopMenu.potraitPersonDialogue = JsonUtilities.LoadStringFromDictionaryFile("ShopText_1", Path.Combine(StringsPaths.ShopDialogue, "HayMakerShopDialogue.json"));
                    Game1.activeClickableMenu = shopMenu;
                }

                return true;
            }

            if (this.heldObject.Value != null && this.MinutesUntilReady == 0)
                if (who.IsLocalPlayer)
                    this.cleanOutHayMaker(true);

            return base.rightClicked(who);
        }

        /// <summary>
        /// Cleans out the hay maker to produce more hay.
        /// </summary>
        /// <param name="addToPlayersInventory"></param>
        protected virtual void cleanOutHayMaker(bool addToPlayersInventory)
        {
            if (addToPlayersInventory)
            {
                SoundUtilities.PlaySound(Enums.StardewSound.coin);
                bool added = Game1.player.addItemToInventoryBool(this.heldObject.Value);
                if (added == false) return;
            }
            this.heldObject.Value = null;
            this.AnimationManager.playDefaultAnimation();
            this.feedType.Value.clearItemReference();

        }

        /// <summary>
        /// Called when a new day is started. Attempt to refill the silos from the hay maker.
        /// </summary>
        /// <param name="location"></param>
        public override void doActualDayUpdateLogic(GameLocation location)
        {
            if (this.heldObject.Value != null)
            {
                this.attemptToFillFarmSilos();
                this.cleanOutHayMaker(false);
            }
        }

        /// <summary>
        /// Performed when dropping in an object into this feeder.
        /// </summary>
        /// <param name="dropInItem"></param>
        /// <param name="probe"></param>
        /// <param name="who"></param>
        /// <returns></returns>
        public override bool performObjectDropInAction(Item dropInItem, bool probe, Farmer who)
        {
            if (probe == true) return false; //Just checking for action.
            if (who.ActiveObject == null) return false;
            if (dropInItem == null) return false;
            if (this.MinutesUntilReady > 0) return false;
            if (this.isUsedForBuyingHayAtAnyTime.Value == true) return false;
            if (this.heldObject.Value != null && this.MinutesUntilReady==0)
                this.cleanOutHayMaker(true);

            return this.processItemFromRecipe(dropInItem, who).successful;
        }


        /// <summary>
        /// Processes a player's item that they are holding to set recipe to be processed for the hay maker.
        /// </summary>
        /// <param name="dropInItem"></param>
        /// <param name="who"></param>
        /// <param name="ShowRedMessage"></param>
        /// <returns></returns>
        public virtual CraftingRecipeResult processItemFromRecipe(Item dropInItem, Farmer who, bool ShowRedMessage = true)
        {
            foreach (var craftingRecipe in RevitalizeModCore.ModContentManager.craftingManager.getUnlockedCraftingRecipes(this.getCraftingBookName()))
            {
                Item neededDropInItem = craftingRecipe.ingredients[0].item;
                int amountRequired = craftingRecipe.ingredients[0].requiredAmount;

                ItemReference itemRef = new ItemReference(neededDropInItem);

                if (neededDropInItem.canStackWith(dropInItem) || itemRef.itemEquals(dropInItem))
                {
                    //Check to make sure the player has enough, otherwise display an error!
                    if (amountRequired > dropInItem.Stack)
                    {
                        if (ShowRedMessage)
                        {
                            Game1.showRedMessage(this.getErrorString_NeedMoreInputItems(amountRequired, dropInItem));
                        }
                        return new CraftingRecipeResult(craftingRecipe, false);
                    }

                    this.feedType.Value.setItemReference(neededDropInItem);
                    Item outputItem = craftingRecipe.outputs[0].item.getOne();

                    //Allow expensive crops such as amaranth and iridium quality corn to get a small bonus, since otherwise wheat would be the best item to process.
                    int additionalHayPieces = 0;
                    if(dropInItem is StardewValley.Object)
                    {
                        long playerBonusId = -1;
                        if (who == null)
                        {
                            playerBonusId = this.owner.Value;
                        }
                        else
                        {
                            playerBonusId = who.uniqueMultiplayerID.Value;
                        }
                        additionalHayPieces += (dropInItem as StardewValley.Object).sellToStorePrice(playerBonusId) / 100;
                    }

                    outputItem.Stack = craftingRecipe.outputs[0].requiredAmount + additionalHayPieces;
                    this.heldObject.Value = (StardewValley.Object)outputItem;
                    this.MinutesUntilReady = (int)(craftingRecipe.timeToCraft);
                    if (who != null)
                    {
                        SoundUtilities.PlaySound(Enums.StardewSound.Ship);
                    }
                    //Due to a quirk on how the game's logic works, when this method returns true and we return true from performObjectDrop in, the active item is naturally reduced by 1 anyways, so we want skip removing too many items from the player's inventory.
                    PlayerUtilities.ReduceInventoryItemStackSize(who, dropInItem, amountRequired-1);
                    this.updateAnimation();

                    return new CraftingRecipeResult(craftingRecipe, true); //Found a sucessful recipe.
                }
            }
            return new CraftingRecipeResult(null, false);
        }

        /// <summary>
        /// Updates the animation for display for this machine.
        /// </summary>
        public virtual void updateAnimation()
        {
            if (this.feedType.Value.SdvObjectId == Enums.SDVObject.Corn)
            {
                this.AnimationManager.playAnimation(CornAnimation);
            }
            else if (this.feedType.Value.SdvObjectId == Enums.SDVObject.Fiber)
            {
                this.AnimationManager.playAnimation(FiberAnimation);
            }
            else if (this.feedType.Value.SdvObjectId == Enums.SDVObject.Amaranth)
            {
                this.AnimationManager.playAnimation(AmaranthAnimation);
            }
            else if (this.feedType.Value.SdvObjectId == Enums.SDVObject.Wheat)
            {
                this.AnimationManager.playAnimation(WheatAnimation);
            }
            else
            {
                this.AnimationManager.playDefaultAnimation();
            }
        }

        public virtual string getCraftingBookName()
        {
            return Constants.CraftingIds.MachineCraftingRecipeBooks.HayMakerRecipes;
        }

        public override bool minutesElapsed(int minutes, GameLocation environment)
        {
            if (this.heldObject.Value == null) return false;
            base.minutesElapsed(minutes, environment);

            if (this.MinutesUntilReady == 0 && this.heldObject.Value != null)
            {
                this.AnimationManager.playAnimation(HayAnimation);
                bool noHayRemainsInFeedMaker = this.attemptToFillFarmSilos();
                if (noHayRemainsInFeedMaker == false)
                    //swip and coin are valid sounds too.
                    SoundUtilities.PlaySound(Enums.StardewSound.dwop);
            }
            return true;
        }

        /// <summary>
        /// Attempts to automatically remove hay in the hay maker and put it into farm's Silo.
        /// </summary>
        protected virtual bool attemptToFillFarmSilos()
        {
            if (this.heldObject.Value == null) return false;
            int remainder = Game1.getFarm().tryToAddHay(this.heldObject.Value.Stack);
            if (remainder == 0)
            {
                this.cleanOutHayMaker(false);
                return true;
            }
            else
            {
                this.heldObject.Value.Stack = remainder;
                return false;
            }
        }

        protected override void drawStatusBubble(SpriteBatch b, int x, int y, float Alpha)
        {
            if (this.machineStatusBubbleBox == null || this.machineStatusBubbleBox.Value == null) this.createStatusBubble();
            if (this.MinutesUntilReady == 0 && this.heldObject.Value != null)
            {
                y--;
                float num = (float)(4.0 * Math.Round(Math.Sin(DateTime.UtcNow.TimeOfDay.TotalMilliseconds / 250.0), 2));
                this.machineStatusBubbleBox.Value.playAnimation(MachineStatusBubble_BlankBubbleAnimationKey);
                this.machineStatusBubbleBox.Value.draw(b, this.machineStatusBubbleBox.Value.getTexture(), Game1.GlobalToLocal(Game1.viewport, new Vector2(x * Game1.tileSize, y * Game1.tileSize + num)), new Rectangle?(this.machineStatusBubbleBox.Value.getCurrentAnimationFrameRectangle()), Color.White, 0f, Vector2.Zero, Game1.pixelZoom, SpriteEffects.None, Math.Max(0f, (y + 2) * Game1.tileSize / 10000f) + .00001f);

                Rectangle itemSourceRectangle = GameLocation.getSourceRectForObject(this.heldObject.Value.ParentSheetIndex);
                this.machineStatusBubbleBox.Value.draw(b, Game1.objectSpriteSheet, Game1.GlobalToLocal(Game1.viewport, new Vector2((float)(x * Game1.tileSize) + 8, y * Game1.tileSize + num + 16)), new Rectangle?(itemSourceRectangle), Color.White, 0f, Vector2.Zero, Game1.pixelZoom, SpriteEffects.None, Math.Max(0f, (y + 2) * Game1.tileSize / 10000f) + .00002f);

            }
        }

        /// <summary>
        /// A simple method for calculating the scale size for showing a machine working.
        /// </summary>
        /// <returns></returns>
        public override float getScaleSizeForWorkingMachine()
        {
            float zoomSpeed = 0.01f;
            if (this.Scale.X < Game1.pixelZoom)
                this.Scale = new Vector2(Game1.pixelZoom, Game1.pixelZoom);

            if (this.feedType.Value.isNotNull() && this.MinutesUntilReady > 0)
            {
                if (this.lerpScaleIncreasing.Value == true)
                {
                    this.Scale = new Vector2(this.scale.X + zoomSpeed, this.scale.Y + zoomSpeed);
                    if (this.Scale.X >= 5.0)
                        this.lerpScaleIncreasing.Value = false;
                }
                else
                {
                    this.Scale = new Vector2(this.scale.X - zoomSpeed, this.scale.Y - zoomSpeed);
                    if (this.Scale.X <= Game1.pixelZoom)
                        this.lerpScaleIncreasing.Value = true;
                }
                return this.Scale.X * Game1.options.zoomLevel;

            }
            else
            {
                float zoom = Game1.pixelZoom * Game1.options.zoomLevel;
                return zoom;
            }
        }

        /// <summary>What happens when the object is drawn at a tile location.</summary>
        public override void draw(SpriteBatch spriteBatch, int x, int y, float alpha = 1f)
        {

            x = (int)this.TileLocation.X;

            y = (int)this.TileLocation.Y;


            if (this.feedType.Value.isNotNull() && this.MinutesUntilReady > 0)
            {
                Vector2 origin = new Vector2(this.AnimationManager.getCurrentAnimationFrameRectangle().Width / 2, this.AnimationManager.getCurrentAnimationFrameRectangle().Height);

                this.basicItemInformation.animationManager.draw(spriteBatch, this.basicItemInformation.animationManager.getTexture(), Game1.GlobalToLocal(Game1.viewport, new Vector2((float)((x + this.basicItemInformation.drawOffset.X) * Game1.tileSize) + this.basicItemInformation.shakeTimerOffset() + Game1.tileSize * origin.X / this.AnimationManager.getCurrentAnimationFrameRectangle().Width, (y + this.basicItemInformation.drawOffset.Y) * Game1.tileSize + this.basicItemInformation.shakeTimerOffset() + Game1.tileSize * (origin.Y / this.AnimationManager.getCurrentAnimationFrameRectangle().Height + 1))), new Rectangle?(this.AnimationManager.getCurrentAnimation().getCurrentAnimationFrameRectangle()), this.basicItemInformation.DrawColor * alpha, 0f, origin, this.getScaleSizeForWorkingMachine(), this.flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Math.Max(0f, (this.TileLocation.Y - this.basicItemInformation.drawOffset.Y) * Game1.tileSize / 10000f) + .00001f);
            }
            else
                this.basicItemInformation.animationManager.draw(spriteBatch, this.basicItemInformation.animationManager.getTexture(), Game1.GlobalToLocal(Game1.viewport, new Vector2((float)((x + this.basicItemInformation.drawOffset.X) * Game1.tileSize) + this.basicItemInformation.shakeTimerOffset(), (y + this.basicItemInformation.drawOffset.Y) * Game1.tileSize + this.basicItemInformation.shakeTimerOffset())), new Rectangle?(this.AnimationManager.getCurrentAnimation().getCurrentAnimationFrameRectangle()), this.basicItemInformation.DrawColor * alpha, 0f, Vector2.Zero, Game1.pixelZoom, this.flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Math.Max(0f, (this.TileLocation.Y - this.basicItemInformation.drawOffset.Y) * Game1.tileSize / 10000f) + .00001f);

            if (this.MinutesUntilReady == 0 && this.heldObject.Value != null)
                this.drawStatusBubble(spriteBatch, x + (int)this.basicItemInformation.drawOffset.X, y + (int)this.basicItemInformation.drawOffset.Y, alpha);

        }


        public override Item getOne()
        {
            return new HayMaker(this.basicItemInformation.Copy(), this.isUsedForBuyingHayAtAnyTime);
        }

        public virtual Item getOne(bool IsUsedForBuyingHayAtAnyTime)
        {
            return new HayMaker(this.basicItemInformation.Copy(), IsUsedForBuyingHayAtAnyTime);
        }
    }
}
