/*  
/*  
    Copyright (C) 2017 G. Michael Barnes
 
    The file Camera.cs is part of AGMGSKv9 a port and update of AGXNASKv8 from
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
/// A viewpoint in the stage.  Cameras have a viewMatrix that is its position and orientation 
/// in the stage.  There are four cameras:  TopDownCamera, FirstCamera, FollowCamera, AboveCamera.
/// The stage has one TopDownCamera named "Whole stage camera".  This is the starting camera
/// for SK565.  The TopDownCamera is stationary.  
/// The other three cameras are associated with an agent.  They are the agent's
/// first (person view), follow (person view), and above (look down on agent view).  
/// Player handles user input to call stage.nextCamera() to selects the currentCamera, 
/// update the agent's current avatarCamera, update currentCamera's viewMatrix 
/// with updateViewMatrix().
/// 
/// 1/20/2017 last changed
/// </summary>

public class Camera  {
   public enum CameraEnum { TopDownCamera, FirstCamera, FollowCamera, AboveCamera }
   Object3D agent;
   /// <summary>
   /// offset used to place first, follow, and above cameras.  
   /// Set to bounding radius of AgentObject
   /// </summary>
   private int terrainCenter, offset;
   private Matrix viewMatrix;
   private string name;
   private Stage scene;
   CameraEnum cameraCase;

   public Camera(Stage aScene, CameraEnum cameraType) { 
      name = "Whole stage";
      scene = aScene;
      cameraCase = cameraType;
      offset = 0;
      terrainCenter = scene.TerrainSize / 2;
      //updateViewMatrix();
      }

   public Camera(Stage aScene, Object3D anAgentObject, CameraEnum cameraType) {
      scene = aScene;
      agent = anAgentObject;
      cameraCase = cameraType;
      offset = (int) anAgentObject.ObjectBoundingSphereRadius;
      }
      
   // Properties

   public string Name {
      get { return name; }
      set { name = value; }}
         
   public Matrix ViewMatrix {
      get { return viewMatrix;}}

   /// <summary>
   /// When an agent updates its place in the stage it calls its currentCamera's
   /// updateViewMatrix() to place the camera's viewMatrix.
   /// For attached cameras offset is used to position the camera based on the bounding radius of the
   /// Object3D the camera is bound to.
   /// </summary>
   public void updateViewMatrix() { 
      switch (cameraCase) {
         case CameraEnum.TopDownCamera:
         viewMatrix = Matrix.CreateLookAt(
            new Vector3(terrainCenter, terrainCenter * 2, terrainCenter),
            new Vector3(terrainCenter, 0, terrainCenter), 
            new Vector3(0, 0, -1));
            break;
         case CameraEnum.FirstCamera:
            viewMatrix = Matrix.CreateLookAt(agent.Translation,
               agent.Translation + agent.Forward, agent.Orientation.Up);
            viewMatrix *= Matrix.CreateTranslation(0, -2.25f * offset , 0);  
            break;
         case CameraEnum.FollowCamera:
            viewMatrix = Matrix.CreateLookAt(agent.Translation,
               agent.Translation + agent.Forward, agent.Orientation.Up);
            viewMatrix *= Matrix.CreateTranslation(0, -4 * offset, -12 * offset);
            break;
         case CameraEnum.AboveCamera:
            viewMatrix = Matrix.CreateLookAt(
               new Vector3(agent.Translation.X, agent.Translation.Y + 10 * offset,
               agent.Translation.Z), 
               agent.Translation, new Vector3(0, 0, -1));
            break;
         }
      }           
   }
}
