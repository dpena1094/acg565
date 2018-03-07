/*  
    Copyright (C) 2017 G. Michael Barnes
 
    The file Terrain.cs is part of AGMGSKv9 a port and update of AGXNASKv8 from
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
/// Terrain represents a ground.
/// The vertices have position and color.  Terrain width = height.  
/// Reads two textures to set terrain height and color values.
/// You might want to pre-compute and store heights of surfaces to be 
/// returned by the surfaceHeight(x, z) method.
/// 
/// 1/25/2012 last changed
/// </summary>
/// 
public class Terrain : IndexVertexBuffers {
   protected VertexPositionColor[] vertex;  // stage vertices    
   private int height = 0, width = 0, multiplier = 20, spacing = 0;
   private int[,] terrainHeight; 
   private BasicEffect effect;
   private GraphicsDevice display;

   public Terrain(Stage theStage, string label, string terrainDataFile) 
      : base (theStage, label) 
      {
		constructorInit();  // common constructor initialization code, base call sets "stage"
     // read vertex data from "terrain.dat" file
		System.IO.TextReader file = System.IO.File.OpenText(terrainDataFile);
		file.ReadLine(); // skip the first description line x y z r g b
		int i = 0;   // index for vertex[]
		string line;
		string[] token;
		for (int z = 0; z < height; z++)
			for (int x = 0; x < width; x++) {
				line = file.ReadLine();
				token = line.Split(' ');
				terrainHeight[x, z] = int.Parse(token[1]) * multiplier;  // Y
				vertex[i] = new VertexPositionColor(
					new Vector3( int.Parse(token[0]) * spacing, terrainHeight[x,z], int.Parse(token[2]) * spacing), // position
					new Color(int.Parse(token[3]), int.Parse(token[4]), int.Parse(token[5])) );  // material
				i++;
			}
		file.Close();
		makeIndicesSetData();
		}

	/// <summary>
	/// Make a Terrain from 2 png texture files.  
	/// Must have __XNA4__ capabilities to read from png files.
	/// </summary>
	/// <param name="theStage"></param>
	/// <param name="label"></param>
	/// <param name="heightFile"></param>
	/// <param name="colorFile"></param>
   public Terrain(Stage theStage, string label, string heightFile, string colorFile) 
      : base (theStage, label) {
		Texture2D heightTexture, colorTexture;
		Microsoft.Xna.Framework.Color[] heightMap, colorMap;
		constructorInit();  // common constructor initialization code, base call sets "stage"
      heightTexture = stage.Content.Load<Texture2D>(heightFile);
      heightMap = 
         new Microsoft.Xna.Framework.Color[width * height];
      heightTexture.GetData<Microsoft.Xna.Framework.Color>(heightMap);
      // create colorMap values from colorTexture
      colorTexture = stage.Content.Load<Texture2D>(colorFile);
      colorMap = 
         new Microsoft.Xna.Framework.Color[width * height];
      colorTexture.GetData<Microsoft.Xna.Framework.Color>(colorMap);
      // create  vertices for terrain
      Vector4 vector4;
      int vertexHeight;
      int i = 0;
      for (int z = 0; z < height; z++)
         for (int x = 0; x < width; x++)  {
            vector4 = heightMap[i].ToVector4();       // convert packed Rgba32 values to floats
            vertexHeight = (int) (vector4.X * 255);   // scale vertexHeight 0..255
            vertexHeight *= multiplier;               // multiply height
            terrainHeight[x, z] = vertexHeight;       // save height for navigation
            vertex[i] = new VertexPositionColor(
               new Vector3(x * spacing, vertexHeight, z * spacing), 
               new Color(colorMap[i].ToVector4()));
            i++;
            }
      // free up unneeded maps
      colorMap = null;
      heightMap = null;
		makeIndicesSetData();
		}	

	private void constructorInit() {
      range = stage.Range;
      width = height = range;
      nVertices = width * height;
      terrainHeight = new int[width, height];
      vertex = new VertexPositionColor[nVertices];
      nIndices = (width -1) * (height - 1) * 6;
      indices = new int[nIndices];  // there are 6 indices 2 faces / 4 vertices 
      spacing = stage.Spacing;
      // set display information 
      display = stage.Display;
      effect = stage.SceneEffect;
		}
 
	private void makeIndicesSetData() {
      // set indices clockwise from point of view ... surfaces really left handed
      int i = 0;
      for (int z = 0; z < height - 1; z++)
         for (int x = 0; x < width - 1; x++) {
            indices[i++] = z * width + x;
            indices[i++] = z * width + x + 1;
            indices[i++] = (z + 1) * width + x;
            indices[i++] = (z + 1) * width + x;
            indices[i++] = z * width + x + 1;
            indices[i++] = (z + 1) * width + x + 1;
            }
      
      // create VertexBuffer and store on GPU
      vb = new VertexBuffer(display, typeof(VertexPositionColor), vertex.Length, BufferUsage.WriteOnly);
      vb.SetData<VertexPositionColor>(vertex); // , 0, vertex.Length);
      // create IndexBuffer and store on GPU
      ib = new IndexBuffer(display,typeof(int), indices.Length, BufferUsage.WriteOnly);
      IB.SetData<int>(indices);     
      }


   // Properties

   public int Spacing {
      get { return stage.Spacing;} }

   // Methods

   ///<summary>
   /// Height of  surface containing position (x,z) terrain coordinates.
   /// This method is a "stub" for the correct get code.
   /// How would you determine a surface's height at (x,?,z) ? 
   /// </summary>
   /// <param name="x"> left -- right terrain position </param>
   /// <param name="z"> forward -- backward terrain position</param>
   /// <returns> vertical height of surface containing position (x,z)</returns>
   public float surfaceHeight(int x, int z) {
      if (x < 0 || x > 511 || z < 0 || z > 511) return 0.0f;  // index valid ?
      return (float) terrainHeight[x,z]; }

   public override void  Draw(GameTime gameTime) {
    effect.VertexColorEnabled = true;
    effect.DirectionalLight0.DiffuseColor = stage.DiffuseLight;
    effect.AmbientLightColor = stage.AmbientLight;
    effect.DirectionalLight0.Direction = stage.LightDirection;
    effect.DirectionalLight0.Enabled = true;             
    effect.View = stage.View;
    effect.Projection = stage.Projection;
    effect.World = Matrix.Identity;
    foreach (EffectPass pass in effect.CurrentTechnique.Passes) {
        pass.Apply(); 
        display.SetVertexBuffer(vb);
        display.Indices = ib;
        // display.DrawIndexedPrimitives(PrimitiveType.TriangleList,0,0,nVertices,
           // 0, nIndices/3); // deprecated in MonoGames 3.5
        display.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, nIndices/3);
        }
    }
   }
}
