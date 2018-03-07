/*  
    Copyright (C) 2017 G. Michael Barnes
 
    The file Cloud.cs is part of AGMGSKv9 a port and update of AGXNASKv8 from
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
/// An example of how to override the MovableModel3D's Update(GameTime) to 
/// animate a model's objects.  The actual update of values is done by calling 
/// each instance object and setting its (Pitch, Yaw, Roll, or Step property. 
/// Then call base.Update(GameTime) method of MovableModel3D to apply transformations.
/// 
/// 1/5/2014  last changed
/// </summary>
public class Cloud : MovableModel3D {
   // Constructor
   public Cloud(Stage stage, string label, string meshFile, int nClouds)
      : base(stage, label, meshFile)
      {
      random = new Random();
		// add nClouds random cloud instances
		for (int i = 0; i < nClouds; i++) {
			int x = (128 + random.Next(256)) * stage.Spacing;  // 128 .. 384
			int z = (128 + random.Next(256)) * stage.Spacing;
			addObject(
				new Vector3(x , stage.surfaceHeight(x, z)  + 4000, z ),
				new Vector3(0, 1, 0), (random.Next(5)) * 0.01f,
				new Vector3(4 + random.Next(3), random.Next(4) + 1, 3 + random.Next(2)));
				}    
      } 
  
   public override void Update(GameTime gameTime) {
      foreach (Object3D obj in instance) {
			obj.Step = 0;
			obj.Yaw = 0;
         if (random.Next(3) == 0) obj.Yaw = (random.Next(5) + 1) * 0.001f;
         obj.updateMovableObject();
         }
      base.Update(gameTime);
      }

   }}
