/*  
    Copyright (C) 2017 G. Michael Barnes
 
    The file Path.cs is part of AGMGSKv9 a port and update of AGXNASKv8 from
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
/// Path represents a collection of NavNodes that a movable Object3D can traverse.
/// Paths have a PathType of
/// <list type="number"> SINGLE, traverse list once, set done to true </list>
/// <list type="number"> REVERSE, loop by reversing path after each traversal </list>
/// <list type=="number"> LOOP, loop by starting at paths first node again </list>
/// 
/// 12/30/2014 last changed
/// </summary>
public class Path : DrawableGameComponent {
   public enum PathType  {SINGLE, REVERSE, LOOP};
   private List<NavNode> node;
   private int nextNode;
   private PathType pathType;
   private bool done;
   private Stage stage;

   /// <summary>
   /// Create a path
   /// </summary>
   /// <param name="theStage"> "world's stage" </param>
   /// <param name="apath"> collection of nodes in path</param>
   /// <param name="aPathType"> SINGLE, REVERSE, or LOOP path traversal</param>
   public Path(Stage theStage, List<NavNode> aPath, PathType aPathType) : base(theStage) {
      node = aPath;
      nextNode = 0;
      pathType = aPathType;
      stage = theStage;
      done = false;
      }

	/// <summary>
	/// Create a path from array
	/// </summary>
	/// <param name="theStage"> "world's stage" </param>
	/// <param name="apath"> collection of nodes in path</param>
	/// <param name="pathNode"> int[x,z] array of WayPoint positions for path</param>
	/// <param name="aPathType"> SINGLE, REVERSE, or LOOP path traversal</param>
	public Path(Stage theStage, int[,] pathNode, PathType aPathType)
		: base(theStage)	{
		nextNode = 0;
		pathType = aPathType;
		stage = theStage;
		done = false;
      int spacing = stage.Spacing;
		int x, z;
      // make a simple path, show how to set the type of the NavNode outside of construction.
		node = new List<NavNode>();
		for(int i = 0; i < pathNode.Length/2; i++) {
			x = pathNode[i, 0];
			z = pathNode[i, 1];
			node.Add(new NavNode(new Vector3(x * spacing, stage.Terrain.surfaceHeight(x, z), z * spacing),
               NavNode.NavNodeEnum.WAYPOINT) ); 
			}
		}


   /// <summary>
   /// Create a path from XZ nodes defined in a pathFile.
   /// The file must be accessible from the executable environment.
   /// </summary>
   /// <param name="theStage"> "world's stage" </param>
   /// <param name="aPathType"> SINGLE, REVERSE, or LOOP path traversal</param>
   /// <param name="pathFile"> text file, each line a node of X Z values, separated by a single space </x></param>
   public Path(Stage theStage, PathType aPathType, string pathFile)  : base(theStage) {
      node = new List<NavNode>();
      stage = theStage;
      nextNode = 0;
      pathType = aPathType;
      done = false;
		// read file for WayPoint vertex (x,z) positions
		int spacing = stage.Spacing;
		int x, z;
		string line;
		string[] tokens;
      using (System.IO.StreamReader fileIn = System.IO.File.OpenText(pathFile)) {
         line = fileIn.ReadLine();
         do {
            tokens = line.Split(new char[] {});  // use default separators
            x = Int32.Parse(tokens[0]);  
            z = Int32.Parse(tokens[1]);
				node.Add(new NavNode(new Vector3(x * spacing, stage.Terrain.surfaceHeight(x, z), z * spacing), 
					NavNode.NavNodeEnum.PATH));  
            line = fileIn.ReadLine();
            } while (line != null);
         }
      }

   // Properties

   public int Count { get { return node.Count; }}   

   public bool Done { get { return done; }}

   /// <summary>
   /// Gets the next node in the path using path's PathType
   /// </summary>
   public NavNode NextNode {
      get {
         NavNode n = null;
         if (node.Count > 0 && node.Count - 1 > nextNode) { // take next step on path
            n = node[nextNode];
            nextNode++; }
         // at end of current path, decide what to do:  stop, reverse path, loop?
         else if (node.Count-1 == nextNode &&  pathType == PathType.SINGLE) {
               n = node[nextNode];
               done = true; }  
            else if (node.Count -1 == nextNode && pathType == PathType.REVERSE) {
               node.Reverse();
               nextNode = 0;  // set to next node
               n = node[nextNode];
               nextNode++; }
            else if (node.Count - 1 == nextNode && pathType == PathType.LOOP) {
               n = node[nextNode]; 
               nextNode = 0; }    
         return n; }
      }   


   // Methods

   public override void Draw(GameTime gameTime) {
      Matrix[] modelTransforms = new Matrix[stage.WayPoint3D.Bones.Count];
      foreach(NavNode navNode in node) {
         // draw the Path markers
            foreach (ModelMesh mesh in stage.WayPoint3D.Meshes) {
               stage.WayPoint3D.CopyAbsoluteBoneTransformsTo(modelTransforms);
               foreach (BasicEffect effect in mesh.Effects) {
                  effect.EnableDefaultLighting();
                  effect.DirectionalLight0.DiffuseColor = navNode.NodeColor;
                  effect.AmbientLightColor = navNode.NodeColor;
                  effect.DirectionalLight0.Direction = stage.LightDirection;
                  effect.DirectionalLight0.Enabled = true;
                  effect.View = stage.View;
                  effect.Projection = stage.Projection;
                  effect.World = Matrix.CreateTranslation(navNode.Translation) * modelTransforms[mesh.ParentBone.Index];
               }
               stage.setBlendingState(true);
               mesh.Draw();
               stage.setBlendingState(false);
            }
         }
      }
      
   }
}
