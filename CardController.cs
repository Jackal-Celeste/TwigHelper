using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.TwigHelper.Entities
{
	[Tracked]
	[CustomEntity("TwigHelper/CardController")]
	public class CardController : Solid
	{
		public static ParticleType P_Smash;

		public static ParticleType P_Sparks;

		private Sprite sprite;

		private SineWave sine;

		private Vector2 start;

		private float sink;

		private bool rolling;

		private bool flag;

		private float shakeCounter;

		private Vector2 bounceDir;

		private Wiggler bounce;

		private bool makeSparks;

		private bool smashParticles;

		private Coroutine pulseRoutine;

		private SoundSource firstHitSfx;

		public int[] ids;

		public float roll;

		public int totalTime;

		public Image face;

		public int thisRoll;

		public String rollFlag;

		/* 
		 * Spades = 0, Corner Boosts
		 * Clubs = 1, Various Tech
		 * Hearts = 2, Ultras
		 * Diamonds = 3, Bunny Hops
		 * 
		 * Intro, 1: Spades, Diamonds
		 * 2, 7a, 7b: Spades Diamonds, Hearts
		 * 3, 4, 5, 8a, 8b, 9a, 9b, 9c, 10a, 10b, 10c: All
		 * 6a, 6b: Spades, Hearts
		 * Outro: Hearts
		*/





		public CardController(Vector2 position, int rollIndex, string flag)
			: base(position, 24f, 24f, safe: true)
		{
			this.rollFlag = flag;
			SurfaceSoundIndex = 32;
			start = Position;
			sprite = TwigModule.spriteBank.Create("cardDealer");
			sprite.Position += new Vector2(-8f, -8f);
			rolling = true;
			Sprite obj = sprite;
			obj.OnLastFrame = (Action<string>)Delegate.Combine(obj.OnLastFrame, (Action<string>)delegate (string anim)
			{

				if (anim != "idle")
				{
					makeSparks = true;
				}
			});
			sprite.Position = new Vector2(base.Width, base.Height) / 2f;
			Add(sprite);
			sine = new SineWave(0.5f, 0f);
			Add(sine);
			bounce = Wiggler.Create(1f, 0.5f);
			bounce.StartZero = false;
			Add(bounce);
			OnDashCollide = Dashed;
			switch (rollIndex)
			{
				case 0:
					ids = new int[1];
					ids[0] = 2;
					break;
				case 1:
					ids = new int[3];
					ids[0] = 0;
					ids[1] = 2;
					ids[2] = 3;
					break;
				case 2:
					ids = new int[4];
					ids[0] = 0;
					ids[1] = 1;
					ids[2] = 2;
					ids[3] = 3;
					break;
				case 3:
					ids = new int[2];
					ids[0] = 0;
					ids[1] = 2;
					break;
				default:
					ids = new int[1];
					ids[0] = 2;
					break;
			}
		}

		public CardController(EntityData e, Vector2 levelOffset)
			: this(e.Position + levelOffset, e.Int("rollIndex"), e.Attr("flag", defaultValue:"1"))
		{
		}


		public DashCollisionResults Dashed(Player player, Vector2 dir)
		{
			if (rolling)
			{
				(base.Scene as Level).DirectionalShake(dir);
				sprite.Scale = new Vector2(1f + Math.Abs(dir.Y) * 0.4f - Math.Abs(dir.X) * 0.4f, 1f + Math.Abs(dir.X) * 0.4f - Math.Abs(dir.Y) * 0.4f);
				Add(firstHitSfx = new SoundSource("event:/game/07_summit/checkpoint_confetti"));
				Celeste.Freeze(0.1f);
				shakeCounter = 0.2f;
				bounceDir = dir;
				bounce.Start();
				smashParticles = true;
				Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);

				rolling = false;
				player.RefillDash();
				player.RefillStamina();
				thisRoll = (int)roll;
                switch (thisRoll)
                {
					case 0:
						sprite.Play("spade");
						(Scene as Level).Session.SetFlag("spade" + rollFlag, true);
						break;
					case 1:
						sprite.Play("club");
						(Scene as Level).Session.SetFlag("club" + rollFlag, true);
						break;
					case 2:
						sprite.Play("heart");
						(Scene as Level).Session.SetFlag("heart" + rollFlag, true);
						break;
					case 3:
						sprite.Play("diamond");
						(Scene as Level).Session.SetFlag("diamond" + rollFlag, true);
						break;
					default:
						sprite.Play("idle");
						break;

                }

				return DashCollisionResults.Rebound;
			}
			return DashCollisionResults.NormalCollision;
		}

		private void SmashParticles(Vector2 dir)
		{
			float direction;
			Vector2 position;
			Vector2 positionRange;
			int num;
			if (dir == Vector2.UnitX)
			{
				direction = 0f;
				position = base.CenterRight - Vector2.UnitX * 12f;
				positionRange = Vector2.UnitY * (base.Height - 6f) * 0.5f;
				num = (int)(base.Height / 8f) * 4;
			}
			else if (dir == -Vector2.UnitX)
			{
				direction = (float)Math.PI;
				position = base.CenterLeft + Vector2.UnitX * 12f;
				positionRange = Vector2.UnitY * (base.Height - 6f) * 0.5f;
				num = (int)(base.Height / 8f) * 4;
			}
			else if (dir == Vector2.UnitY)
			{
				direction = (float)Math.PI / 2f;
				position = base.BottomCenter - Vector2.UnitY * 12f;
				positionRange = Vector2.UnitX * (base.Width - 6f) * 0.5f;
				num = (int)(base.Width / 8f) * 4;
			}
			else
			{
				direction = -(float)Math.PI / 2f;
				position = base.TopCenter + Vector2.UnitY * 12f;
				positionRange = Vector2.UnitX * (base.Width - 6f) * 0.5f;
				num = (int)(base.Width / 8f) * 4;
			}
			num += 2;
			//SceneAs<Level>().Particles.Emit(P_Smash, num, position, positionRange, direction);
		}

		public override void Added(Scene scene)
		{
			base.Added(scene);
			if (flag && (base.Scene as Level).Session.GetFlag("disable_lightning"))
			{
				RemoveSelf();
			}
		}

		public override void Update()
		{
			totalTime += 1;
			Random rand = new Random(totalTime);
			roll = (float)rand.NextDouble() * ids.Length;

			base.Update();
			if (makeSparks && base.Scene.OnInterval(0.03f))
			{
				//SceneAs<Level>().ParticlesFG.Emit(P_Sparks, 1, base.Center, Vector2.One * 12f);
			}
			if (Collidable)
			{
				bool flag = HasPlayerRider();
				sink = Calc.Approach(sink, flag ? 1 : 0, 2f * Engine.DeltaTime);
				sine.Rate = MathHelper.Lerp(1f, 0.5f, sink);
				Vector2 vector = start;
				vector.Y += (sink * 6f + sine.Value * MathHelper.Lerp(4f, 2f, sink)) / 4f;
				vector += bounce.Value * bounceDir * 4f;
				MoveToX(vector.X);
				MoveToY(vector.Y);
				if (smashParticles)
				{
					smashParticles = false;
					//	SmashParticles(bounceDir.Perpendicular());
					//	SmashParticles(-bounceDir.Perpendicular());
				}
			}
			sprite.Scale.X = Calc.Approach(sprite.Scale.X, 1f, Engine.DeltaTime * 4f);
			sprite.Scale.Y = Calc.Approach(sprite.Scale.Y, 1f, Engine.DeltaTime * 4f);
			LiftSpeed = Vector2.Zero;
		}

		public override void Render()
		{
			Vector2 position = sprite.Position;
			base.Render();
			sprite.Position = position;
		}

	}

}
