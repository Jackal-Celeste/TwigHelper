using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using static TwigHelper.ARC_Project.SnakeBlock;

namespace TwigHelper.ARC_Project
{
	[CustomEntity("TwigHelper/SnakeRotate")]
	public class SnakeBlockRotate : Entity
	{

		public SnakeBlockRotate(Vector2 position, int width, int height)
		{
			Position = position;
			base.Collider = new Hitbox(width, height);
		}

		public SnakeBlockRotate(EntityData data, Vector2 offset)
			: this(data.Position + offset, data.Width, data.Height)
		{
		}


		public override void Update()
		{
			List<Entity> heads;
			List<Entity> tails;
			base.Update();
			if (TwigModule.GetLevel() != null)
			{
				heads = TwigModule.GetLevel().Tracker.GetEntities<SnakeBlock>();
				tails = TwigModule.GetLevel().Tracker.GetEntities<SnakeBlockTail>();
				foreach (SnakeBlock e in heads)
				{
					if (e.Position == Position) { 
						e.changeDirection();
					}
				}
				foreach (SnakeBlockTail e in tails)
				{
					if (e.Position == Position)
					{
						e.changeDirection();
					}
				}

			}


		}

	}
}
