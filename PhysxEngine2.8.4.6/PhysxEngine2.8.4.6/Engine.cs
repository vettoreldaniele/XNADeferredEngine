using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

using StillDesign.PhysX;
using System.IO;

namespace PhysxEngine
{
	/// <summary>
	/// This is the main type for your game
	/// </summary>
	public class Engine
	{

		#region Properties
		public Game Game
		{
			get;
			private set;
		}

		public Camera Camera
		{
			get;
			private set;
		}

		public Core Core
		{
			get;
			private set;
		}
		public Scene Scene
		{
			get;
			private set;
		}

		public GraphicsDeviceManager DeviceManager
		{
			get;
			private set;
		}
		public GraphicsDevice Device
		{
			get
			{
				return this.DeviceManager.GraphicsDevice;
			}
		}

		public SpriteBatch SpriteBatch
		{
			get;
			private set;
		}
		#endregion

		private BasicEffect visEffect;

		public Engine(Game game)
		{
			this.Game = game;
			this.DeviceManager = new GraphicsDeviceManager(game);
			this.DeviceManager.PreparingDeviceSettings += new EventHandler<PreparingDeviceSettingsEventArgs>(OnPreparingDeviceSettings);



			this.DeviceManager.PreferredBackBufferWidth = (int)(GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width * 0.8);
			this.DeviceManager.PreferredBackBufferHeight = (int)(GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height * 0.8);
		}

		/// <summary>
		/// Allows the game to perform any initialization it needs to before starting to run.
		/// This is where it can query for any required services and load any non-graphic
		/// related content.  Calling base.Initialize will enumerate through any components
		/// and initialize them as well.
		/// </summary>
		public void Initialize()
		{
			this.Camera = new Camera(Device, Game.Window, new Vector3(0,5,20), new Vector3(0,0,0),0.1f,1000f);
			CoreDescription _coreDesc = new CoreDescription();
			UserOutput _output = new UserOutput();

			this.Core = new Core(_coreDesc, _output);
			var core = this.Core;
			core.SetParameter(PhysicsParameter.VisualizationScale, 2.0f);
			core.SetParameter(PhysicsParameter.VisualizeCollisionShapes, true);
			core.SetParameter(PhysicsParameter.VisualizeClothMesh, true);
			core.SetParameter(PhysicsParameter.VisualizeJointLocalAxes, true);
			core.SetParameter(PhysicsParameter.VisualizeJointLimits, true);
			core.SetParameter(PhysicsParameter.VisualizeFluidPosition, true);
			core.SetParameter(PhysicsParameter.VisualizeFluidEmitters, false); // Slows down rendering a bit to much
			core.SetParameter(PhysicsParameter.VisualizeForceFields, true);
			core.SetParameter(PhysicsParameter.VisualizeSoftBodyMesh, true);
			core.SetParameter(PhysicsParameter.VisualizeWorldAxes, true);
			core.SetParameter(PhysicsParameter.VisualizeContactForce, true);
			SceneDescription sceneDesc = new SceneDescription()
			{
				//SimulationType = SimulationType.Hardware,
				Gravity = new Vector3(0.0f, -9.81f, 0.0f).AsPhysX(),
				GroundPlaneEnabled = true
			};

			this.Scene = core.CreateScene(sceneDesc);

			HardwareVersion ver = Core.HardwareVersion;
			SimulationType simType = this.Scene.SimulationType;

			// Connect to the remote debugger if its there
			core.Foundation.RemoteDebugger.Connect("localhost");

			// Sets up the effect for drawing
			visEffect = new BasicEffect(Device)
			{
				VertexColorEnabled = true
			};
		}

		/// <summary>
		/// Allows the game to run logic such as updating the world,
		/// checking for collisions, gathering input, and playing audio.
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		public void Update(GameTime gameTime)
		{
			// Update Physics
			this.Scene.Simulate((float)gameTime.ElapsedGameTime.TotalMilliseconds / 1000.0f);
			this.Scene.FlushStream();
			this.Scene.FetchResults(SimulationStatus.RigidBodyFinished, true);

			this.Camera.Update(gameTime);
		}

		/// <summary>
		/// This is called when the game should draw itself.
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		public void Draw(RenderTarget2D renderTarget)
		{
			Device.SetRenderTarget(renderTarget);

			visEffect.World = Matrix.Identity;
			visEffect.View = this.Camera.View;
			visEffect.Projection = this.Camera.Projection;

			#region Debug Drawing
			
			DebugRenderable data = this.Scene.GetDebugRenderable();
			foreach (EffectPass ep in visEffect.CurrentTechnique.Passes)
			{
				ep.Apply();

				// Draw points as lines. 
				// http://blogs.msdn.com/b/shawnhar/archive/2010/03/22/point-sprites-in-xna-game-studio-4-0.aspx

				/*
				if (data.PointCount > 0)
				{
					var points = data.GetDebugPoints();

					var vertices = new VertexPositionColor[points.Length];
					for (int i = 0; i < data.PointCount; i++)
					{
						var point = points[i];

						vertices[i * 2 + 0] = new VertexPositionColor(point.Point.As<Vector3>(), Int32ToColor(point.Color));
					}

					
					Device.DrawUserPrimitives<VertexPositionColor>(PrimitiveType.LineList, vertices, 0, points.Length);
				}
				 * 
				*/

				if (data.LineCount > 0)
				{
					DebugLine[] lines = data.GetDebugLines();

					VertexPositionColor[] vertices = new VertexPositionColor[data.LineCount * 2];
					for (int x = 0; x < data.LineCount; x++)
					{
						DebugLine line = lines[x];

						vertices[x * 2 + 0] = new VertexPositionColor(new Vector3(line.Point0.X, line.Point0.Y, line.Point0.Z), Int32ToColor(line.Color));
						vertices[x * 2 + 1] = new VertexPositionColor(new Vector3(line.Point1.X, line.Point1.Y, line.Point1.Z), Int32ToColor(line.Color));
					}

					Device.DrawUserPrimitives<VertexPositionColor>(PrimitiveType.LineList, vertices, 0, lines.Length);
				}

				if (data.TriangleCount > 0)
				{
					DebugTriangle[] triangles = data.GetDebugTriangles();

					VertexPositionColor[] vertices = new VertexPositionColor[data.TriangleCount * 3];
					for (int x = 0; x < data.TriangleCount; x++)
					{
						DebugTriangle triangle = triangles[x];

						vertices[x * 3 + 0] = new VertexPositionColor(new Vector3(triangle.Point0.X, triangle.Point0.Y, triangle.Point0.Z), Int32ToColor(triangle.Color));
						vertices[x * 3 + 1] = new VertexPositionColor(new Vector3(triangle.Point1.X, triangle.Point1.Y, triangle.Point1.Z), Int32ToColor(triangle.Color));
						vertices[x * 3 + 2] = new VertexPositionColor(new Vector3(triangle.Point2.X, triangle.Point2.Y, triangle.Point2.Z), Int32ToColor(triangle.Color));
					}

					Device.DrawUserPrimitives<VertexPositionColor>(PrimitiveType.TriangleList, vertices, 0, triangles.Length);
				}

				// World axis
				{
					float axisLength = 30f;
					var vertices = new[] 
				{
					// X
					new VertexPositionColor(new Vector3(0,0,0), new Color(1, 0, 0)),
					new VertexPositionColor(new Vector3(axisLength,0,0), new Color(1, 0, 0)),

					// Y
					new VertexPositionColor(new Vector3(0,0,0), new Color(0, 1, 0)),
					new VertexPositionColor(new Vector3(0,axisLength,0), new Color(0, 1, 0)),

					// Z
					new VertexPositionColor(new Vector3(0,0,0), new Color(0, 0, 1)),
					new VertexPositionColor(new Vector3(0,0,axisLength), new Color(0, 0, 1)),
				};

					Device.DrawUserPrimitives<VertexPositionColor>(PrimitiveType.LineList, vertices, 0, vertices.Length / 2);
				}
			}

			#endregion

		}


		public static Color Int32ToColor(int color)
		{
			byte a = (byte)((color & 0xFF000000) >> 32);
			byte r = (byte)((color & 0x00FF0000) >> 16);
			byte g = (byte)((color & 0x0000FF00) >> 8);
			byte b = (byte)((color & 0x000000FF) >> 0);

			return new Color(r, g, b, a);
		}


		// NVIDIA PERFHUD
		void OnPreparingDeviceSettings(object sender, PreparingDeviceSettingsEventArgs e)
		{
			foreach (GraphicsAdapter adapter in GraphicsAdapter.Adapters)
			{
				if (adapter.Description.Contains("PerfHUD"))
				{
					e.GraphicsDeviceInformation.Adapter = adapter;
					GraphicsAdapter.UseReferenceDevice = true;  //  this is the modified line from usage in previous xna version
					break;
				}
			}
		}
	}
}