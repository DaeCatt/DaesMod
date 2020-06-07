using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace DaesMod.Items {
	class EndlessAmmoInventory : ModItem {
		public override void SetDefaults() {
			item.maxStack = 1;
			item.consumable = true;
			item.value = 0;
			item.width = 14;
			item.height = 14;
			item.useStyle = ItemUseStyleID.HoldingUp; // Like a life crystal
			item.rare = ItemRarityID.LightRed; // LightRed, early hardmode items
		}

		public override bool CanUseItem(Player player) {
			DaesPlayer modPlayer = player.GetModPlayer<DaesPlayer>();
			return !modPlayer.hasEndlessAmmo;
		}

		public override bool UseItem(Player player) {
			DaesPlayer modPlayer = player.GetModPlayer<DaesPlayer>();
			if (modPlayer.hasEndlessAmmo)
				return false;

			modPlayer.UnlockEndlessAmmo();
			return true;
		}

		public override void AddRecipes() {
			ModRecipe recipe = new ModRecipe(mod);
			recipe.AddIngredient(ItemID.EndlessMusketPouch);
			recipe.AddIngredient(ItemID.EndlessQuiver);
			recipe.AddTile(TileID.TinkerersWorkbench);
			recipe.AddTile(TileID.CrystalBall);
			recipe.SetResult(this);
			recipe.AddRecipe();
		}
	}
}
