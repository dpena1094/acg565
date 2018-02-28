/*  
    The file TerrainMap.cs is part of AGMGSKv8 
    Academic Graphics Starter Kit version 8 for MonoGames 3.5
   
    Mike Barnes
    2/3/2017

    AGMGSKv8 is free software: you can redistribute it and/or modify
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

/*	TerrainMap for MonoGames requirements Visual Studio and MonoDevelop:
	MonoDevelop Project | Edit References  | ALL | check System.Drawing, click OK
   Visual Studio Project | Add Refereces  | check System.Drawing, click OK
*/


using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Color = Microsoft.Xna.Framework.Color;

// needed for TerrainMap's use of Stream class in saveTerrainAsText()
// using System.Collections.Generic;


namespace TerrainMap {
	/// <summary>
	///     MonoGame project use, see note at end of summary.
	///     Generate and save two 2D textures:  heightTexture.png and colorTexture.png.
	///     File heightTexture.png stores a terrain's height values 0..255.
	///     File colorTexture.png stores the terrain's vertex color values.
	///     The files are saved in the execution directory.
	///     Pressing 't' will toggle the display between the height and color
	///     texture maps.  As distributed, the heightTexture will look all black
	///     because the values range from 0 to 3.
	///     The heightTexture will be mostly black since in the SK565v3 release there
	///     are two height areas:  grass plain and pyramid.  The pyramid (upper left corner)'
	///     will show grayscale values.
	///     Grass height values range from 0..3 -- which is black in greyscale.
	///     Note:  using grayscale in a texture to represent height constrains the
	///     range of heights from 0 to 255.  Often you need to scale the values into this range
	///     before saving the texture.  In your world's terrain you can then scale these
	///     values to the range you want.  This program does not scale since no values
	///     become greater than 255.
	///     Normally one thinks of a 2D texture as having [u, v] coordinates.
	///     In createHeightTexture() the height and in createColorTexture the color
	///     values are created.
	///     The heightMap and colorMap used are [u, v] -- 2D.  They are converted to a
	///     1D textureMap1D[u*v] when the colorTexture's values are set.
	///     This is necessary because the method
	///     newTexture.SetData
	///     <Color>
	///         (textureMap1D);
	///         requires a 1D array, not a 2D array.
	///         TerrainMap displays the textures using SpriteBatch.Draw(...).  These images are not the
	///         same as you will see in AGMGSK.  TerrianMap's displayed image flips the x and z
	///         coordinates (not sure why).
	///         Program design was influenced by Riemer Grootjans example 3.7
	///         Create a texture and save to file.
	///         In XNA 2.0 Grame Programming Recipies:  A Problem-Solution Approach,
	///         pp 176-178, Apress, 2008.
	///         MonoGames can write textures using System.Drawing.Color and System.Drawing.Bitmap
	///         classes.  You need to add a reference for System.Drawing in Visual Studio or MonoDevelop
	///         Visual Studio 2015, right click solution explorer References, select Add Reference....,
	///         select Assemblies | Framework, scroll down and select System.Drawing, click OK.
	///         Mike Barnes
	///         2/3/2017
	/// </summary>
	public class TerrainMap : Game {
		private const int _TextureWidth = 512; // textures should be powers of 2 for mipmapping

		private const int _TextureHeight = 512;
		private readonly GraphicsDeviceManager _graphics;
		private GraphicsDevice _device;
		private SpriteBatch _spriteBatch;
		private Texture2D _heightTexture, _colorTexture; // resulting textures 

		private Color[,] _colorMap, _heightMap; // values for the color and height textures

		private Color[] _textureMap1D; // hold the generated values for a texture.
		private readonly Random _random;
		private bool _showHeight;
		private KeyboardState _oldState;

		private int _edgeSize, _size, _max;
		private float[] _floatHeightMap;
		private float _roughness;

		/// <summary>
		///     Constructor
		/// </summary>
		public TerrainMap() {
			_graphics = new GraphicsDeviceManager(this);
			Window.Title = "Terrain Maps " + _TextureWidth + " by " + _TextureHeight +
			               " to change map 't'";
			Content.RootDirectory = "Content";
			_random = new Random();
		}

		/// <summary>
		///     Set the window size based on the texture dimensions.
		/// </summary>
		protected override void Initialize() {
			// Game object exists, set its window size 
			_graphics.PreferredBackBufferWidth = _TextureWidth;
			_graphics.PreferredBackBufferHeight = _TextureHeight;
			_graphics.ApplyChanges();
			base.Initialize();
		}

		/// <summary>
		///     Create and save two textures:
		///     heightTexture.png
		///     colorTexture.png
		/// </summary>
		protected override void LoadContent() {
			// Create a new SpriteBatch, which can be used to draw textures.
			_spriteBatch = new SpriteBatch(GraphicsDevice);
			_device = _graphics.GraphicsDevice;
			_heightTexture = CreateHeightTexture();
			_colorTexture = CreateColorTexture();
			//saveTerrainAsText("terrain.dat"); // FYI: save terrain as text file included in unused method
			SaveTexture(_heightMap, "heightTexture.png");
			SaveTexture(_colorMap, "colorTexture.png");
		}

		/// <summary>
		///     Uses .Net System.Drawing.Bitmap and System.Drawing.Color to create
		///     png image files.
		/// </summary>
		/// <param name="map"> Color [width, height] values for texture </param>
		/// <param name="filename"> texture's nanme</param>
		private void SaveTexture(Color[,] map, string filename) {
			var image = new Bitmap(_TextureWidth, _TextureHeight);
			for (var x = 0; x < _TextureWidth; x++)
			for (var z = 0; z < _TextureHeight; z++) {
				var color = System.Drawing.Color.FromArgb(
					Convert.ToInt32(map[x, z].R),
					Convert.ToInt32(map[x, z].G),
					Convert.ToInt32(map[x, z].B));
				image.SetPixel(x, z, color);
			}
			image.Save(filename, ImageFormat.Png);
		}

		/// <summary>
		///     Save the terrain data as a text file.  This method is provided for
		///     illustration purposes.  Not used by TerrainMap
		/// </summary>
		/// <param name="filename"> terrain data's file name</param>
		private void SaveTerrainAsText(string filename) {
			var fout = new StreamWriter("terrain.dat", false);
			fout.WriteLine("Terrain data: vertex positions (x,y,z) and colors (r,g,b)");
			for (var x = 0; x < _TextureWidth; x++)
			for (var z = 0; z < _TextureHeight; z++)
				fout.WriteLine(
					"{0}  {1}  {2}  {3}  {4}  {5}",
					x,
					Convert.ToInt32(_heightMap[x, z].R),
					z,
					Convert.ToInt32(_colorMap[x, z].R),
					Convert.ToInt32(_colorMap[x, z].G),
					Convert.ToInt32(_colorMap[x, z].B));
			fout.Close();
		}

		/// <summary>
		///     Create a height map as a texture of byte values (0..255)
		///     that can be viewed as a greyscale bitmap.
		///     You should re-write this method for your "Brownian" height map.
		///     Scale all height values to the range 0..255
		///     The variable percentColor is used to store the percent of this "color"
		///     or grayscale value.  Dividing a value (0..255)/255.0f results in a
		///     float 0 to 1.0f or the percent of that grayscale (or color).
		///     The scene here will have a plane of grass (heights 0..3) and
		///     a pyramid (height > 5).
		/// </summary>
		/// <returns>height texture</returns>

		private float GetHeight(int x, int y) {
			if (x < 0 || x > _max || y < 0 || y > _max) return -1;

			return _floatHeightMap[x + _size * y];
		}

		private void SetHeight(int x, int y, float val) {
			_floatHeightMap[x + _size * y] = val;
		}

		private static float[] RemoveNegativeOne(IEnumerable<float> values) {
			var arr = values.ToList();
			for (var i = 0; i < arr.Count; ++i) {
				if ((int)arr[i] == -1) arr.RemoveAt(i);
			}

			return arr.ToArray();
		}

		private static float Average(IEnumerable<float> values) {
			var valid = RemoveNegativeOne(values);
			var total = valid.Sum();

			return total / valid.Length;
		}

		private void Square(int x, int y, int size, float offset) {
			var average = Average(new[] {
				GetHeight(x - size, y - size),
				GetHeight(x + size, y - size),
				GetHeight(x + size, y + size),
				GetHeight(x - size, y + size)
			});

			SetHeight(x, y, average + offset);
		}

		private void Diamond(int x, int y, int size, float offset) {
			var average = Average(new[] {
				GetHeight(x, y - size),
				GetHeight(x + size, y),
				GetHeight(x, y + size),
				GetHeight(x - size, y)
			});

			SetHeight(x, y, average + offset);
		}

		private void Divide(int size) {
			var half = size / 2;
			var scale = _roughness * size;

			if (half < 1) return;

			for (var y = half; y < _max; y += size) {
				for (var x = half; x < _max; x += size) {
					Square(x, y, half, (float)_random.NextDouble() * scale * 2 - scale);
				}
			}

			for (var y = 0; y <= _max; y += half) {
				for (var x = (y + half) % size; x <= _max; x += size) {
					Diamond(x, y, half, (float)_random.NextDouble() * scale * 2 - scale);
				}
			}

			Divide(size / 2);
		}

		private Texture2D CreateHeightTexture() {
			_heightMap = new Color[_TextureWidth, _TextureHeight];

			_edgeSize = 9;
			_size = (int)Math.Pow(2, _edgeSize) + 1;
			_max = _size - 1;
			_floatHeightMap = new float[_size * _size];
			_roughness = 0.7f;

			for (var x = 0; x < _size; ++x) {
				for (var y = 0; y < _size; ++y) {
					_floatHeightMap[x + _size * y] = x + y;
				}
			}

			SetHeight(0, 0, _max);
			SetHeight(_max, 0, _size / 2.0f);
			SetHeight(_max, _max, 0f);
			SetHeight(0, _max, _max / 2.0f);

			Divide(_max);

			float minFloat = 0f, maxFloat = 0f;
			foreach (var val in _floatHeightMap) {
				if (val > maxFloat) maxFloat = val;
				if (val < minFloat) minFloat = val;
			}

			for (var k = 0; k < _floatHeightMap.Length; ++k) {
				_floatHeightMap[k] = (_floatHeightMap[k] - minFloat) / (maxFloat - minFloat);
			}

			for (var x = 0; x < _TextureWidth; ++x) {
				for (var y = 0; y < _TextureHeight; ++y) {
					var val = _floatHeightMap[x + _size * y];
					var colorVec3 = new Vector3(val, val, val);
					_heightMap[x, y] = new Color(colorVec3);
				}
			}

			// convert heightMap[,] to textureMap1D[]
			_textureMap1D = new Color[_TextureWidth * _TextureHeight];
			var i = 0;
			for (var x = 0; x < _TextureWidth; x++)
				for (var z = 0; z < _TextureHeight; z++) {
					_textureMap1D[i] = _heightMap[x, z];
					i++;
				}
			// create the texture to return.       
			var newTexture = new Texture2D(_device, _TextureWidth, _TextureHeight);
			newTexture.SetData(_textureMap1D);
			return newTexture;
		}

		/// <summary>
		///     Return random int -range ... range
		/// </summary>
		/// <param name="range"></param>
		/// <returns></returns>
		private int FractalRand(int range) {
			if (_random.Next(2) == 0) // flip a coin
				return _random.Next(range);
			return -1 * _random.Next(range);
		}


		/// <summary>
		///     Convert a height value in the range of 0 ... 255 to
		///     a Vector4 value that will be later converted to a Color.
		///     Vector4 is used instead of color to add some random noise to the value
		/// </summary>
		/// <param name="h"></param>
		/// <returns></returns>
		private Vector4 HeightToVector4(int h) {
			int r, g, b;
			if (h < 50) {
				// dark grass
				r = 0;
				g = 128 + _random.Next(65); // 128 .. 192 ;
				b = 0;
			} else if (h < 75) {
				// lighter green grass
				r = 64 + _random.Next(65); // 64 .. 128 ;
				g = 128 + _random.Next(33); // 128 .. 160 ;
				b = _random.Next(33); // 0 .. 32 
			} else if (h < 100) {
				// lighter green / yellow grass
				r = 128 + _random.Next(33); // 128 .. 160 
				g = 160 + _random.Next(33); // 160 .. 192
				b = 32 + _random.Next(33); // 32 .. 64
			} else if (h < 125) {
				// green .. brown dirt
				r = 160 + _random.Next(21); // 160 .. 180
				g = 192 - _random.Next(65); // 192 .. 128
				b = 64 - _random.Next(33); // 64 .. 32
			} else if (h < 150) {
				// dark to lighter dirt
				r = 180 - _random.Next(61); // 180 .. 120
				g = 120 - _random.Next(21); // 120 .. 100
				b = 20;
			} else if (h < 175) {
				// light dirt to gray
				r = 180 - _random.Next(41); // 180 .. 120
				g = 120 - _random.Next(21); // 120 .. 100
				b = 20 + _random.Next(41); // 20 .. 60
			} else if (h < 225) // dark gray to light gray
			{
				r = g = b = 128 + _random.Next(98); // 128 .. 225
			}
			// top of mountains don't need randomization.
			else // snow
			{
				r = g = b = h;
			}
			// add noise with fractalRand
			if (h <= 175) {
				// not snow
				// randomize values and clamp values to 0..255
				r = Math.Abs((r + FractalRand(20)) % 255);
				g = Math.Abs((g + FractalRand(20)) % 255);
				b = Math.Abs((b + FractalRand(20)) % 255);
			} else if (h > 175 && h < 225) // snow
			{
				r = g = b = Math.Abs((r + FractalRand(20)) % 255);
			}
			return new Vector4(
				r / 255.0f,
				g / 255.0f,
				b / 255.0f,
				1.0f); // must be floats
		}

		/// <summary>
		///     Create a color texture that will be used to "color" the terrain.
		///     Some comments about color that might explain some of the code in createColorTexture().
		///     Colors can be converted to vector4s.   vector4Value =  colorValue / 255.0
		///     You would replace the code in this method with a call to heightToVector4(...)
		///     for use with your actual height map values.
		///     color's (RGBA), color.ToVector4()
		///     Color.DarkGreen (R:0 G:100 B:0 A:255)    vector4 (X:0 Y:0.392 Z:0 W:1)
		///     Color.Green     (R:0 G:128 B:0 A:255)    vector4 (X:0 Y:0.502 Z:0 W:1)
		///     Color.OliveDrab (R:107 G:142 B:35 A:255) vector4 (X:0.420 Y:0.557 Z:0.137, W:1)
		///     You can create colors with new Color(byte, byte, byte, byte) where byte = 0..255
		///     or, new Color(byte, byte, byte).
		///     The Color conversion to Vector4 and back is used to add noise.
		///     You could just have Color.
		/// </summary>
		/// <returns>color texture</returns>
		private Texture2D CreateColorTexture() {
			const int grassHeight = 5;
			var colorVec4 = new Vector4();
			_colorMap = new Color[_TextureWidth, _TextureHeight];
			for (var x = 0; x < _TextureWidth; x++)
			for (var z = 0; z < _TextureHeight; z++) {
				if (_heightMap[x, z].R < grassHeight) // make random grass
					switch (_random.Next(3)) {
						case 0:
							colorVec4 = new Color(0, 100, 0, 255).ToVector4();
							break; // Color.DarkGreen
						case 1:
							colorVec4 = Color.Green.ToVector4();
							break;
						case 2:
							colorVec4 = Color.OliveDrab.ToVector4();
							break;
					}
				// color the pyramid based on height
				else colorVec4 = HeightToVector4(_heightMap[x, z].R);
				// add some noise, convert to a color, and set colorMap
				colorVec4 = colorVec4 + new Vector4((float)(_random.NextDouble() / 20.0));
				_colorMap[x, z] = new Color(colorVec4);
			}
			// convert colorMap[,] to textureMap1D[]
			_textureMap1D = new Color[_TextureWidth * _TextureHeight];
			var i = 0;
			for (var x = 0; x < _TextureWidth; x++)
			for (var z = 0; z < _TextureHeight; z++) {
				_textureMap1D[i] = _colorMap[x, z];
				i++;
			}
			// create the texture to return.   
			var newTexture = new Texture2D(_device, _TextureWidth, _TextureHeight);
			newTexture.SetData(_textureMap1D);
			return newTexture;
		}
/*
   /// <summary>
   /// UnloadContent will be called once per game and is the place to unload
   /// all content.
   /// </summary>
   protected override void UnloadContent() {
      // TODO: Unload any non ContentManager content here
      }
*/

		/// <summary>
		///     Process user keyboard input.
		///     Pressing 'T' or 't' will toggle the display between the height and color textures
		/// </summary>
		protected override void Update(GameTime gameTime) {
			var keyboardState = Keyboard.GetState();
			if (keyboardState.IsKeyDown(Keys.Escape)) Exit();
			else if (Keyboard.GetState().IsKeyDown(Keys.T) &&
			         !_oldState.IsKeyDown(Keys.T))
				_showHeight = !_showHeight;
			else if (Keyboard.GetState().IsKeyDown(Keys.R) &&
			         !_oldState.IsKeyDown(Keys.R)) {
				_heightTexture = CreateHeightTexture();
				_colorTexture = CreateColorTexture();
			} else if (Keyboard.GetState().IsKeyDown(Keys.S) &&
			           !_oldState.IsKeyDown(Keys.S)) {
				SaveTexture(_heightMap, "heightTexture.png");
				SaveTexture(_colorMap, "colorTexture.png");
			}
			_oldState = keyboardState; // Update saved state.
			base.Update(gameTime);
		}

		/// <summary>
		///     Display the textures.
		/// </summary>
		/// <param name="gameTime"></param>
		protected override void Draw(GameTime gameTime) {
			_device.Clear(
				ClearOptions.Target | ClearOptions.DepthBuffer,
				Color.White,
				1,
				0);
			_spriteBatch.Begin();
			_spriteBatch.Draw(
				_showHeight ? _heightTexture : _colorTexture,
				Vector2.Zero,
				Color.White);
			_spriteBatch.End();
			base.Draw(gameTime);
		}
	}
}