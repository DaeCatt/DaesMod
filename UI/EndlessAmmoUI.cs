using DaesMod.Data;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.UI;
using Terraria.UI.Chat;

namespace DaesMod.UI {
	class EndlessAmmoUI : UIState {
		public static readonly int UNLOCK_AMOUNT = 3996;
		public static int AmmoPicker = AmmoID.None;
		public static int AmmoPickerScroll = 0;

		public Color PreviewColor = new Color(255 / 2, 255 / 2, 255 / 2, 255 / 2);
		private Color UnlockColor = new Color(255, 215, 0);

		private static readonly int DeltaX = 534 - 497;
		private static readonly int LeftX = 534 + DeltaX;

		private void DrawAmmoPicker(SpriteBatch spriteBatch, DaesPlayer modPlayer) {
			int dx = LeftX;
			Color titleColor = new Color(Main.mouseTextColor, Main.mouseTextColor, Main.mouseTextColor, Main.mouseTextColor);
			Point mousePoint = new Point(Main.mouseX, Main.mouseY);

			Main.LocalPlayer.mouseInterface = true;
			Color locked = new Color(187, 187, 187, 187);

			float stringScale = 0.75f;
			float gap = 8;

			List<Item> ammos = new List<Item>();
			float widestAmmoName = Main.fontMouseText.MeasureString($"{UNLOCK_AMOUNT} / {UNLOCK_AMOUNT}").X;
			float stringHeight = 0;

			foreach (Item ammoItem in DataContainer.AmmoItems) {
				if (ammoItem.ammo == AmmoPicker) {
					ammos.Add(ammoItem);
					Vector2 stringDimensions = Main.fontMouseText.MeasureString(ammoItem.Name);
					if (stringDimensions.X > widestAmmoName)
						widestAmmoName = stringDimensions.X;
					if (stringDimensions.Y > stringHeight)
						stringHeight = stringDimensions.Y;
				}
			}

			Vector2 stringLeftAlignedVertCenter = new Vector2(0, stringHeight / 2);

			widestAmmoName *= stringScale;
			stringHeight *= stringScale;

			float lineHeight = stringHeight * 0.6f;

			foreach (EndlessAmmoType ammoType in DataContainer.EndlessAmmoTypes) {
				if (ammoType.Type == AmmoPicker) {
					ChatManager.DrawColorCodedString(spriteBatch, Main.fontMouseText, ammoType.Title, new Vector2(dx, 84f), titleColor, 0, Vector2.Zero, Vector2.One * 0.75f);
					break;
				}
			}

			float unit = 56 * Main.inventoryScale;
			Rect dropdownRect = new Rect(dx, 105f, unit + widestAmmoName + gap, Main.inventoryScale * (52 + (56 * (ammos.Count - 1)))); // Math.Min(3, ammos.Count)

			LargeItemSlot.DrawPanel(spriteBatch, dropdownRect, Main.inventoryScale);

			Rect itemRect = new Rect(dx, 105, 52, 52);
			itemRect.Scale(Main.inventoryScale);

			Rect textRect = new Rect(dx, 105, unit + widestAmmoName, 52);
			textRect.Scale(1, Main.inventoryScale);

			Item hoveringAmmo = new Item();
			int action = 0;
			int hoveringCount = 0;

			for (int i = 0; i < ammos.Count; i++) {
				Item ammo = ammos[i];
				LargeItemSlot.DrawItem(spriteBatch, itemRect, ammo, Main.inventoryScale);

				Vector2 position = itemRect.Position();
				position.X += unit;
				position.Y += unit / 2;

				int count = 0;
				bool unlocked = modPlayer.HasEndlessAmmoItemUnlocked(ammo.type);
				bool canUnlock = false;

				if (unlocked) {
					ChatManager.DrawColorCodedStringWithShadow(
						spriteBatch,
						Main.fontMouseText,
						ammo.Name,
						position,
						Color.White,
						0f,
						stringLeftAlignedVertCenter,
						Vector2.One * stringScale
					);
				} else {
					count = Math.Min(UNLOCK_AMOUNT, modPlayer.CountItemsInInventory(ammo.type));
					canUnlock = count == UNLOCK_AMOUNT;

					position.Y -= lineHeight / 2;
					ChatManager.DrawColorCodedStringWithShadow(
						spriteBatch,
						Main.fontMouseText,
						ammo.Name,
						position,
						canUnlock ? UnlockColor : locked,
						0f,
						stringLeftAlignedVertCenter,
						Vector2.One * stringScale
					);

					position.Y += lineHeight;
					ChatManager.DrawColorCodedStringWithShadow(
						spriteBatch,
						Main.fontMouseText,
						$"{count} / {UNLOCK_AMOUNT}",
						position,
						canUnlock ? Color.White : locked,
						0f,
						stringLeftAlignedVertCenter,
						Vector2.One * stringScale
					);
				}

				if (itemRect.Contains(mousePoint)) {
					hoveringAmmo = ammo;
				} else if (textRect.Contains(mousePoint)) {
					hoveringCount = count;
					hoveringAmmo = ammo;

					if (unlocked) {
						action = 1;
					} else if (canUnlock) {
						action = 2;
					} else {
						action = 3;
					}
				}

				itemRect.Translate(0, unit);
				textRect.Translate(0, unit);
			}

			if (hoveringAmmo.type != ItemID.None) {
				switch (action) {
					case 0:
						Main.HoverItem = hoveringAmmo.Clone();
						Main.HoverItem.ammo = 0;
						Main.HoverItem.material = false;
						Main.HoverItem.consumable = false;
						Main.instance.MouseText(hoveringAmmo.Name, hoveringAmmo.rare, 0);
						break;
					case 1:
						Main.instance.MouseText(string.Format("Select {0}", hoveringAmmo.Name));
						if (Main.mouseLeft && Main.mouseLeftRelease) {
							modPlayer.SelectUnlockedAmmo(hoveringAmmo.type);
							Main.PlaySound(SoundID.Grab);
							AmmoPicker = AmmoID.None;
						}
						break;
					case 2:
						Main.instance.MouseText(string.Format("Unlock {0}", hoveringAmmo.Name));
						if (Main.mouseLeft && Main.mouseLeftRelease) {
							if (modPlayer.UnlockEndlessAmmo(hoveringAmmo.type)) {
								Main.PlaySound(SoundID.Grab);
							}

							AmmoPicker = AmmoID.None;
						}
						break;
					case 3:
						Main.instance.MouseText(string.Format("Unlock {0} ({1} / {2})", hoveringAmmo.Name, hoveringCount, UNLOCK_AMOUNT));
						break;
				}
			}

			if (!dropdownRect.Contains(mousePoint) && Main.mouseLeft && Main.mouseLeftRelease) {
				Main.PlaySound(SoundID.MenuClose);
				AmmoPicker = AmmoID.None;
			}
		}
		
		public override void Draw(SpriteBatch spriteBatch) {
			DaesPlayer modPlayer = Main.LocalPlayer.GetModPlayer<DaesPlayer>();
			if (!modPlayer.hasEndlessAmmo) {
				base.Draw(spriteBatch);
				return;
			}

			Main.inventoryScale = 0.6f;
			
			if (AmmoPicker != AmmoID.None) {
				DrawAmmoPicker(spriteBatch, modPlayer);
				base.Draw(spriteBatch);
				return;
			} else {
				AmmoPickerScroll = 0;
			}

			int dx = LeftX;
			Color titleColor = new Color(Main.mouseTextColor, Main.mouseTextColor, Main.mouseTextColor, Main.mouseTextColor);
			Point mousePoint = new Point(Main.mouseX, Main.mouseY);

			Rect checkboxRect = new Rect(dx, 84f, 16, 16);
			LargeItemSlot.DrawPanel(spriteBatch, checkboxRect, Main.inventoryScale);

			string EndlessAmmoLabel = "∞ Ammo";
			ChatManager.DrawColorCodedString(spriteBatch, Main.fontMouseText, EndlessAmmoLabel, new Vector2(dx + 20, 84f), titleColor, 0, Vector2.Zero, Vector2.One * 0.75f);

			Rect slotRect = new Rect(52, 52);
			slotRect.Scale(Main.inventoryScale);

			int hoverSlot = -1;
			int hoverAmmoType = 0;
			Item hoverAmmo = new Item();
			for (int i = 0; i < DataContainer.EndlessAmmoTypes.Length; i++) {
				int x = i / 4;
				int y = i % 4;

				slotRect.X = dx + x * DeltaX;
				slotRect.Y = (int) (105f + y * 56 * Main.inventoryScale);

				EndlessAmmoType AmmoType = DataContainer.EndlessAmmoTypes[i];
				Item ammo = modPlayer.GetItemForEndlessAmmoType(AmmoType.Type);

				LargeItemSlot.DrawPanel(spriteBatch, slotRect, Main.inventoryScale);
				if (ammo.type != ItemID.None) {
					LargeItemSlot.DrawItem(spriteBatch, slotRect, ammo, Main.inventoryScale);
				} else {
					Texture2D texture = AmmoType.Texture;
					Vector2 position = slotRect.Center() - texture.Size() * Main.inventoryScale / 2f;
					spriteBatch.Draw(texture, position, texture.Frame(), PreviewColor, 0f, Vector2.Zero, Main.inventoryScale, SpriteEffects.None, 0f);
				}

				if (modPlayer.CanUnlockAmmoForType(AmmoType.Type)) {
					Vector2 hPosition = slotRect.Position();
					hPosition.X += 24;
					hPosition.Y += 4;
					ChatManager.DrawColorCodedStringWithShadow(
						spriteBatch,
						Main.fontMouseText,
						"!",
						hPosition,
						UnlockColor,
						0f,
						Vector2.Zero,
						Vector2.One * 0.6f
					);
				}

				if (AmmoPicker == AmmoID.None && slotRect.Contains(mousePoint)) {
					hoverSlot = i;
					hoverAmmo = ammo;
					hoverAmmoType = AmmoType.Type;
				}
			}

			if (hoverSlot > -1) {
				Main.LocalPlayer.mouseInterface = true;
				if (hoverAmmo.type != ItemID.None) {
					Main.HoverItem = hoverAmmo.Clone();
					Main.HoverItem.ammo = 0;
					Main.HoverItem.material = false;
					Main.HoverItem.consumable = false;
					Main.instance.MouseText(hoverAmmo.Name, hoverAmmo.rare, 0);
				} else {
					Main.instance.MouseText(DataContainer.EndlessAmmoTypes[hoverSlot].NoSelected);
				}

				if (Main.mouseLeft && Main.mouseLeftRelease) {
					Main.PlaySound(SoundID.MenuOpen);
					AmmoPicker = hoverAmmoType;
				}
			}

			if (checkboxRect.Contains(mousePoint)) {
				Main.LocalPlayer.mouseInterface = true;
				Main.instance.MouseText("Use endless ammo first.");

				if (Main.mouseLeft && Main.mouseLeftRelease) {
					Main.PlaySound(SoundID.MenuTick);
					modPlayer.useEndlessAmmoFirst = !modPlayer.useEndlessAmmoFirst;
				}
			}

			if (modPlayer.useEndlessAmmoFirst) {
				Vector2 position = checkboxRect.Center();
				position.X += 2;
				position.Y -= 2;
				ChatManager.DrawColorCodedStringWithShadow(spriteBatch, Main.fontMouseText, "✓", position, Color.White, 0f, checkboxRect.Dimensions() / 2, Vector2.One * Main.inventoryScale);
			}

			base.Draw(spriteBatch);
		}
	}
}
