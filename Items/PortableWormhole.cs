using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace DaesMod.Items {
	public class PortableWormhole : ModItem {
		public override void SetDefaults() {
			item.CloneDefaults(ItemID.WormholePotion);
			item.maxStack = 1;
			item.consumable = false;
			item.value = Item.buyPrice(4, 19, 0, 0);
			item.width = 14;
			item.height = 14;
			item.useStyle = ItemUseStyleID.HoldingUp; // Like a life crystal
			item.rare = ItemRarityID.Pink; // Pink, Pre-Plantera items
		}

		public override void UpdateInventory(Player player) {
			DaesPlayer modPlayer = player.GetModPlayer<DaesPlayer>();
			modPlayer.hasPortableWormhole = true;
		}
	}
}
