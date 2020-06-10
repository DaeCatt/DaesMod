using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace DaesMod.Data {
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
		public string Title;
		public string NoSelected;
		public Texture2D Texture;
		public EndlessAmmoType(string title, string noSelected, int type, string path) {
			Title = title;
			NoSelected = noSelected;
			Type = type;
			Path = path;
			Texture = null;
		}
	}

	class DataContainer {
		public static readonly AnomalyHighlights[] Anomalies = {
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

		static public List<Item> AmmoItems = new List<Item>();
		static public EndlessAmmoType[] EndlessAmmoTypes = {
			new EndlessAmmoType("Bullets", "No bullet selected.", AmmoID.Bullet, "UI/BulletPreview"),
			new EndlessAmmoType("Arrows", "No arrow selected.", AmmoID.Arrow, "UI/ArrowPreview"),
			new EndlessAmmoType("Rockets", "No rocket selected.", AmmoID.Rocket, "UI/RocketPreview"),
			new EndlessAmmoType("Darts", "No dart selected.", AmmoID.Dart, "UI/DartPreview"),
			new EndlessAmmoType("Solutions", "No solution selected.",AmmoID.Solution, "UI/SolutionPreview"),
		};

		public static void Load() {
			for (int i = 0; i < EndlessAmmoTypes.Length; i++) {
				if (EndlessAmmoTypes[i].Texture != null)
					continue;

				EndlessAmmoTypes[i].Texture = GetTexture(nameof(DaesMod) + "/" + EndlessAmmoTypes[i].Path);
			}
		}

		public static void LoadItems() {
			AmmoItems.Clear();

			List<int> AmmoIds = new List<int>();
			foreach (EndlessAmmoType EndlessAmmoType in EndlessAmmoTypes)
				AmmoIds.Add(EndlessAmmoType.Type);

			int i = 1;
			Item item = new Item();
			for (; i < ItemID.Count; i++) {
				item.SetDefaults(i);
				if (item.consumable && AmmoIds.Contains(item.ammo))
					AmmoItems.Add(item.Clone());
			}

			for (; i < ItemLoader.ItemCount; i++) {
				ModItem modItem = ItemLoader.GetItem(i);
				if (modItem.item.consumable && AmmoIds.Contains(modItem.item.ammo))
					AmmoItems.Add(modItem.item.Clone());
			}
		}

		public static void Unload() {
			AmmoItems.Clear();
		}
	}
}
