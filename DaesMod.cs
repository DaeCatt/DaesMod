using DaesMod.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.UI;
using static Mono.Cecil.Cil.OpCodes;

namespace DaesMod {
	public struct AnomalyHighlights {
		public byte R;
		public byte G;
		public byte B;
		public int DustType;
		public int[] Tiles;

		public AnomalyHighlights(int rgb, int dustType, int[] tiles) {
			R = (byte) ((rgb >> 16) & 0xff);
			G = (byte) ((rgb >> 8) & 0xff);
			B = (byte) (rgb & 0xff);
			DustType = dustType;
			Tiles = tiles;
		}
	}

	public struct EndlessAmmoType {
		public int Type;
		public string Path;

		public EndlessAmmoType(int type, string path) {
			Type = type;
			Path = path;
		}
	}

	class DaesGlobalTile : GlobalTile {
		private readonly Random rand = new Random();

		private readonly AnomalyHighlights[] Anomalies = {
			// Hallow
			new AnomalyHighlights(0x4EC1E3, 63, new int[] {
				TileID.HallowedGrass, // 109
				TileID.HallowedPlants, // 110
				TileID.HallowedPlants2, // 113
				TileID.Pearlstone, // 117

				TileID.Pearlsand, // 116
				TileID.HallowHardenedSand, // 402
				TileID.HallowSandstone, // 403

				TileID.HallowedIce, // 164
			}),
			// Corruption
			new AnomalyHighlights(0x5F4B0A4, 62, new int[] {
				TileID.CorruptGrass, // 23
				TileID.CorruptPlants, // 24
				TileID.CorruptThorns, // 32
				TileID.Ebonstone, // 25

				TileID.Ebonsand, // 112
				TileID.CorruptHardenedSand, // 398
				TileID.CorruptSandstone, // 400

				TileID.CorruptIce, // 163
			}),
			// Crimson
			new AnomalyHighlights(0x993127, 60, new int[] {
				TileID.FleshGrass, // 199
				TileID.CrimtaneThorns, // 352
				TileID.Crimstone, // 203

				TileID.Crimsand, // 234
				TileID.CrimsonHardenedSand, // 399
				TileID.CrimsonSandstone, // 401

				TileID.FleshIce, // 200
			})
		};

		public override void DrawEffects(int i, int j, int type, SpriteBatch spriteBatch, ref Color drawColor, ref int nextSpecialDrawIndex) {
			DaesPlayer modPlayer = Main.LocalPlayer.GetModPlayer<DaesPlayer>();

			if (modPlayer.hasAnomalyDetector) {
				foreach (AnomalyHighlights anomalyHighlights in Anomalies) {
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

		static public EndlessAmmoType[] EndlessAmmoTypes = {
			new EndlessAmmoType(AmmoID.Bullet, "UI/BulletPreview"),
			new EndlessAmmoType(AmmoID.Arrow, "UI/ArrowPreview"),
			new EndlessAmmoType(AmmoID.Rocket, "UI/RocketPreview"),
			new EndlessAmmoType(AmmoID.Dart, "UI/DartPreview"),
			new EndlessAmmoType(AmmoID.Solution, "UI/SolutionPreview"),
		};

		private void HookPickAmmo(ILContext il) {
			ILLabel UseNormalAmmoLabel = il.DefineLabel();
			ILLabel UseEndlessAmmoLabel = il.DefineLabel();
			ILLabel CanShootLabel = il.DefineLabel();

			// Item item = new Item();
			// bool flag = false;
			ILCursor cursor = new ILCursor(il);
			if (!cursor.TryGotoNext(i => i.MatchLdcI4(0) && i.Next.MatchStloc(1)))
				throw new Exception("Could not locate flag = false");

			cursor.Index += 2;

			// bool useEndlessAmmoFirst = Delegate(this);
			cursor.Emit(Ldarg_0); // Player player
			cursor.EmitDelegate<Func<Player, bool>>((player) => {
				DaesPlayer modPlayer = player.GetModPlayer<DaesPlayer>();
				return modPlayer.useEndlessAmmoFirst;
			});

			cursor.Emit(Stloc_2);

			// if (useEndlessAmmoFirst == false) goto USE_NORMAL_AMMO;
			cursor.Emit(Ldloc_2);
			cursor.Emit(Brfalse, UseNormalAmmoLabel);

			// USE_ENDLESS_AMMO:
			cursor.MarkLabel(UseEndlessAmmoLabel);

			// item = Delegate(this, sItem);
			cursor.Emit(Ldarg_0); // Player player
			cursor.Emit(Ldarg_1); // Item weapon
			cursor.EmitDelegate<Func<Player, Item, Item>>((player, weapon) => {
				DaesPlayer modPlayer = player.GetModPlayer<DaesPlayer>();
				Item playerAmmo = modPlayer.GetItemForEndlessAmmoType(weapon.useAmmo);
				return playerAmmo;
			});
			cursor.Emit(Stloc_0);

			ILLabel IfItemIsAirLabel = il.DefineLabel();
			// if (item.type == 0) goto ITEM_IS_AIR;
			cursor.Emit(Ldloc_0);
			cursor.Emit(Ldfld, typeof(Item).GetField(nameof(Item.type)));
			cursor.Emit(Brfalse, IfItemIsAirLabel);
			// canShoot = true;
			cursor.Emit(Ldarg, 4);
			cursor.Emit(Ldc_I4_1);
			cursor.Emit(Stind_I1);
			// dontConsume = true;
			cursor.Emit(Ldc_I4_1);
			cursor.Emit(Starg, 7);

			// goto CAN_SHOOT;
			cursor.Emit(Br, CanShootLabel);

			// ITEM_IS_AIR:
			cursor.MarkLabel(IfItemIsAirLabel);

			// if (useEndlessAmmoFirst == false) goto CAN_SHOOT;
			cursor.Emit(Ldloc_2);
			cursor.Emit(Brfalse, CanShootLabel);

			// USE_NORMAL_AMMO:
			cursor.MarkLabel(UseNormalAmmoLabel);

			// ..
			if (!cursor.TryGotoNext(i => i.MatchLdarg(4) && i.Next.MatchLdindU1()))
				throw new Exception("Could not locate canShoot == false conditional.");

			// if (useEndlessAmmoFirst) goto CAN_SHOOT;
			ILLabel SkipAheadLabel = il.DefineLabel();
			cursor.Emit(Ldloc_2);
			cursor.Emit(Brtrue, CanShootLabel);

			// if (item.type == 0) goto USE_ENDLESS_AMMO;
			cursor.Emit(Ldloc_0);
			cursor.Emit(Ldfld, typeof(Item).GetField(nameof(Item.type)));
			cursor.Emit(Brfalse, UseEndlessAmmoLabel);

			// CAN_SHOOT:
			cursor.MarkLabel(CanShootLabel);

			// if (!canShoot) return;
			// ...
		}

		private void HookHasAmmo(ILContext il) {
			ILCursor cursor = new ILCursor(il);
			cursor.Emit(Ldarg_0);
			cursor.Emit(Ldarg_1);
			cursor.EmitDelegate<Func<Player, Item, bool>>((player, item) => {
				DaesPlayer modPlayer = player.GetModPlayer<DaesPlayer>();
				return modPlayer.GetItemForEndlessAmmoType(item.useAmmo).type > ItemID.None;
			});

			ILLabel label = il.DefineLabel();
			cursor.Emit(Brfalse, label);
			cursor.Emit(Ldc_I4_1);
			cursor.Emit(Ret);
			cursor.MarkLabel(label);
		}

		private void HookItemSlotDraw(ILContext il) {
			ILCursor cursor = new ILCursor(il);
			if (!cursor.TryGotoNext(i => i.MatchLdcI4(-1) && i.Next.Next.MatchLdarg(2) && i.Next.Next.Next.MatchLdcI4(13)))
				throw new Exception("Could not locate int [x] = -1; if (context == 13) in IL.Terraria.UI.ItemSlot.Draw");

			byte indx = (byte) ((VariableDefinition) cursor.Next.Next.Operand).Index;
			System.Reflection.MethodInfo callTo = typeof(Int32).GetMethod(nameof(Int32.ToString), new Type[] { });

			if (!cursor.TryGotoNext(i => i.MatchLdloca(indx) && i.Next.MatchCall(callTo)))
				throw new Exception("Could not locate call to ChatManager.DrawColorCodedStringWithShadow");

			cursor.Index += 2;
			cursor.Emit(Ldloc_1);
			cursor.EmitDelegate<Func<string, Item, string>>((ammoCount, weapon) => {
				if (ammoCount != "0")
					return ammoCount;

				DaesPlayer modPlayer = Main.LocalPlayer.GetModPlayer<DaesPlayer>();
				Item ammo = modPlayer.GetItemForEndlessAmmoType(weapon.useAmmo);
				return ammo.type > ItemID.None ? "∞" : "0";
			});
		}

		private void HookHasUnityPotion(ILContext il) {
			ILCursor cursor = new ILCursor(il);
			cursor.Emit(Ldarg_0);
			cursor.EmitDelegate<Func<Player, bool>>((player) => {
				DaesPlayer modPlayer = player.GetModPlayer<DaesPlayer>();
				return modPlayer.hasPortableWormhole;
			});

			ILLabel label = il.DefineLabel();
			cursor.Emit(Brfalse, label);
			cursor.Emit(Ldc_I4_1);
			cursor.Emit(Ret);
			cursor.MarkLabel(label);
		}

		private void HookTakeUnityPotion(ILContext il) {
			ILCursor cursor = new ILCursor(il);
			cursor.Emit(Ldarg_0);
			cursor.EmitDelegate<Func<Player, bool>>((player) => {
				DaesPlayer modPlayer = player.GetModPlayer<DaesPlayer>();
				return modPlayer.hasPortableWormhole;
			});

			ILLabel label = il.DefineLabel();
			cursor.Emit(Brfalse, label);
			cursor.Emit(Ret);
			cursor.MarkLabel(label);
		}

		public override void Load() {
			IL.Terraria.Player.HasAmmo += HookHasAmmo;
			IL.Terraria.Player.PickAmmo += HookPickAmmo;
			IL.Terraria.Player.HasUnityPotion += HookHasUnityPotion;
			IL.Terraria.Player.TakeUnityPotion += HookTakeUnityPotion;
			IL.Terraria.UI.ItemSlot.Draw_SpriteBatch_ItemArray_int_int_Vector2_Color += HookItemSlotDraw;

			if (Main.dedServ)
				return;

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
			base.Unload();
		}

		public override void PreSaveAndQuit() {
			ReforgeUI.visible = false;
		}

		// Insert our interface layer
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
					}

					return true;
				},
				InterfaceScaleType.UI
			));
		}

		public override void PostDrawFullscreenMap(ref string mouseText) {
			DaesPlayer modPlayer = Main.LocalPlayer.GetModPlayer<DaesPlayer>();
			if (modPlayer.hasPortableWormhole) {
				PortableWormholeUI.Draw(ref mouseText);
			}
		}
	}
	internal class DaesPlayer : ModPlayer {
		public bool hasPortableWormhole = false;
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
			hasPortableWormhole = false;
			hasAnomalyDetector = false;
		}

		public override void OnEnterWorld(Player player) {
			if (player != Main.LocalPlayer)
				return;

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
	}
}
