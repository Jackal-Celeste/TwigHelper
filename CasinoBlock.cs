using Celeste.Mod.UI;
using FMOD.Studio;
using Microsoft.Xna.Framework;
using Monocle;
using Celeste;
using System;
using System.Collections;
using Celeste.Mod.Entities;
using System.Collections.Generic;
using System.Reflection;
using Celeste.Mod;


namespace Celeste.Mod.TwigHelper.Entities
{
    [Tracked]
    [CustomEntity("Thomasdb05Helper/SlotMachineBlock")]
	public class CasinoBlock : Solid
	{
		private enum States
		{
			Waiting,
			WindingUp,
			Bouncing,
			BounceEnd,
			Broken
		}

		public static ParticleType P_Reform;

		public static ParticleType P_FireBreak;

		public static ParticleType P_IceBreak;

		private const float WindUpDelay = 0f;

		private const float WindUpDist = 10f;

		private const float IceWindUpDist = 16f;

		private const float BounceDist = 24f;

		private const float LiftSpeedXMult = 0.75f;

		private const float RespawnTime = 1.6f;

		private const float WallPushTime = 0.1f;

		private const float BounceEndTime = 0.05f;

		private Vector2 bounceDir;

		private States state;

		private Vector2 startPos;

		private float moveSpeed;

		private float windUpStartTimer;

		private float windUpProgress;

		private float respawnTimer;

		private float bounceEndTimer;

		private Vector2 bounceLift;

		private float reappearFlash;

		private bool reformed;

		private Vector2 debrisDirection;

		private List<Image> hotImages;

		private List<Image> coldImages;

		private Sprite hotCenterSprite;

		private Sprite coldCenterSprite;

		public CasinoBlock(Vector2 position, float width, float height) : base(position, width, height, true)
		{
			reformed = true;
			state = States.Waiting;
			startPos = Position;
			hotImages = BuildSprite(GFX.Game["objects/bumpblocknew/fire00"]);
			hotCenterSprite = GFX.SpriteBank.Create("bumpBlockCenterFire");
			hotCenterSprite.Position = new Vector2(base.Width, base.Height) / 2f;
			hotCenterSprite.Visible = false;
			Add(hotCenterSprite);
			coldImages = BuildSprite(GFX.Game["objects/bumpblocknew/ice00"]);
			coldCenterSprite = GFX.SpriteBank.Create("bumpBlockCenterIce");
			coldCenterSprite.Position = new Vector2(base.Width, base.Height) / 2f;
			coldCenterSprite.Visible = false;
			Add(coldCenterSprite);
		}

		public CasinoBlock(EntityData data, Vector2 offset) : this(data.Position + offset, data.Width, data.Height)
		{
			
		}

		private List<Image> BuildSprite(MTexture source)
		{
			List<Image> list = new List<Image>();
			int num = source.Width / 8;
			int num2 = source.Height / 8;
			for (int i = 0; (float)i < base.Width; i += 8)
			{
				for (int j = 0; (float)j < base.Height; j += 8)
				{
					int num3 = ((i != 0) ? ((!((float)i >= base.Width - 8f)) ? Calc.Random.Next(1, num - 1) : (num - 1)) : 0);
					int num4 = ((j != 0) ? ((!((float)j >= base.Height - 8f)) ? Calc.Random.Next(1, num2 - 1) : (num2 - 1)) : 0);
					Image image = new Image(source.GetSubtexture(num3 * 8, num4 * 8, 8, 8));
					image.Position = new Vector2(i, j);
					list.Add(image);
					Add(image);
				}
			}
			return list;
		}

		private void ToggleSprite()
		{
			hotCenterSprite.Visible = true;
			coldCenterSprite.Visible = false;
			foreach (Image hotImage in hotImages)
			{
				hotImage.Visible = true;
			}
			foreach (Image coldImage in coldImages)
			{
				coldImage.Visible = false;
			}
		}

		public override void Added(Scene scene)
		{
			base.Added(scene);
			ToggleSprite();
		}


		public override void Render()
		{

				base.Render();
		}

			
	}
}


	

	



