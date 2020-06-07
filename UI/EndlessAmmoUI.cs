using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.UI;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;

namespace DaesMod.UI {
	class EndlessAmmoUI : UIState {
		public Texture2D[] EndlessAmmoPreviewTextures = new Texture2D[DaesMod.EndlessAmmoTypes.Length];
		public Color PreviewColor = new Color(255 / 2, 255 / 2, 255 / 2, 255 / 2);

		public override void OnInitialize() {
			for (int i = 0; i < DaesMod.EndlessAmmoTypes.Length; i++)
				EndlessAmmoPreviewTextures[i] = GetTexture(nameof(DaesMod) + "/" + DaesMod.EndlessAmmoTypes[i].Path);
		}

		public override void Draw(SpriteBatch spriteBatch) {
			DaesPlayer modPlayer = Main.LocalPlayer.GetModPlayer<DaesPlayer>();
			if (!modPlayer.hasEndlessAmmo) {
				base.Draw(spriteBatch);
				return;
			}

			Main.inventoryScale = 0.6f;

			float width = 56 * Main.inventoryScale * ((3 + DaesMod.EndlessAmmoTypes.Length) / 4);

			string EndlessAmmoLabel = "∞ Ammo";
			Vector2 vector = Main.fontMouseText.MeasureString(EndlessAmmoLabel);
			float scale = vector.X > width ? width / vector.X : 1;
			ReLogic.Graphics.DynamicSpriteFontExtensionMethods.DrawString(
				spriteBatch,
				Main.fontMouseText,
				EndlessAmmoLabel,
				new Vector2(571f + width / 2f - vector.X / 2f, 84f),
				new Color(Main.mouseTextColor, Main.mouseTextColor, Main.mouseTextColor, Main.mouseTextColor), 0f, default, 0.75f * scale, SpriteEffects.None, 0f);

			int x = 0;
			int y = 0;
			Vector2 itemSlotSize = Main.inventoryBackTexture.Size() * Main.inventoryScale;

			for (int i = 0; i < DaesMod.EndlessAmmoTypes.Length; i++) {
				EndlessAmmoType AmmoType = DaesMod.EndlessAmmoTypes[i];
				Item ammo = modPlayer.GetItemForEndlessAmmoType(AmmoType.Type);

				Vector2 position = new Vector2(571f + (x * 56) * Main.inventoryScale, 85f + (y * 56) * Main.inventoryScale + 20f);
				ItemSlot.Draw(spriteBatch, ref ammo, ItemSlot.Context.InventoryAmmo, position);
				if (ammo.type == ItemID.None) {
					Texture2D texture = EndlessAmmoPreviewTextures[i];
					position += itemSlotSize / 2 - texture.Size() * Main.inventoryScale / 2f;
					spriteBatch.Draw(texture, position, texture.Frame(), PreviewColor, 0f, Vector2.Zero, Main.inventoryScale, SpriteEffects.None, 0f);
				}

				if (++y > 3) {
					y = 0;
					x++;
				}
			}


			base.Draw(spriteBatch);
		}
	}
}
