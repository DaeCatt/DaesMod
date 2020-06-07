using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.Localization;

namespace DaesMod.UI {
	class PortableWormholeUI {
		static public void Draw(ref string mouseText) {
			if (Main.netMode == NetmodeID.MultiplayerClient && Main.LocalPlayer.team > 0 && Main.instance.unityMouseOver)
				return;

			float scale = Main.mapFullscreenScale / 16f;
			float dx = Main.screenWidth / 2 - Main.mapFullscreenPos.X * Main.mapFullscreenScale;
			float dy = Main.screenHeight / 2 - Main.mapFullscreenPos.Y * Main.mapFullscreenScale;

			for (int i = 0; i < Main.npc.Length; i++) {
				// Only check active NPCs that are set to townNPC.
				if (!Main.npc[i].active || !Main.npc[i].townNPC)
					continue;

				int headIndex = NPC.TypeToHeadIndex(Main.npc[i].type);
				if (headIndex <= 0)
					continue;

				Texture2D headTexture = Main.npcHeadTexture[headIndex];

				float x = dx + scale * (Main.npc[i].position.X + Main.npc[i].width / 2);
				float y = dy + scale * (Main.npc[i].position.Y + Main.npc[i].gfxOffY + Main.npc[i].height / 2);

				float minX = x - headTexture.Width / 2 * Main.UIScale;
				float minY = y - headTexture.Height / 2 * Main.UIScale;
				float maxX = minX + headTexture.Width * Main.UIScale;
				float maxY = minY + headTexture.Height * Main.UIScale;

				if (Main.mouseX >= minX && Main.mouseX <= maxX && Main.mouseY >= minY && Main.mouseY <= maxY) {
					SpriteEffects effect = SpriteEffects.None;
					if (Main.npc[i].direction > 0) {
						effect = SpriteEffects.FlipHorizontally;
					}

					Main.spriteBatch.Draw(headTexture, new Vector2(x, y), headTexture.Frame(), Color.White, 0f, headTexture.Frame().Size() / 2, Main.UIScale + 0.5f, effect, 0f);

					if (!Main.instance.unityMouseOver)
						Main.PlaySound(SoundID.MenuTick);

					Main.instance.unityMouseOver = true;

					if (Main.mouseLeft && Main.mouseLeftRelease) {
						Main.mouseLeftRelease = false;
						Main.mapFullscreen = false;

						Main.NewText(Language.GetTextValue("Game.HasTeleportedTo", Main.player[Main.myPlayer].name, Main.npc[i].FullName), 255, 255, 0);
						Main.player[Main.myPlayer].Teleport(Main.npc[i].position);
						return;
					}

					mouseText = Language.GetTextValue("Game.TeleportTo", Main.npc[i].FullName);
					return;
				}
			}
		}
	}
}
