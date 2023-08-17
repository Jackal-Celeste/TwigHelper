// Celeste.LightningRenderer
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
namespace Celeste.Mod.JackalCollabHelper.Entities
{
	[Tracked]
	public class DarkMatterRenderer : Entity
	{
		public class Edge
		{
			public DarkMatter Parent;

			public bool Visible;

			public Vector2 A;

			public Vector2 B;

			public Vector2 Min;

			public Vector2 Max;

			public Vector2 position;

			public Edge(DarkMatter parent, Vector2 a, Vector2 b)
			{
				Parent = parent;
				Visible = false;
				A = a;
				B = b;
				Min = new Vector2(Math.Min(a.X, b.X), Math.Min(a.Y, b.Y));
				Max = new Vector2(Math.Max(a.X, b.X), Math.Max(a.Y, b.Y));
			}
			public bool InView(ref Rectangle view)
			{
				return (float)view.Left < Parent.X + Max.X && (float)view.Right > Parent.X + Min.X && (float)view.Top < Parent.Y + Max.Y && (float)view.Bottom > Parent.Y + Min.Y;
			}
		}

		public List<DarkMatter> list = new List<DarkMatter>();

		public List<Edge> edges = new List<Edge>();

		private VertexPositionColor[] edgeVerts;

		private VirtualMap<bool> tiles;

		private Rectangle levelTileBounds;

		private uint edgeSeed;

		private uint leapSeed;

		private bool dirty;

		public float totalTime;

		public Color color;

		public Color fillColor;

		public float frameCheck = 0f;
		public List<DarkMatter> barriers = new List<DarkMatter>();
		public DarkMatterController controller;

	public DarkMatterRenderer()
		{
			base.Tag = (int)Tags.Global | (int)Tags.TransitionUpdate;
			base.Depth = -10100;
			Add(new CustomBloom(OnRenderBloom));
		}



		public Color saturatedIndigoCycle(bool off)
		{
		    float time;
		    if (off)
		    {
		        time = (float)(((2 * totalTime) + 5) % 10);
		    }
		    else
		    {
		        time = (float)(2 * totalTime % 10);
		    }
		    
		    int timeInt = (int)time;
		    
		    if (TwigModule.GetLevel() != null)
		    {
		        int colorIndex1;
		        int colorIndex2;
		
		        if (off)
		        {
		            colorIndex1 = (timeInt + 1) % 6;
		            colorIndex2 = timeInt % 6;
		        }
		        else
		        {
		            colorIndex1 = (timeInt - 1) % 6;
		            colorIndex2 = timeInt % 6;
		        }
		
		        Color color1 = Calc.HexToColor(controller.darkMatterLightningColors[colorIndex1]);
		        Color color2 = Calc.HexToColor(controller.darkMatterLightningColors[colorIndex2]);
		
		        color = Color.Lerp(color1, color2, time % 1f);
		    }
		
		    return color;
		}



		public void Track(DarkMatter block, Level level, DarkMatterController darkMatterController)
		{
			controller = darkMatterController;
			list.Add(block);
			if (tiles == null)
			{
				levelTileBounds = level.TileBounds;
				tiles = new VirtualMap<bool>(levelTileBounds.Width, levelTileBounds.Height, emptyValue: false);
			}
				for (int i = (int)block.X / 8; i < ((int)block.X + block.Width) / 8; i++)
				{
					for (int j = (int)block.Y / 8; j < ((int)block.Y + block.Height) / 8; j++)
					{
						tiles[i - levelTileBounds.X, j - levelTileBounds.Y] = true;
					}
				}

			foreach (DarkMatter barrier in level.Tracker.GetEntities<DarkMatter>())
			{
				barriers.Add(barrier);
			}
			dirty = true;
		}

		public void Untrack(DarkMatter block)
		{
			list.Remove(block);
			if (list.Count <= 0)
			{
				tiles = null;
			}
			else
			{
				for (int i = (int)block.X / 8; (float)i < block.Right / 8f; i++)
				{
					for (int j = (int)block.Y / 8; (float)j < block.Bottom / 8f; j++)
					{
						tiles[i - levelTileBounds.X, j - levelTileBounds.Y] = false;
					}
				}
			}
			dirty = false;
		}

		public override void Update()
		{
			Depth = -8000;
			totalTime += Engine.DeltaTime;
			if (dirty)
			{
				RebuildEdges();
			}
			if (TwigModule.GetLevel() != null)
			{
				ToggleEdges();
			}
				if (base.Scene.OnInterval(0.1f))
				{
					edgeSeed = (uint)Calc.Random.Next();
				}
				if (base.Scene.OnInterval(0.7f))
				{
					leapSeed = (uint)Calc.Random.Next();
				}
			
		}

		public void ToggleEdges(bool immediate = false)
		{
		    Camera camera = (base.Scene as Level).Camera;
		    Rectangle view = new Rectangle((int)camera.Left - 4, (int)camera.Top - 4, (int)(camera.Right - camera.Left) + 8, (int)(camera.Bottom - camera.Top) + 8);
		
		    for (int i = 0; i < edges.Count; i++)
		    {
		        var edge = edges[i];
		        if (edge != null)
		        {
		            bool inView = edge.InView(ref view);
		
		            if (immediate)
		            {
		                edge.Visible = inView;
		            }
		            else
		            {
		                float intervalDelay = (float)i * 0.01f;
		
		                if (!edge.Visible && base.Scene.OnInterval(0.05f, intervalDelay) && inView)
		                {
		                    edge.Visible = true;
		                }
		                else if (edge.Visible && base.Scene.OnInterval(0.25f, intervalDelay) && !inView)
		                {
		                    edge.Visible = false;
		                }
		            }
		        }
		    }
		
		    if (barriers.Count > 0)
		    {
		        for (int i = 0; i < edges.Count; i++)
		        {
		            var edge = edges[i];
		            if (edge != null && edge.InView(ref view))
		            {
		                var parent = edge.Parent;
		                if ((parent.zapLeft && edge.A.X == 0 && edge.B.X == 0) ||
		                    (parent.zapRight && edge.A.X == parent.Width && edge.B.X == parent.Width) ||
		                    (parent.zapTop && edge.A.Y == 0 && edge.B.Y == 0) ||
		                    (parent.zapBottom && edge.A.Y == parent.Height && edge.B.Y == parent.Height))
		                {
		                    edge.Visible = true;
		                }
		                else
		                {
		                    edges[i] = null;
		                }
		            }
		        }
		    }
		}




		private void RebuildEdges()
		{
		    dirty = false;
		    edges.Clear();
		    if (list.Count <= 0)
		    {
		        return;
		    }
		    Level level = base.Scene as Level;
		    int left = level.TileBounds.Left;
		    int top = level.TileBounds.Top;
		    int right = level.TileBounds.Right;
		    int bottom = level.TileBounds.Bottom;
		
		    Point[] array = new Point[4]
		    {
		        new Point(0, -1),
		        new Point(0, 1),
		        new Point(-1, 0),
		        new Point(1, 0)
		    };
		
		    foreach (DarkMatter item in list)
		    {
		        int startX = (int)item.X / 8;
		        int endX = (int)(item.X + item.Width) / 8 + 1;
		        int startY = (int)item.Y / 8;
		        int endY = (int)(item.Y + item.Height) / 8 + 1;
		
		        for (int i = startX; i < endX; i++)
		        {
		            for (int j = startY; j < endY; j++)
		            {
		                foreach (Point point in array)
		                {
		                    Point point2 = new Point(-point.Y, point.X);
		                    int neighborX = i + point.X;
		                    int neighborY = j + point.Y;
		                    int oppositeX = i - point2.X;
		                    int oppositeY = j - point2.Y;
		
		                    if (Inside(neighborX, neighborY) || (Inside(oppositeX, oppositeY) && !Inside(neighborX - point2.X, neighborY - point2.Y)))
		                    {
		                        continue;
		                    }
		
		                    Point point3 = new Point(i, j);
		                    Point point4 = new Point(i + point2.X, j + point2.Y);
		                    Vector2 value = new Vector2(4f) + new Vector2(point.X - point2.X, point.Y - point2.Y) * 4f;
		                    int num = 1;
		
		                    while (Inside(point4.X, point4.Y) && !Inside(point4.X + point.X, point4.Y + point.Y))
		                    {
		                        point4.X += point2.X;
		                        point4.Y += point2.Y;
		                        num++;
		
		                        if (num > 8)
		                        {
		                            Vector2 a = new Vector2(point3.X, point3.Y) * 8f + value - item.Position;
		                            Vector2 b = new Vector2(point4.X, point4.Y) * 8f + value - item.Position;
		                            edges.Add(new Edge(item, a, b));
		                            num = 0;
		                            point3 = point4;
		                        }
		                    }
		
		                    if (num > 0)
		                    {
		                        Vector2 a = new Vector2(point3.X, point3.Y) * 8f + value - item.Position;
		                        Vector2 b = new Vector2(point4.X, point4.Y) * 8f + value - item.Position;
		                        edges.Add(new Edge(item, a, b));
		                    }
		                }
		            }
		        }
		    }
		
		    if (edgeVerts == null)
		    {
		        edgeVerts = new VertexPositionColor[1024];
		    }
		}



		private bool Inside(int tx, int ty)
		{
			return tiles[tx - levelTileBounds.X, ty - levelTileBounds.Y];
		}

		private void OnRenderBloom()
		{
			Camera camera = (base.Scene as Level).Camera;
			Rectangle rectangle = new Rectangle((int)camera.Left, (int)camera.Top, (int)(camera.Right - camera.Left), (int)(camera.Bottom - camera.Top));
		}


		public override void Render()
		{
		    if (list.Count <= 0 || edges.Count <= 0)
		    {
		        return;
		    }
		
		    Camera camera = (base.Scene as Level).Camera;
		    int index = 0;
		    uint seed = leapSeed;
		
		    foreach (Edge edge in edges)
		    {
		        if (edge != null && edge.Visible)
		        {
		            Color cycleColor = saturatedIndigoCycle();
		            Color cycleOffColor = saturatedIndigoCycleOff();
		
		            DrawSimpleLightning(ref index, ref edgeVerts, edgeSeed, edge.Parent.Position, edge.A, edge.B, cycleColor);
		            DrawSimpleLightning(ref index, ref edgeVerts, edgeSeed + 1, edge.Parent.Position, edge.A, edge.B, cycleOffColor);
		
		            if (PseudoRand(ref seed) % 30u == 0)
		            {
		                DrawBezierLightning(ref index, ref edgeVerts, edgeSeed, edge.Parent.Position, edge.A, edge.B, 24f, 10, cycleColor);
		                DrawBezierLightning(ref index, ref edgeVerts, edgeSeed + 1, edge.Parent.Position, edge.A, edge.B, 24f, 10, cycleOffColor);
		            }
		        }
		    }
		
		    if (index > 0)
		    {
		        GameplayRenderer.End();
		        GFX.DrawVertices(camera.Matrix, edgeVerts, index);
		        GameplayRenderer.Begin();
		    }
		}
		
		private static void DrawSimpleLightning(ref int index, ref VertexPositionColor[] verts, uint seed, Vector2 pos, Vector2 a, Vector2 b, Color color, float thickness = 1f)
		{
		    seed += (uint)(a.GetHashCode() + b.GetHashCode());
		    a += pos;
		    b += pos;
		    float num = (b - a).Length();
		    Vector2 vector = (b - a) / num;
		    Vector2 vector2 = vector.TurnRight();
		    a += vector2;
		    b += vector2;
		    Vector2 vector3 = a;
		    int num2 = ((PseudoRand(ref seed) % 2u != 0) ? 1 : (-1));
		    float num3 = PseudoRandRange(ref seed, 0f, (float)Math.PI * 2f);
		    float num4 = 0f;
		    float num5 = (float)index + ((b - a).Length() / 4f + 1f) * 6f;
		
		    if (num5 >= verts.Length)
		    {
		        Array.Resize(ref verts, Math.Max(verts.Length * 2, (int)num5));
		    }
		
		    for (int i = index; (float)i < num5; i++)
		    {
		        verts[i].Color = color;
		    }
		
		    do
		    {
		        float num6 = PseudoRandRange(ref seed, 0f, 4f);
		        num3 += 0.1f;
		        num4 += 4f + num6;
		        Vector2 vector4 = a + vector * num4;
		
		        if (num4 < num)
		        {
		            vector4 += num2 * vector2 * num6 - vector2;
		        }
		        else
		        {
		            vector4 = b;
		        }
		
		        AddLightningVertices(ref verts, ref index, vector3, vector4, vector2, thickness, color);
		        vector3 = vector4;
		        num2 = -num2;
		    }
		    while (num4 < num);
		}
		
		private static void AddLightningVertices(ref VertexPositionColor[] verts, ref int index, Vector2 start, Vector2 end, Vector2 direction, float thickness, Color color)
		{
		    if (index + 6 >= verts.Length)
		    {
		        Array.Resize(ref verts, verts.Length * 2);
		    }
		
		    verts[index++].Position = new Vector3(start - direction * thickness, 0f);
		    verts[index++].Position = new Vector3(end - direction * thickness, 0f);
		    verts[index++].Position = new Vector3(end + direction * thickness, 0f);
		    verts[index++].Position = new Vector3(start - direction * thickness, 0f);
		    verts[index++].Position = new Vector3(end + direction * thickness, 0f);
		    verts[index++].Position = new Vector3(start, 0f);
		    ColorizeVertices(ref verts, index - 6, index, color);
		}
		
		private static void ColorizeVertices(ref VertexPositionColor[] verts, int startIndex, int endIndex, Color color)
		{
		    for (int i = startIndex; i < endIndex; i++)
		    {
		        verts[i].Color = color;
		    }
		}
		
		private static void DrawBezierLightning(ref int index, ref VertexPositionColor[] verts, uint seed, Vector2 pos, Vector2 a, Vector2 b, float anchor, int steps, Color color)
		{
		    seed += (uint)(a.GetHashCode() + b.GetHashCode());
		    a += pos;
		    b += pos;
		    Vector2 vector = (b - a).SafeNormalize().TurnRight();
		    SimpleCurve simpleCurve = new SimpleCurve(a, b, (b + a) / 2f + vector * anchor);
		    int num = index + (steps + 2) * 6;
		
		    if (num >= verts.Length)
		    {
		        Array.Resize(ref verts, Math.Max(verts.Length * 2, (int)num));
		    }
		
		    Vector2 vector2 = simpleCurve.GetPoint(0f);
		    for (int i = 0; i <= steps; i++)
		    {
		        Vector2 point = simpleCurve.GetPoint((float)i / (float)steps);
		        if (i != steps)
		        {
		            point += new Vector2(PseudoRandRange(ref seed, -2f, 2f), PseudoRandRange(ref seed, -2f, 2f));
		        }
		        AddLightningVertices(ref verts, ref index, vector2, point, vector, 0f, color);
		        vector2 = point;
		    }
		}


		private static uint PseudoRand(ref uint seed)
		{
			seed ^= seed << 13;
			seed ^= seed >> 17;
			return seed;
		}

		public static float PseudoRandRange(ref uint seed, float min, float max)
		{
			return min + (float)(PseudoRand(ref seed) & 0x3FFu) / 1024f * (max - min);
		}
	}
}
