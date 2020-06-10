using DaesMod.Data;
using DaesMod.Hooks;
using DaesMod.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.UI;

namespace DaesMod {
	class DaesGlobalTile : GlobalTile {
		private readonly Random rand = new Random();

		public override void DrawEffects(int i, int j, int type, SpriteBatch spriteBatch, ref Color drawColor, ref int nextSpecialDrawIndex) {
			DaesPlayer modPlayer = Main.LocalPlayer.GetModPlayer<DaesPlayer>();

			if (modPlayer.hasAnomalyDetector) {
				foreach (AnomalyHighlights anomalyHighlights in DataContainer.Anomalies) {
					if (!anomalyHighlights.Tiles.Contains(type))
						continue;

					byte R = anomalyHighlights.R;
					byte G = anomalyHighlights.G;
					byte B = anomalyHighlights.B;

					if (drawColor.R < R)
						drawColor.R = R;

					if (drawColor.G < G)
						drawColor.G = G;

					if (drawColor.B < B)
						drawColor.B = B;

					drawColor.A = Main.mouseTextColor;

					if (!Main.gamePaused && !Main.gameInactive && rand.Next(120) == 0) {
						Dust dust = Dust.NewDustDirect(new Vector2(i * 16, j * 16), 16, 16, anomalyHighlights.DustType, 0f, 0f, 150, default, 0.3f);
						dust.fadeIn = 1f;
						dust.velocity *= 0.1f;
						dust.noLight = true;
					}

					break;
				}
			}
		}
	}

	class DaesMod : Mod {
		public UserInterface ReforgeUserInterface;
		public ReforgeUI ReforgeUIInstance;
		public UserInterface EndlessAmmoUserInterface;
		public EndlessAmmoUI EndlessAmmoUIInstance;

		public override void Load() {
			IL.Terraria.Player.HasAmmo += Hook.HasAmmo;
			IL.Terraria.Player.PickAmmo += Hook.PickAmmo;
			IL.Terraria.UI.ItemSlot.Draw_SpriteBatch_ItemArray_int_int_Vector2_Color += Hook.ItemSlotDraw;

			if (Main.dedServ)
				return;

			DataContainer.Load();

			// Create Gnome Reforge Interface
			ReforgeUIInstance = new ReforgeUI();

			ReforgeUserInterface = new UserInterface();
			ReforgeUserInterface.SetState(ReforgeUIInstance);

			EndlessAmmoUIInstance = new EndlessAmmoUI();
			EndlessAmmoUserInterface = new UserInterface();
			EndlessAmmoUserInterface.SetState(EndlessAmmoUIInstance);
		}

		public override void Unload() {
			ReforgeUIInstance = null;
			ReforgeUserInterface = null;

			EndlessAmmoUIInstance = null;
			EndlessAmmoUserInterface = null;

			DataContainer.Unload();
			base.Unload();
		}

		public override void PreSaveAndQuit() {
			ReforgeUI.visible = false;
		}

		// Insert our interface layers
		public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers) {
			int inventoryIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Inventory"));
			if (inventoryIndex == -1)
				throw new Exception("Could not find 'Vanilla: Inventory'");

			layers.Insert(inventoryIndex, new LegacyGameInterfaceLayer(
				"DaesMod: Reforge UI",
				delegate {
					if (ReforgeUI.visible) {
						ReforgeUserInterface.Draw(Main.spriteBatch, new GameTime());
					}

					return true;
				},
				InterfaceScaleType.UI
			));

			layers.Insert(inventoryIndex, new LegacyGameInterfaceLayer(
				"DaesMod: Endless Ammo UI",
				delegate {
					if (Main.playerInventory) {
						EndlessAmmoUserInterface.Draw(Main.spriteBatch, new GameTime());
					} else {
						EndlessAmmoUI.AmmoPicker = AmmoID.None;
					}

					return true;
				},
				InterfaceScaleType.UI
			));
		}
	}
	internal class DaesPlayer : ModPlayer {
		public bool hasAnomalyDetector = false;
		public Item ReforgeItem = new Item();

		public bool hasEndlessAmmo = false;
		public bool useEndlessAmmoFirst = false;
		public List<Item> UnlockedAmmo = new List<Item>();
		public List<bool> SelectedAmmo = new List<bool>();

		public override TagCompound Save() {
			return new TagCompound {
				[nameof(ReforgeItem)] = ReforgeItem,
				[nameof(hasEndlessAmmo)] = hasEndlessAmmo,
				[nameof(useEndlessAmmoFirst)] = useEndlessAmmoFirst,
				[nameof(UnlockedAmmo)] = UnlockedAmmo,
				[nameof(SelectedAmmo)] = SelectedAmmo
			};
		}

		public override void Load(TagCompound tag) {
			ReforgeItem = tag.Get<Item>(nameof(ReforgeItem));
			hasEndlessAmmo = tag.GetBool(nameof(hasEndlessAmmo));
			useEndlessAmmoFirst = tag.GetBool(nameof(useEndlessAmmoFirst));
			UnlockedAmmo = tag.Get<List<Item>>(nameof(UnlockedAmmo));
			SelectedAmmo = tag.Get<List<bool>>(nameof(SelectedAmmo));
		}

		public override void ResetEffects() {
			hasAnomalyDetector = false;
		}

		public override void OnEnterWorld(Player player) {
			DataContainer.LoadItems();
			DropReforgeItem();
		}

		public void DropReforgeItem() {
			if (!ReforgeItem.IsAir) {
				Main.LocalPlayer.QuickSpawnClonedItem(ReforgeItem);
				ReforgeItem.TurnToAir();
			}
		}

		public void UnlockEndlessAmmo() {
			if (hasEndlessAmmo)
				return;

			hasEndlessAmmo = true;

			Item musketBall = new Item();
			musketBall.SetDefaults(ItemID.MusketBall);
			UnlockedAmmo.Add(musketBall);
			SelectedAmmo.Add(true);

			Item arrow = new Item();
			arrow.SetDefaults(ItemID.WoodenArrow);
			UnlockedAmmo.Add(arrow);
			SelectedAmmo.Add(true);
		}

		public Item GetItemForEndlessAmmoType(int ammoType) {
			if (!hasEndlessAmmo)
				return new Item();

			for (int i = 0; i < SelectedAmmo.Count; i++) {
				if (SelectedAmmo[i]) {
					Item selectedAmmo = UnlockedAmmo[i];
					if (selectedAmmo.ammo == ammoType)
						return selectedAmmo;
				}
			}

			for (int i = SelectedAmmo.Count; i < UnlockedAmmo.Count; i++) {
				SelectedAmmo.Add(false);
			}

			for (int i = 0; i < UnlockedAmmo.Count; i++) {
				Item possibleAmmo = UnlockedAmmo[i];
				if (possibleAmmo.ammo == ammoType) {
					SelectedAmmo[i] = true;
					return possibleAmmo;
				}
			}

			return new Item();
		}

		public bool HasEndlessAmmoItemUnlocked(int type) {
			foreach (Item unlocked in UnlockedAmmo) {
				if (unlocked.type == type)
					return true;
			}

			return false;
		}

		public bool SelectUnlockedAmmo(int type) {
			if (!HasEndlessAmmoItemUnlocked(type))
				return false;

			Item ammo = new Item();
			ammo.SetDefaults(type);

			int ammoType = ammo.ammo;
			for (int i = 0; i < UnlockedAmmo.Count; i++) {
				Item possibleAmmo = UnlockedAmmo[i];
				if (possibleAmmo.ammo == ammoType)
					SelectedAmmo[i] = possibleAmmo.type == type;
			}

			return true;
		}

		public bool UnlockEndlessAmmo(int type) {
			if (SelectUnlockedAmmo(type))
				return true;
			
			if (!ConsumeItem(type, EndlessAmmoUI.UNLOCK_AMOUNT)) return false;

			Item ammo = new Item();
			ammo.SetDefaults(type);
			UnlockedAmmo.Add(ammo);
			SelectedAmmo.Add(false);

			return SelectUnlockedAmmo(type);
		}

		public int CountItemsInInventory(int type) {
			int count = 0;
			for (int i = 0; i < 58; i++) {
				if (player.inventory[i].type == type) {
					count += player.inventory[i].stack;
				}
			}

			return count;
		}

		public bool ConsumeItem(int type, int count) {
			if (CountItemsInInventory(type) < count)
				return false;

			int remaining = count;
			for (int i = 0; i < 58 && remaining > 0; i++) {
				if (player.inventory[i].type != type)
					continue;

				if (player.inventory[i].stack > remaining) {
					player.inventory[i].stack -= remaining;
					break;
				}

				remaining -= player.inventory[i].stack;
				player.inventory[i].TurnToAir();
			}

			return true;
		}

		public bool CanUnlockAmmoForType(int type) {
			foreach (Item item in DataContainer.AmmoItems) {
				if (item.ammo != type)
					continue;

				if (HasEndlessAmmoItemUnlocked(item.type))
					continue;

				if (CountItemsInInventory(item.type) >= EndlessAmmoUI.UNLOCK_AMOUNT)
					return true;
			}
			return false;
		}
	}
}
