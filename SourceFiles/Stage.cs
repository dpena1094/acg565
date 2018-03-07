/*  
    Copyright (C) 2017 G. Michael Barnes
 
    The file Stage.cs is part of AGMGSKv9 a port and update of AGXNASKv8 from
    MonoGames 3.5 to MonoGames 3.6  

    AGMGSKv9 is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/

#region Using Statements
using System;
using System.IO;  // needed for trace()'s fout
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
#endregion

namespace AGMGSKv9 {

/// <summary>
/// Stage.cs  is the environment / framework for AGXNASK.
/// 
/// Stage declares and initializes the common devices, Inspector pane,
/// and user input options.  Stage attempts to "hide" the infrastructure of AGMGSK
/// for the developer.  It's subclass Stage is where the program specific aspects are
/// declared and created.
/// 
/// AGXNASK is a starter kit for Comp 565 assignments using XNA Game Studio 4.0
/// or MonoGames 3.?.
/// 
/// See AGXNASKv7Doc.pdf file for class diagram and usage information. 
/// 
/// Property Trace has been added.  Trace will print its string "value" to the
/// executable directory (where AGMGSKv8.exe is).
///   ../bin/Windows/debug/trace.txt file for Windows MonoGames projects
///   
/// 
/// 1/20/2016   last  updated
/// </summary>
public class Stage : Game {
   // Range is the length of the cubic volume of stage's terrain.
   // Each dimension (x, y, z) of the terrain is from 0 .. range (512)
   // The terrain will be centered about the origin when created.
   // Note some recursive terrain height generation algorithms (ie SquareDiamond)
   // work best with range = (2^n) - 1 values (513 = (2^9) -1).
   protected const int range = 512;
   protected const int spacing = 150;  // x and z spaces between vertices in the terrain
   protected const int terrainSize = range * spacing;
   // Window size
   private const bool runFullScreen = false;  // set false to use values on next line
   private const int windowWidth = 1280, windowHeight = 800;
   // Graphics device
   protected GraphicsDeviceManager graphics;
   protected GraphicsDevice display;    // graphics context
   protected BasicEffect effect;        // default effects (shaders)
   protected SpriteBatch spriteBatch;   // for Trace's displayStrings
   protected BlendState blending, notBlending;
   // stage Models
   protected Model boundingSphere3D;    // a  bounding sphere model
   protected Model wayPoint3D;          // a way point marker -- for paths.
   protected bool drawBoundingSpheres = false;
   protected bool fog = false;
   protected bool fixedStepRendering = true;     // 60 updates / second
   // Viewports and matrix for split screen display w/ Inspector.cs
   protected Viewport defaultViewport;
   protected Viewport inspectorViewport, sceneViewport;     // top and bottom "windows"
   protected Matrix sceneProjection, inspectorProjection;
   // variables required for use with Inspector
   protected const int InfoPaneSize = 5;   // number of lines / info display pane
   protected const int InfoDisplayStrings = 20;  // number of total display strings
   protected Inspector inspector;
   protected SpriteFont inspectorFont;
   // Projection values
   protected Matrix projection;
   protected float fov = (float)Math.PI / 3.0f;  // 60 degrees
   protected float hither = 5.0f, yon = terrainSize * 1.3f;
   protected bool yonFlag = true;
   // User event state
   protected GamePadState oldGamePadState;
   protected KeyboardState oldKeyboardState;
   // Lights
   protected Vector3 lightDirection, ambientColor, diffuseColor;
   // Cameras
   protected List<Camera> camera = new List<Camera>();  // collection of cameras
   protected Camera currentCamera, topDownCamera;
   protected int cameraIndex = 0;
   // Required entities -- all AGXNASK programs have a Player and Terrain
   protected Player player = null;
   protected NPAgent npAgent = null;
   protected Terrain terrain = null;
   protected List<Object3D> collidable = null;
   // Screen display and other information variables
   protected double fpsSecond;
   protected int draws, updates;
	private StreamWriter fout = null;
	// Stage variables
	private TimeSpan time;  // if you need to know the time see Property Time


   public Stage() : base() {
      graphics = new GraphicsDeviceManager(this);
      Content.RootDirectory = "Content";
	  // next line needed for Monogame 3.6
	  graphics.GraphicsProfile = GraphicsProfile.HiDef;
      graphics.SynchronizeWithVerticalRetrace = false;  // allow faster FPS
      // Directional light values
      lightDirection = Vector3.Normalize(new Vector3(-1.0f, -1.0f, -1.0f));
      ambientColor =  new Vector3(0.4f, 0.4f, 0.4f);
      diffuseColor =  new Vector3(0.2f, 0.2f, 0.2f);
      IsMouseVisible = true;  // make mouse cursor visible
      // information display variables
      fpsSecond = 0.0;
      draws = updates = 0;
      }

   // Properties

   public Vector3 AmbientLight {
      get { return ambientColor; } }

   public Model BoundingSphere3D {
      get { return boundingSphere3D; } }

   public List<Object3D> Collidable {
      get { return collidable; } }

   public Vector3 DiffuseLight {
      get { return diffuseColor; } }

   public GraphicsDevice Display {
      get { return display; } }
 
   public bool DrawBoundingSpheres {
      get { return drawBoundingSpheres; }
      set { drawBoundingSpheres = value;
            inspector.setInfo(8, String.Format("Draw bounding spheres = {0}", drawBoundingSpheres)); } }

   public bool FixedStepRendering {
      get { return fixedStepRendering; }
      set { fixedStepRendering = value;
            IsFixedTimeStep = fixedStepRendering; } }

   public bool Fog {
      get { return fog; }
      set { fog = value; } }

   public Vector3 LightDirection {
      get { return lightDirection; } }

   public Matrix Projection {
      get { return projection; } }

   public int Range {
      get { return range; } }

   public BasicEffect SceneEffect {
      get { return effect; } }

   public int Spacing {
      get { return spacing; }}

   public Terrain Terrain {
      get { return terrain; } }

   public int TerrainSize {
      get { return terrainSize; } }

	public TimeSpan Time {  // Update's GameTime
		get { return time; }}

	/// <summary>
	/// Trace = "string to be printed"
	/// Will append its value string to ../bin/debug/trace.txt file
	/// Each line has a time stamp 00:00:00:000 for hr:min:sec:msec
	/// For example:
	/// 00:00:00:000  |  April 21, 2016 trace output from AGMGSKv8
	/// 00:00:00:000  |  Scene created with 81 collidable objects.
	/// 00:00:22:437  |  system exit
	/// </summary>
	public string Trace {
		set {
			string timeStamp = string.Format("{0:D2}:{1:D2}:{2:D2}:{3:D3}  |  ", 
				time.Hours, time.Minutes, time.Seconds, time.Milliseconds); 
			fout.WriteLine(timeStamp + value); }}

   public Matrix View {
      get { return currentCamera.ViewMatrix; } }

   public Model WayPoint3D {
      get { return wayPoint3D; }}

   public bool YonFlag {
      get { return yonFlag; }
      set { yonFlag = value;
            if (yonFlag)  setProjection(yon);
            else setProjection(yon); } }

   // Methods

   public bool isCollidable(Object3D obj3d) {
      if (collidable.Contains(obj3d)) return true;
      else return false;
      }   

   /// <summary>
   /// Make sure that aMovableModel3D does not move off the terain.
   /// Called from MovableModel3D.Update()
   /// The Y dimension is not constrained -- code commented out.
   /// </summary>
   /// <param name="aName"> </param>
   /// <param name="newLocation"></param>
   /// <returns>true iff newLocation is within range</returns>
   public bool withinRange(String aName, Vector3 newLocation) {
      if (newLocation.X < spacing || newLocation.X > (terrainSize - 2 * spacing) ||
         newLocation.Z < spacing || newLocation.Z > (terrainSize - 2 * spacing)) {
         // inspector.setInfo(14, String.Format("error:  {0} can't move off the terrain", aName));
         return false; }
      else 
         return true; 
      }

   public void addCamera(Camera aCamera) {
      camera.Add(aCamera);
      cameraIndex++;
      }

	public String agentLocation(Agent agent)	{
		return string.Format("{0}:   Location ({1,5:f0} = {2,3:f0}, {3,3:f0}, {4,5:f0} = {5,3:f0})  Looking at ({6,5:f2},{7,5:f2},{8,5:f2})",
		agent.Name, agent.AgentObject.Translation.X, (agent.AgentObject.Translation.X) / spacing, agent.AgentObject.Translation.Y, agent.AgentObject.Translation.Z, (agent.AgentObject.Translation.Z) / spacing,
		agent.AgentObject.Forward.X, agent.AgentObject.Forward.Y, agent.AgentObject.Forward.Z);
		}

	public void setInfo(int index, string info) {
   inspector.setInfo(index, info);
   }

   protected void setProjection(float yonValue) {
      projection = Matrix.CreatePerspectiveFieldOfView(fov,
         graphics.GraphicsDevice.Viewport.AspectRatio, hither, yonValue);
      }

   /// <summary>
   /// Changing camera view for Agents will always set YonFlag false
   /// and provide a clipped view.
	/// 'x' selects the previous camera
	/// 'c' selects the next camera
   /// </summary>
   public void setCamera(int direction) {
      cameraIndex = (cameraIndex + direction);
      if (cameraIndex == camera.Count) cameraIndex = 0;
		if (cameraIndex < 0) cameraIndex = camera.Count -1;
      currentCamera = camera[cameraIndex];
      setProjection(yon); 
      }
 
   /// <summary>
   /// Get the height of the surface containing stage coordinates (x, z)
   /// </summary>
   public float surfaceHeight(float x, float z) {
      return terrain.surfaceHeight( (int) x/spacing, (int) z/spacing); }

	/// <summary>
	/// Sets the Object3D's height value to the corresponding surface position's height
	/// </summary>
	/// <param name="anObject3D"> has  Translation.X and Translation.Y values</param>
   public void setSurfaceHeight(Object3D anObject3D) {
      float terrainHeight = terrain.surfaceHeight( 
			(int) (anObject3D.Translation.X / spacing), 
			(int) (anObject3D.Translation.Z / spacing) );
      anObject3D.Translation = new Vector3( anObject3D.Translation.X, terrainHeight, 
			anObject3D.Translation.Z);
      }

   public void setBlendingState(bool state) {
      if (state) display.BlendState = blending;
      else display.BlendState = notBlending;
      }
  
   // Overridden Game class methods. 
  
   /// <summary>
   /// Allows the game to perform any initialization it needs to before starting to run.
   /// This is where it can query for any required services and load any non-graphic
   /// related content.  Calling base.Initialize will enumerate through any components
   /// and initialize them as well.
   /// </summary>
   protected override void Initialize() {
      // TODO: Add your initialization logic here
      base.Initialize();
      }


   /// <summary>
   /// Set GraphicDevice display and rendering BasicEffect effect.  
   /// Create SpriteBatch, font, and font positions.
   /// Creates the traceViewport to display information and the sceneViewport
   /// to render the environment.
   /// Create and add all DrawableGameComponents and Cameras.
   /// First, add all required contest:  Inspector, Cameras, Terrain, Agents
   /// Second, add all optional (scene specific) content
   /// </summary>
   protected override void LoadContent() {
      display = graphics.GraphicsDevice;
      effect = new BasicEffect(display);
      // Set up Inspector display
      spriteBatch = new SpriteBatch(display);      // Create a new SpriteBatch
      inspectorFont = Content.Load<SpriteFont> ("Consolas");    // Windows XNA && MonoGames
      // set window size 
      if (runFullScreen) {
            graphics.IsFullScreen = true;  
            graphics.PreferredBackBufferWidth = GraphicsDevice.DisplayMode.Width;
            graphics.PreferredBackBufferHeight = GraphicsDevice.DisplayMode.Height; }
         else { // run with window values 
            graphics.PreferredBackBufferWidth = windowWidth;  
            graphics.PreferredBackBufferHeight = windowHeight; 
            }
       graphics.ApplyChanges(); 
      // viewports
      defaultViewport = GraphicsDevice.Viewport;
      inspectorViewport = defaultViewport;
      sceneViewport = defaultViewport;
      inspectorViewport.Height = InfoPaneSize * inspectorFont.LineSpacing;
      inspectorProjection = Matrix.CreatePerspectiveFieldOfView(fov,
         inspectorViewport.Width/inspectorViewport.Height, hither, yon);
      sceneViewport.Height = defaultViewport.Height - inspectorViewport.Height;
      sceneViewport.Y = inspectorViewport.Height;
      sceneProjection = Matrix.CreatePerspectiveFieldOfView(fov,
         sceneViewport.Width /sceneViewport.Height, hither, yon);
      // create Inspector display
      Texture2D inspectorBackground = Content.Load<Texture2D>("inspectorBackground");
      inspector = new Inspector(display, inspectorViewport, inspectorFont, Color.Black, inspectorBackground);
      // create information display strings
      // help strings
      inspector.setInfo(0, "Academic Graphics MonoGames 3.6 Starter Kit for CSUN Comp 565 assignments.");
      inspector.setInfo(1, "Press keyboard for input (not case sensitive 'H' || 'h')   'Esc' to quit");
      inspector.setInfo(2, "Inspector toggles:  'H' help or info   'M'  matrix or info   'I'  displays next info pane.");
      inspector.setInfo(3, "Arrow keys move the player in, out, left, or right.  'R' resets player to initial orientation.");
      inspector.setInfo(4, "Stage toggles:  'B' bounding spheres, 'C' || 'X' cameras, 'T' fixed updates");
      // initialize empty info strings
      for (int i = 5; i < 20; i++) inspector.setInfo(i, "  ");
      // set blending for bounding sphere drawing
      blending = new BlendState();
      blending.ColorSourceBlend = Blend.SourceAlpha;
      blending.ColorDestinationBlend = Blend.InverseSourceAlpha;
      blending.ColorBlendFunction = BlendFunction.Add;
      notBlending = new BlendState();
      notBlending = display.BlendState;
      // Create and add stage components
      // You must have a TopDownCamera, BoundingSphere3D, WayPoint3D, Terrain, and Agents (player, npAgent) in your stage!
      // Place objects at a position, provide rotation axis and rotation radians.
      // All location vectors are specified relative to the center of the stage.
      // Create a top-down "Whole stage" camera view, make it first camera in collection.
      topDownCamera = new Camera(this, Camera.CameraEnum.TopDownCamera);
      camera.Add(topDownCamera);
      boundingSphere3D = Content.Load<Model>("boundingSphereV8");
      wayPoint3D = Content.Load<Model>("100x50x100Marker");				// model for navigation node display
      // Create required entities:  
      collidable = new List<Object3D>();  // collection of objects to test for collisions
		terrain = new Terrain(this, "terrain", "heightTexture", "colorTexture");
      Components.Add(terrain);
      // Load Agent mesh objects, meshes do not have textures
      player = new Player(this, "Chaser",
         new Vector3(510 * spacing, terrain.surfaceHeight(510, 507), 507 * spacing),
         new Vector3(0, 1, 0), 0.78f, "redAvatarV6");  // face looking diagonally across stage
      player.IsCollidable = true; // test collisions for player
      Components.Add(player);
      npAgent = new NPAgent(this, "Evader",
         new Vector3(490 * spacing, terrain.surfaceHeight(490, 450), 450 * spacing),
         new Vector3(0, 1, 0), 0.0f, "magentaAvatarV6");  // facing +Z
		npAgent.IsCollidable = false;  // npAgent does not test for collisions
      Components.Add(npAgent);
		// create file output stream for trace()
		fout = new StreamWriter("trace.txt", false);
		Trace = string.Format("{0} trace output from AGMGSKv8", DateTime.Today.ToString("MMMM dd, yyyy"));  
		//  ------ The wall and pack are required for Comp 565 projects, but not AGMGSK   ---------
		// create walls for navigation algorithms
		Wall wall = new Wall(this, "wall", "100x100x100Brick");
		Components.Add(wall);
		// create a pack for "flocking" algorithms
		// create a Pack of 6 dogs centered at (450, 500) that is leaderless
		Pack pack = new Pack(this, "dog", "dogV6", 6, 450, 430, null);
		Components.Add(pack);
      // ----------- OPTIONAL CONTENT HERE -----------------------
      // Load content for your project here
      // create a temple
      Model3D m3d = new Model3D(this, "temple", "templeV3");
      m3d.IsCollidable = true;  // must be set before addObject(...) and Model3D doesn't set it
      m3d.addObject(new Vector3(340 * spacing, terrain.surfaceHeight(340, 340), 340 * spacing),
         new Vector3(0, 1, 0), 0.79f); // , new Vector3(1, 4, 1));
      Components.Add(m3d);
		// create 20 clouds
		Cloud cloud = new Cloud(this, "cloud", "cloudV3", 20);
		// Set initial camera and projection matrix
		setCamera(1);  // select the "whole stage" camera
		Components.Add(cloud);
      // Describe the scene created
      Trace = string.Format("scene created:");
      Trace = string.Format("\t {0,4:d} components", Components.Count);
		Trace = string.Format("\t {0,4:d} collidable objects", Collidable.Count);
      Trace = string.Format("\t {0,4:d} cameras", camera.Count);
      }
  
   /// <summary>
   /// UnloadContent will be called once per game and is the place to unload
   /// all content.
   /// </summary>
   protected override void UnloadContent() {
      // TODO: Unload any non ContentManager content here
		Trace = "system exit";
		fout.Close(); // close fout file output stream, used by Trace
      }

	/// <summary>
	/// Uses an Inspector to display update and display information to player.
	/// All user input that affects rendering of the stage is processed either
	/// from the gamepad or keyboard.
	/// See Player.Update(...) for handling of user events that affect the player.
	/// The current camera's place is updated after all other GameComponents have 
	/// been updated.
	/// </summary>
	/// <param name="gameTime">Provides a snapshot of timing values.</param>
	protected override void Update(GameTime gameTime) {
		// set info pane values
		time = gameTime.TotalGameTime;
		fpsSecond += gameTime.ElapsedGameTime.TotalSeconds;
		updates++;
		if (fpsSecond >= 1.0) {
			inspector.setInfo(10,
			String.Format("{0} camera    Game time {1:D2}::{2:D2}::{3:D2}    {4:D} Updates/Seconds {5:D} Draws/Seconds",
				currentCamera.Name, time.Hours, time.Minutes, time.Seconds, updates.ToString(), draws.ToString()));
			draws = updates = 0;
			fpsSecond = 0.0;
			inspector.setInfo(11, agentLocation(player));
			inspector.setInfo(12, agentLocation(npAgent));
			// inspector lines 13 and 14 can be used to describe player and npAgent's status
			inspector.setMatrices("player", "npAgent", player.AgentObject.Orientation, npAgent.AgentObject.Orientation);
			}
		// Process user keyboard events that relate to the render state of the the stage
		KeyboardState keyboardState = Keyboard.GetState();
		if (keyboardState.IsKeyDown(Keys.Escape)) Exit();
		else if (keyboardState.IsKeyDown(Keys.B) && !oldKeyboardState.IsKeyDown(Keys.B)) 
		DrawBoundingSpheres = ! DrawBoundingSpheres;
		else if (keyboardState.IsKeyDown(Keys.C) && !oldKeyboardState.IsKeyDown(Keys.C)) 
		setCamera(1);
		else if (keyboardState.IsKeyDown(Keys.X) && !oldKeyboardState.IsKeyDown(Keys.X))
		setCamera(-1);
	
		// key event handlers needed for Inspector
		// set help display on
		else if (keyboardState.IsKeyDown(Keys.H) && !oldKeyboardState.IsKeyDown(Keys.H)) {
			inspector.ShowHelp = ! inspector.ShowHelp; 
			inspector.ShowMatrices = false; }
		// set info display on
		else if (keyboardState.IsKeyDown(Keys.I) && !oldKeyboardState.IsKeyDown(Keys.I)) 
		inspector.showInfo();
		// set miscellaneous display on
		else if (keyboardState.IsKeyDown(Keys.M) && !oldKeyboardState.IsKeyDown(Keys.M)) {
			inspector.ShowMatrices = ! inspector.ShowMatrices;
			inspector.ShowHelp = false; }
		// toggle update speed between FixedStep and ! FixedStep
		else if (keyboardState.IsKeyDown(Keys.T) && !oldKeyboardState.IsKeyDown(Keys.T))
		FixedStepRendering = ! FixedStepRendering;
		oldKeyboardState = keyboardState;    // Update saved state.
		base.Update(gameTime);  // update all GameComponents and DrawableGameComponents
		currentCamera.updateViewMatrix();
		}

   /// <summary>
   /// Draws information in the display viewport.
   /// Resets the GraphicsDevice's context and makes the sceneViewport active.
   /// Has Game invoke all DrawableGameComponents Draw(GameTime).
   /// </summary>
   /// <param name="gameTime">Provides a snapshot of timing values.</param>
   protected override void Draw(GameTime gameTime) {
      draws++;
      display.Viewport = defaultViewport; //sceneViewport;
      display.Clear(Color.CornflowerBlue);
      // Draw into inspectorViewport
      display.Viewport = inspectorViewport;
      spriteBatch.Begin();
      inspector.Draw(spriteBatch);
      spriteBatch.End();
      // need to restore state changed by spriteBatch
      GraphicsDevice.BlendState = BlendState.Opaque;
      GraphicsDevice.DepthStencilState = DepthStencilState.Default;
      GraphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;
      // draw objects in stage 
      display.Viewport = sceneViewport;
      display.RasterizerState = RasterizerState.CullNone;
      base.Draw(gameTime);  // draw all GameComponents and DrawableGameComponents
      }



	/*
	  /// <summary>
	  /// The main entry point for the application.
	 /// </summary>
	  static void Main(string[] args) {
		  using (Stage stage = new Stage()){ stage.Run(); }
		  }
	 */
   }
}
