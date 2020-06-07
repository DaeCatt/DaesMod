using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.UI;
using Terraria.UI.Chat;

namespace DaesMod.UI {
	class ReforgeUI : UIState {
		internal static string ValueIDPrefix = $"Mods.{nameof(DaesMod)}.{nameof(ReforgeUI)}.NPCChat.";

		public static bool visible = false;

		public static Item lastItem = null;
		// Allow up to 15 purchasable items
		public static Item[] purchasableItems = new Item[15];
		public static int purchasableItemsLength = 0;

		// List of all the highest-stat accessory prefixes.
		public static int[] accessoryPrefixes = {
			PrefixID.Warding,
			PrefixID.Arcane,
			PrefixID.Lucky,
			PrefixID.Menacing,
			PrefixID.Quick,
			PrefixID.Violent
		};

		public static int[] weaponPrefixes = {
			// Melee
			PrefixID.Legendary,
			PrefixID.Godly,
			
			// Tools
			PrefixID.Light,
			PrefixID.Massive,
			
			// Ranged & Magic
			PrefixID.Unreal,
			PrefixID.Demonic,
			PrefixID.Mythical,

			// Summon
			PrefixID.Ruthless
		};

		/*public override void OnInitialize() {
			// We might want to use vanilla panels here, but for now do nothing.
		}*/

		public static void ClearCurrentPrefixes() {
			purchasableItemsLength = 0;
		}

		public static void UpdateCurrentPrefixesForItem(Item item) {
			// Avoid re-calculating best prefixes if the item hasn't changed.
			if (item == lastItem) {
				return;
			} else {
				lastItem = item;
			}

			purchasableItemsLength = 0;
			if (item.IsAir)
				return;

			if (item.vanity)
				return;

			int[] prefixList = item.accessory ? accessoryPrefixes : weaponPrefixes;

			foreach (int prefixID in prefixList) {
				if (item.prefix == prefixID || purchasableItemsLength == 15) {
					continue;
				}

				Item clone = new Item();
				clone.netDefaults(item.netID);
				clone = clone.CloneWithModdedDataFrom(item);
				clone.Prefix(prefixID);
				if (clone.prefix != prefixID) {
					continue;
				}

				purchasableItems[purchasableItemsLength] = clone;
				purchasableItemsLength++;
			}

			/**
			 if (item.accessory) {
				foreach (int prefixID in accessoryPrefixes) {
					if (ItemLoader.AllowPrefix(item, prefixID)) {
						AddPurchaseableItemWithPrefix(item, prefixID);
					}
				}
			} else {
				// Add Godly or Demonic
				if (item.knockBack > 0) {
					AddPurchaseableItemWithPrefix(item, PrefixID.Godly);
				} else {
					AddPurchaseableItemWithPrefix(item, PrefixID.Demonic);
				}

				if (item.axe > 0 || item.hammer > 0 || item.pick > 0) {
					// Skip drills
					if (!item.channel) {
						AddPurchaseableItemWithPrefix(item, PrefixID.Light);
						AddPurchaseableItemWithPrefix(item, PrefixID.Massive);
					}
				} else if (item.melee) {
					// Skip spears
					if (!item.noMelee) {
						AddPurchaseableItemWithPrefix(item, PrefixID.Legendary);
					}
				} else if (item.ranged) {
					// TODO: Investigate weird harpoon restriction
					if (item.knockBack > 0) {
						AddPurchaseableItemWithPrefix(item, PrefixID.Unreal);
					}
				} else if (item.summon) {
					AddPurchaseableItemWithPrefix(item, PrefixID.Ruthless);
				} else if (item.magic) {
					AddPurchaseableItemWithPrefix(item, PrefixID.Mythical);
				}
			}
			*/

			// TODO: Support mod prefixes.
		}

		public static void ShowInterface() {
			// Remove any NPC Chat Box
			Main.npcChatText = "";

			// Open the player inventory and hide the crafting menu
			Main.playerInventory = true;
			Main.HidePlayerCraftingMenu = true;

			// Play the menu opening sound
			Main.PlaySound(SoundID.MenuTick);
			visible = true;
		}

		public static void HideInterface(DaesPlayer player) {
			// Drop item from reforge item slot
			player.DropReforgeItem();

			// Clear list of purchasable items
			ClearCurrentPrefixes();

			// Allow the crafting menu to be shown again
			Main.HidePlayerCraftingMenu = false;
			visible = false;
		}

		public override void Draw(SpriteBatch spriteBatch) {
			// Hide the crafting menu
			Main.HidePlayerCraftingMenu = true;

			// Get reference to current player
			DaesPlayer player = Main.LocalPlayer.GetModPlayer<DaesPlayer>();

			/**
			 * If the player:
			 * - has closed their inventory
			 * - has opened a chest
			 * - has opened another npcs shop
			 * - is no longer in range of the gnome
			 * Close the reforge ui.
			 */

			if (!Main.playerInventory || Main.player[Main.myPlayer].chest != -1 || Main.npcShop != 0 || Main.player[Main.myPlayer].talkNPC == -1) {
				HideInterface(player);
				return;
			}

			/**
			 * Create a point for where the mouse is. Used to check whether the
			 * cursor is inside certain regions of the interface.
			 */
			Point mousePoint = new Point(Main.mouseX, Main.mouseY);

			// Calculate Position of ItemSlot
			Main.inventoryScale = 0.85f;
			float xPosition = 50f;
			float yPosition = Main.instance.invBottom + 12f;

			// Pre-calculate slot width and height.
			int slotWidth = (int) (Main.inventoryBackTexture.Width * Main.inventoryScale);
			int slotHeight = (int) (Main.inventoryBackTexture.Height * Main.inventoryScale);

			// Create our "collision" rectangle
			Rectangle slotRectangle = new Rectangle((int) xPosition, (int) yPosition, slotWidth, slotHeight);
			if (slotRectangle.Contains(mousePoint)) {
				Main.LocalPlayer.mouseInterface = true;
				if (Main.mouseLeftRelease && Main.mouseLeft) {
					// Item.Prefix(-3) checks whether an item can be reforged.
					if (Main.mouseItem.IsAir || Main.mouseItem.Prefix(-3)) {
						Utils.Swap(ref player.ReforgeItem, ref Main.mouseItem);

						if (!Main.mouseItem.IsAir || !player.ReforgeItem.IsAir) {
							Main.PlaySound(SoundID.Grab);
						}
					}

					UpdateCurrentPrefixesForItem(player.ReforgeItem);
				} else {
					ItemSlot.MouseHover(new Item[] { player.ReforgeItem }, ItemSlot.Context.PrefixItem);
				}
			}

			player.ReforgeItem.newAndShiny = false;
			ItemSlot.Draw(Main.spriteBatch, new Item[] { player.ReforgeItem }, ItemSlot.Context.PrefixItem, 0, new Vector2(xPosition, yPosition), default);

			// If there's no purchasable prefixes, stop drawing the interface.
			if (purchasableItemsLength > 0) {
				xPosition += slotWidth + 8;
				string labelText = Language.GetTextValue(ValueIDPrefix + "ShopLabel");
				ChatManager.DrawColorCodedStringWithShadow(Main.spriteBatch, Main.fontMouseText, labelText, new Vector2(xPosition, yPosition), Color.White, 0f, Vector2.Zero, Vector2.One, -1f, 2f);
				yPosition -= slotHeight / 2;

				// List all purchasable prefixes.
				for (int i = 0; i < purchasableItemsLength; i++) {
					yPosition += slotHeight + 8;

					Item item = purchasableItems[i];
					int buyCost = item.value * 2;
					string price = FormatCoinCost(buyCost);

					// Draw item slot containing the item with the purchasable prefix
					ItemSlot.Draw(Main.spriteBatch, purchasableItems, ItemSlot.Context.CraftingMaterial, i, new Vector2(xPosition, yPosition), default);

					// Draw the cost of the item
					ChatManager.DrawColorCodedStringWithShadow(Main.spriteBatch, Main.fontMouseText, price, new Vector2(xPosition + slotWidth + 8, yPosition), Color.White, 0f, Vector2.Zero, Vector2.One, -1f, 2f);

					// TODO: Investigate this re-assignment
					purchasableItems[i] = item;

					slotRectangle = new Rectangle((int) xPosition, (int) yPosition, slotWidth, slotHeight);
					if (!slotRectangle.Contains(mousePoint)) {
						continue;
					}

					Main.LocalPlayer.mouseInterface = true;
					ItemSlot.MouseHover(purchasableItems, ItemSlot.Context.PrefixItem, i);

					// Check if the player has clicked to buy the reforge AND can afford it.
					if (Main.mouseItem.IsAir && Main.mouseLeftRelease && Main.mouseLeft && Main.player[Main.myPlayer].CanBuyItem(buyCost, -1)) {
						Main.mouseLeft = false;
						Main.mouseLeftRelease = false;

						Main.player[Main.myPlayer].BuyItem(buyCost, -1);

						item.favorited = player.ReforgeItem.favorited;
						item.stack = player.ReforgeItem.stack;
						item.position.X = Main.player[Main.myPlayer].position.X + (Main.player[Main.myPlayer].width / 2) - (item.width / 2);
						item.position.Y = Main.player[Main.myPlayer].position.Y + (Main.player[Main.myPlayer].height / 2) - (item.height / 2);

						Main.mouseItem = item;
						player.ReforgeItem.TurnToAir();
						ItemText.NewText(item, item.stack, noStack: true);
						Main.PlaySound(SoundID.Item37);

						ClearCurrentPrefixes();
						// player.ReforgeItem = item;
						// ClearCurrentPrefixes();
						break;
					}
				}
			}

			base.Draw(spriteBatch);
		}

		// TODO: Move this to somewhere more appropriate?
		public static string FormatCoinCost(int value) {
			int copper = value;
			int silver = 0;
			int gold = 0;
			int platinum = 0;

			if (copper >= 100) {
				silver = copper / 100;
				copper %= 100;
			}

			if (silver >= 100) {
				gold = silver / 100;
				silver %= 100;
			}

			if (gold >= 100) {
				platinum = gold / 100;
				gold %= 100;
			}

			string tagString = "";
			if (platinum > 0) {
				tagString = tagString + "[c/" + Colors.AlphaDarken(Colors.CoinPlatinum).Hex3() + ":" + platinum + " " + Language.GetTextValue("LegacyInterface.15") + "] ";
			}

			if (gold > 0) {
				tagString = tagString + "[c/" + Colors.AlphaDarken(Colors.CoinGold).Hex3() + ":" + gold + " " + Language.GetTextValue("LegacyInterface.16") + "] ";
			}

			if (silver > 0) {
				tagString = tagString + "[c/" + Colors.AlphaDarken(Colors.CoinSilver).Hex3() + ":" + silver + " " + Language.GetTextValue("LegacyInterface.17") + "] ";
			}

			if (copper > 0) {
				tagString = tagString + "[c/" + Colors.AlphaDarken(Colors.CoinCopper).Hex3() + ":" + copper + " " + Language.GetTextValue("LegacyInterface.18") + "] ";
			}

			return tagString;
		}
	}
}
