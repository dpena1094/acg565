/*  
    Copyright (C) 2017 G. Michael Barnes
 
    The file Wall.cs is part of AGMGSKv9 a port and update of AGXNASKv8 from
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
/// A collection of brick.x Models. 
/// Used for path finding and obstacle avoidance algorithms.
/// You set the brick positions in the brick[,] array in initWall(...).
/// 
/// 2/14/2016 last changed
/// </summary>

public class Wall : Model3D {

public Wall(Stage theStage, string label, string meshFile)  : base(theStage, label, meshFile) {
	initWall(450, 460);  // origin of wall on terrain
	}	

public Wall(Stage theStage, string label, string meshFile, int xOffset, int zOffset)  
	: base(theStage, label, meshFile) 
	{ 	initWall(xOffset, zOffset); }

/// <summary>
/// Shared constructor intialization code.
/// </summary>
/// <param name="offsetX"> center brick's x position</param>
/// <param name="offsetZ"> center brick's z position</param>
private void initWall(int offsetX, int offsetZ) {
	isCollidable = true;
	int spacing = stage.Terrain.Spacing;
	Terrain terrain = stage.Terrain;
	int baseX = offsetX;
	int baseZ = offsetZ;
	// brick[x,z] vertex positions on terrain
	int[,] brick = { // "just another brick in the wall", Pink Floyd 
		{0, 0}, {1, 0}, {2, 0}, {3, 0}, {4, 0}, {5, 0}, {6, 0},  // 7 middle right
		{7, -9}, {7, -8}, {7, -7}, {7, -6}, {7, -5}, {7, -4}, {7, -3}, {7, -2}, {7, -1}, {7, 0}, 
			{7, 1}, {7, 2}, {7, 3}, {7, 4}, {7, 5}, {7, 6}, {7, 7}, {7, 8}, {7, 9}, {7, 10}, {7, 11}, // 21 down on right
		{-3, 14}, {-3, 13}, {-3, 12}, {-3, 11}, {-3, 10},  // 5 up on right
		{-3, 10}, {-4, 10}, {-5, 10}, {-6, 10}, {-7, 10}, {-8, 10}, {-9, 10}, {-10, 10}, // 8 middle left
		{-10, 10}, {-10, 9}, {-10, 8}, {-10, 7}, {-10, 6}, {-10, 5}, {-10, 4}, {-10, 3}, {-10, 2}, {-10, 1}, {-10, 0}, 
			{-10, -1}, {-10, -2}, {-10, -3}, {-10, -4}, {-10, -5}, {-10, -6}, {-10, -7}, {-10, -8}, {-10, -9}, {-10, -10}, // 21 up on left
		{-9, -10}, {-8, -10}, {-7, -10}, {-6, -10}, {-5, -10}, {-4, -10}, {-3, -10},  // 7 right
		{-3, -10}, {-3, -11}, {-3, -12}, {-3, -13}, {-3, -14}  // 5 up right
		};
	for (int i = 0; i < brick.GetLength(0); i++)
	{
		int xPos = brick[i, 0] + baseX;
		int zPos = brick[i, 1] + baseZ;
		addObject(new Vector3(xPos * spacing, terrain.surfaceHeight(xPos, zPos), zPos * spacing), Vector3.Up, 0.0f);
	}
}
	}
}