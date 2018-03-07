/*  
    Copyright (C) 2017 G. Michael Barnes
 
    The file MovableModel3D.cs is part of AGMGSKv9 a port and update of AGXNASKv8 from
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
/// Defines the Update(GameTime) method for moving an instance of a model.
/// Model instances are Object3Ds.
/// Movements: 
///   step (forward), stepSize, vertical (+ up, - down), 
///   yaw, pitch, and roll.
/// While abstract, subclasses invoke their base.Update(GameTime) to apply
/// the inherited movement step values.
/// 
/// 1/25/2012 last changed
/// </summary>

public class MovableModel3D : Model3D {
	protected Random random = null;
	/// <summary>
	/// Constructor is a "pass-through" for arguments to Model3D's constructor
	/// </summary>
	/// <param name="theStage"></param>
	/// <param name="label"></param>
	/// <param name="meshFile"></param>
	public MovableModel3D(Stage theStage, string label, string meshFile)    
		: base(theStage, label, meshFile) { }     

   public void reset() { foreach (Object3D obj in instance) obj.reset(); }

   ///<summary>
   ///  Update each object instance and invoke super Update()
   ///</summary>
   // override virtual DrawableGameComponent methods                   
   public override void Update(GameTime gameTime) {
      foreach (Object3D obj in instance) obj.updateBoundingSphere(); 
      base.Update(gameTime);
      }

   }
}
