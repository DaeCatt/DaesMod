using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI;

namespace DaesMod.UI {
	class Rect {
		public float X = 0;
		public float Y = 0;
		public float Width = 0;
		public float Height = 0;
		public float Right => X + Width;
		public float Bottom => Y + Height;

		public Rect(float x, float y, float width, float height) {
			X = x;
			Y = y;
			Width = width;
			Height = height;
		}

		public Rect(float width, float height) {
			Width = width;
			Height = height;
		}

		public Rect() {

		}

		public void Scale(float xScale, float yScale) {
			Width *= xScale;
			Height *= yScale;
		}

		public void Scale(float scale) {
			Scale(scale, scale);
		}

		public void Translate(float x, float y) {
			X += x;
			Y += y;
		}

		public void Grow(float x, float y) {
			Width += x;
			Height += y;
		}

		public bool Contains(Point point) {
			if (point.X < X)
				return false;
			if (point.Y < Y)
				return false;
			if (point.X > Right)
				return false;
			if (point.Y > Bottom)
				return false;
			return true;
		}

		public bool Contains(Vector2 vector) {
			if (vector.X < X)
				return false;
			if (vector.Y < Y)
				return false;
			if (vector.X > Right)
				return false;
			if (vector.Y > Bottom)
				return false;
			return true;
		}

		public Vector2 Position() {
			return new Vector2(X, Y);
		}

		public Vector2 Dimensions() {
			return new Vector2(Width, Height);
		}

		public Vector2 Center() {
			return new Vector2(X + Width / 2, Y + Height / 2);
		}
	}

	class LargeItemSlot {
		private static readonly int C = 8;
		private static readonly int S = 52;

		private static readonly int N = S - C;
		private static readonly int I = S - C * 2;

		public static Rectangle LTRect = new Rectangle(0, 0, C, C);
		public static Rectangle MTRect = new Rectangle(C, 0, I, C);
		public static Rectangle RTRect = new Rectangle(N, 0, C, C);

		public static Rectangle LMRect = new Rectangle(0, C, C, I);
		public static Rectangle MMRect = new Rectangle(C, C, I, I);
		public static Rectangle RMRect = new Rectangle(N, C, C, I);

		public static Rectangle LBRect = new Rectangle(0, N, C, C);
		public static Rectangle MNRect = new Rectangle(C, N, I, C);
		public static Rectangle RBRect = new Rectangle(N, N, C, C);

		public static void DrawPanel(SpriteBatch spriteBatch, Rect rect, float detailScale = 1) {
			Texture2D texture = Main.inventoryBackTexture;
			Color color = Main.inventoryBack;

			float cs = detailScale * C;

			float w = rect.Width - cs * 2;
			float h = rect.Height - cs * 2;

			float x0 = rect.X;
			float x1 = x0 + cs;
			float x2 = x1 + w;

			float y0 = rect.Y;
			float y1 = y0 + cs;
			float y2 = y1 + h;

			float sc = detailScale;
			float sx = w / I;
			float sy = h / I;

			spriteBatch.Draw(texture, new Vector2(x0, y0), LTRect, color, 0f, Vector2.Zero, new Vector2(sc, sc), SpriteEffects.None, 0);
			spriteBatch.Draw(texture, new Vector2(x1, y0), MTRect, color, 0f, Vector2.Zero, new Vector2(sx, sc), SpriteEffects.None, 0);
			spriteBatch.Draw(texture, new Vector2(x2, y0), RTRect, color, 0f, Vector2.Zero, new Vector2(sc, sc), SpriteEffects.None, 0);

			spriteBatch.Draw(texture, new Vector2(x0, y1), LMRect, color, 0f, Vector2.Zero, new Vector2(sc, sy), SpriteEffects.None, 0);
			spriteBatch.Draw(texture, new Vector2(x1, y1), MMRect, color, 0f, Vector2.Zero, new Vector2(sx, sy), SpriteEffects.None, 0);
			spriteBatch.Draw(texture, new Vector2(x2, y1), RMRect, color, 0f, Vector2.Zero, new Vector2(sc, sy), SpriteEffects.None, 0);

			spriteBatch.Draw(texture, new Vector2(x0, y2), LBRect, color, 0f, Vector2.Zero, new Vector2(sc, sc), SpriteEffects.None, 0);
			spriteBatch.Draw(texture, new Vector2(x1, y2), MNRect, color, 0f, Vector2.Zero, new Vector2(sx, sc), SpriteEffects.None, 0);
			spriteBatch.Draw(texture, new Vector2(x2, y2), RBRect, color, 0f, Vector2.Zero, new Vector2(sc, sc), SpriteEffects.None, 0);
		}

		public static void DrawItem(SpriteBatch spriteBatch, Rect rect, Item item, float scale = 1) {
			Texture2D itemTexture = Main.itemTexture[item.type];
			Rectangle itemSourceRect = (Main.itemAnimations[item.type] == null) ? itemTexture.Frame() : Main.itemAnimations[item.type].GetFrame(itemTexture);

			Color color = Main.inventoryBack;
			Color currentColor = Main.inventoryBack;

			float secondaryScale = 1f;
			ItemSlot.GetItemLight(ref currentColor, ref secondaryScale, item);

			float itemScale = 1f;
			if (itemSourceRect.Width > 32 || itemSourceRect.Height > 32) {
				itemScale = (itemSourceRect.Width <= itemSourceRect.Height) ? (32f / itemSourceRect.Height) : (32f / itemSourceRect.Width);
			}

			itemScale *= scale;

			Vector2 itemPosition = rect.Center() - itemSourceRect.Size() * itemScale / 2f;
			Vector2 origin = itemSourceRect.Size() * (secondaryScale / 2f - 0.5f);

			float finalScale = itemScale * secondaryScale;

			if (ItemLoader.PreDrawInInventory(item, spriteBatch, itemPosition, itemSourceRect, item.GetAlpha(currentColor), item.GetColor(color), origin, finalScale)) {
				spriteBatch.Draw(itemTexture, itemPosition, itemSourceRect, item.GetAlpha(currentColor), 0f, origin, finalScale, SpriteEffects.None, 0f);
				if (item.color != Color.Transparent) {
					spriteBatch.Draw(itemTexture, itemPosition, itemSourceRect, item.GetColor(color), 0f, origin, finalScale, SpriteEffects.None, 0f);
				}
			}

			ItemLoader.PostDrawInInventory(item, spriteBatch, itemPosition, itemSourceRect, item.GetAlpha(currentColor), item.GetColor(color), origin, finalScale);
		}

		public static void DrawItemSlot(SpriteBatch spriteBatch, Rect rect, Item item, float scale = 1) {
			DrawPanel(spriteBatch, rect, scale);
			DrawItem(spriteBatch, rect, item, scale);
		}
	}
}
