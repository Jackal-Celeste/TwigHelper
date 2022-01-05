﻿using System;
using System.Collections;
using Celeste;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;


namespace Celeste.Mod.TwigHelper.Entities
{

	[Tracked]
	[CustomEntity("TwigHelper/LargeRefill")]

	public class ShroomRefill : Entity
	{

		public WindController.Patterns Pattern;

		public static ParticleType P_Shatter;

		public static ParticleType P_Regen;

		public static ParticleType P_Glow;

		public static ParticleType P_ShatterTwo;

		public static ParticleType P_RegenTwo;

		public static ParticleType P_GlowTwo;

		private Sprite sprite;

		private Image outline;

		private Wiggler wiggler;

		private BloomPoint bloom;

		private VertexLight light;

		private Level level;

		private SineWave sine;

		private bool oneUse;

		private ParticleType p_shatter;

		private ParticleType p_regen;

		private ParticleType p_glow;

		private float respawnTimer;

		public bool check = false;

		public bool refillDash;

		public bool refillStamina;

		public bool last = false;

		public bool windNext = false;
		public Vector2 lastPos;

		public ShroomRefill(Vector2 position, bool oneUse, bool refillDash, bool refillStamina)
			: base(position)
		{
			this.oneUse = oneUse;
			this.refillDash = refillDash;
			this.refillStamina = refillStamina;
			base.Collider = new Hitbox(16f, 16f, -8f, -8f);
			Add(new PlayerCollider(OnPlayer));
			p_shatter = P_ShatterTwo;
			p_regen = P_RegenTwo;
			p_glow = P_GlowTwo;
			Add(outline = new Image(GFX.Game["objects/mushroomThing/outline"]));
			outline.CenterOrigin();
			outline.Visible = false;
			Add(sprite = TwigModule.spriteBank.Create("shroomy"));
			sprite.Play("idle");
			sprite.CenterOrigin();
			Add(wiggler = Wiggler.Create(1f, 4f, delegate (float v)
			{
				sprite.Scale = Vector2.One * (1f + v * 0.2f);
			}));
			Add(new MirrorReflection());
			Add(bloom = new BloomPoint(0.8f, 16f));
			Add(light = new VertexLight(Color.White, 1f, 16, 48));
			Add(sine = new SineWave(0.6f, 0f));
			sine.Randomize();
			UpdateY();
			base.Depth = -100;
			TwigModule.Session.HasShroomDash = false;
			TwigModule.Session.ShroomDashActive = false;
		}

		public ShroomRefill(EntityData data, Vector2 offset)
			: this(data.Position + offset, data.Bool("oneUse", false), data.Bool("refillDash", true), data.Bool("refillStamina", true))
		{
		}

		public override void Added(Scene scene)
		{
			base.Added(scene);
			level = SceneAs<Level>();
		}

		public override void Update()
		{
			base.Update();
			if (respawnTimer > 0f)
			{
				respawnTimer -= Engine.DeltaTime;
				if (respawnTimer <= 0f)
				{
					Respawn();
				}
			}
			UpdateY();
			light.Alpha = Calc.Approach(light.Alpha, sprite.Visible ? 1f : 0f, 4f * Engine.DeltaTime);
			bloom.Alpha = light.Alpha * 0.8f;
		}

		private void Respawn()
		{
			if (!Collidable)
			{
				Collidable = true;
				sprite.Visible = true;
				outline.Visible = false;
				base.Depth = -100;
				wiggler.Start();
				Audio.Play("event:/TwigHelper/diamond_return", Position);
			}
		}

		private void UpdateY()
		{
			Sprite obj = sprite;
			Sprite obj2 = sprite;
			float num2 = (bloom.Y = sine.Value * 2f);
			float num5 = (obj.Y = (obj2.Y = num2));

		}

		public override void Render()
		{
			if (sprite.Visible)
			{
				sprite.DrawOutline();
			}
			base.Render();
		}

		private void OnPlayer(Player player)
		{
			Audio.Play("event:/TwigHelper/diamond_touch", Position);
			Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
			Collidable = false;
			Add(new Coroutine(RefillRoutine(player)));
			respawnTimer = 2.5f;
			if (refillDash)
			{
				player.RefillDash();
			}
			if (refillStamina)
			{
				player.RefillStamina();
			}
			TwigModule.Session.HasShroomDash = true;
		}

		private IEnumerator RefillRoutine(Player player)
		{
			Celeste.Freeze(0.05f);
			yield return null;
			level.Shake();
			sprite.Visible = false;
			if (!oneUse)
			{
				outline.Visible = true;
			}
			Depth = 8999;
			yield return 0.05f;
			float angle = player.Speed.Angle();
			SlashFx.Burst(Position, angle);
			if (oneUse)
			{
				RemoveSelf();
			}
		}



	
	}
}