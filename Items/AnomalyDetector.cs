using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace DaesMod.Items {
	public class AnomalyDetector : ModItem {
		public override void SetDefaults() {
			item.CloneDefaults(ItemID.MetalDetector);
			item.maxStack = 1;
			item.consumable = false;
			item.value = Item.buyPrice(1, 0, 0, 0);
			item.width = 16;
			item.height = 16;
			item.useStyle = ItemUseStyleID.HoldingUp; // Like a life crystal
			item.rare = ItemRarityID.Pink; // Pink, Pre-Plantera items
		}

		public override void UpdateInventory(Player player) {
			DaesPlayer modPlayer = player.GetModPlayer<DaesPlayer>();
			modPlayer.hasAnomalyDetector = true;
		}

		public override void UpdateAccessory(Player player, bool hideVisual) {
			DaesPlayer modPlayer = player.GetModPlayer<DaesPlayer>();
			modPlayer.hasAnomalyDetector = true;
		}
	}
}
