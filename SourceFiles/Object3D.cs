/*  
    Copyright (C) 2017 G. Michael Barnes
 
    The file Object3D.cs is part of AGMGSKv9 a port and update of AGXNASKv8 from
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
   /// Defines location and orientation.
   /// Object's orientation is a 4 by 4 XNA Matrix. 
   /// Object's location is Vector3 describing it position in the stage.
   /// Has good examples of C# Properties (Location, Orientation, Right, Up, and At).
   /// These properties show how the 4 by 4 XNA Matrix values are
   /// stored and what they represent.
   /// Properties Right, Up, and At get and set values in matrix orientation.
   /// Right, the object's local X axis, is the lateral unit vector.
   /// Up, the object's local Y axis, is the vertical unit vector.
   /// At, the object's local Z axis, is the forward unit vector.
   /// 
   /// 2/1/2016  last changed
   /// </summary>

public class Object3D  {
   private Model3D model;
   private string name;                // string identifier
   private Stage stage;                // framework stage object
   private Matrix orientation;         // object's orientation 
   private Vector3 scales;             // object's scale factors
   private float pitch, roll, yaw;     // changes in rotation
   private int step,  stepSize;        // values for stepping
   // object's BoundingSphere
   private Vector3 objectBoundingSphereCenter;    
   private float objectBoundingSphereRadius = 0.0f;
   private Matrix objectBoundingSphereWorld;    

   // constructors

   /// <summary>
   /// Object that places and orients itself.
   /// </summary>
   /// <param name="theStage"> the stage containing object </param> 
   /// <param name="aModel">how the object looks</param> 
   /// <param name="label"> name of object </param> 
   /// <param name="position"> position in stage </param> 
   /// <param name="orientAxis"> axis to orient on </param> 
   /// <param name="radians"> orientation rotation </param> 
   public Object3D(Stage theStage, Model3D aModel, string label, Vector3 position, 
      Vector3 orientAxis, float radians) { 
      scales = Vector3.One;  // no scaling of model
      stage = theStage;
      model = aModel;
      name = label;
      step = 1;
      stepSize = 10; 
      pitch = yaw = roll = 0.0f;
      orientation = Matrix.Identity;
      orientation *= Matrix.CreateFromAxisAngle(orientAxis, radians);
      orientation *= Matrix.CreateTranslation(position);
      scaleObjectBoundingSphere();
      }

   /// <summary>
   /// Object that places, orients, and scales itself.
   /// </summary>
   /// <param name="theStage"> the stage containing object </param> 
   /// <param name="aModel">how the object looks</param> 
   /// <param name="label"> name of object </param> 
   /// <param name="position"> position in stage </param> 
   /// <param name="orientAxis"> axis to orient on </param> 
   /// <param name="radians"> orientation rotation </param> 
   /// <param name="objectScales">re-scale Model3D </param>  
   public Object3D(Stage theStage, Model3D aModel, string label, Vector3 position, 
      Vector3 orientAxis, float radians, Vector3 objectScales) { 
      stage = theStage;
      name = label;
      scales = objectScales;
      model = aModel;
      step = 1;
      stepSize = 10;
      pitch = yaw = roll = 0.0f;
      orientation = Matrix.Identity;
      orientation *= Matrix.CreateScale(scales);
      orientation *= Matrix.CreateFromAxisAngle(orientAxis, radians);
      orientation *= Matrix.CreateTranslation(position);
      scaleObjectBoundingSphere();
      }

   // Properties

   public string Name {
      get { return name; }
      set { name = value; }
      }

   public Matrix ObjectBoundingSphereWorld {
      get { return objectBoundingSphereWorld; }}

	public float ObjectBoundingSphereRadius {
		get { return objectBoundingSphereRadius; }}

   public Matrix Orientation {
      get { return orientation; }
      set { orientation = value; }
      }

   public Vector3 Translation {
      get {return orientation.Translation; }
      set {orientation.Translation = value; }}

   public Vector3 Up {
      get { return orientation.Up; }
      set { orientation.Up = value; }}

   public Vector3 Down {
      get { return orientation.Down; }
      set { orientation.Down = value; } }

   public Vector3 Right {
      get { return orientation.Right; }
      set { orientation.Right = value; } }

   public Vector3 Left {
      get { return orientation.Left; }
      set { orientation.Left = value; }}

   public Vector3 Forward {
      get { return orientation.Forward; }
      set { orientation.Forward = value; } }

   public Vector3 Backward {
      get { return orientation.Backward; }
      set { orientation.Backward = value; } }  // was orientation.Forward ??

   public float Pitch  {
      get { return pitch; }
      set { pitch = value; }}

   public float Yaw {
      get { return yaw; }
      set { yaw = value; }}

   public float Roll {
      get { return roll; }
      set { roll = value; }}

   public int Step {
      get { return step; }
      set { step = value; }}

   public int StepSize {
      get { return stepSize; }
      set { stepSize = value; }}

   public void reset() {
      pitch = roll = yaw = 0;
      step = 0;
      }

   // Methods

   /// <summary>
   ///  Does the Object3D's new position collide with any Collidable Object3Ds ?
   /// </summary>
   /// <param name="position"> position Object3D wants to move to </param>
   /// <returns> true when there is a collision </returns>
   private bool collision(Vector3 position) {
		float distance;
		Vector2 pos2D = new Vector2(position.X, position.Z);
		foreach (Object3D obj3d in stage.Collidable) {
			Vector2 obj2D = new Vector2(obj3d.Translation.X, obj3d.Translation.Z); 
			distance = Vector2.Distance(pos2D, obj2D);
			if (!this.Equals(obj3d) && 
				distance <= ObjectBoundingSphereRadius + obj3d.ObjectBoundingSphereRadius)  
				{
				stage.setInfo(18, 
					string.Format("Last Object3D.collison(): {0} BR = {1,5:f2} with {2} BR = {3,5:f2}, {4,5:f2} >= {5,5:f2} distance",
					Name, ObjectBoundingSphereRadius, obj3d.Name, obj3d.ObjectBoundingSphereRadius,
					ObjectBoundingSphereRadius + obj3d.ObjectBoundingSphereRadius, distance));
				return true; 
				}
			}
		return false;
		}
   
   // Only need to scale by x and z values
   private void scaleObjectBoundingSphere() {
     // if (scales.X >= scales.Y && scales.X >= scales.Z) 
	  if (scales.X >= scales.Z)
        objectBoundingSphereRadius = model.BoundingSphereRadius * scales.X;
     //else if (scales.Y >= scales.X && scales.Y >= scales.Z) 
     //   objectBoundingSphereRadius = model.BoundingSphereRadius * scales.Y;
     else objectBoundingSphereRadius = model.BoundingSphereRadius * scales.Z;
      }


   /// <summary>
   /// Update the object's orientation matrix so that it is rotated to 
   /// look at target. AGXNASK is terrain based -- so all turn are wrt flat XZ plane.
   /// AGXNASK assumes models are made to "look" -Z 
	///    toObj is a vector to this Object3D
	///    toTarget is a vector to the target (nextGoal on path)
	///    axis is the vector to rotate about (positive Y)
   /// </summary>
   /// <param name="target"> to look at</param>
   public void turnToFace(Vector3 target) {
      Vector3 axis, toTarget, toObj;
      double radian, aCosDot;
      // put both vector on the XZ plane of Y == 0
      toObj = new Vector3(Translation.X, 0, Translation.Z);
      target = new Vector3(target.X, 0, target.Z);
		toTarget = target - toObj; 
      toTarget.Normalize();
		// Dot not defined for co-linear vectors:  test toTarget and Forward
		// if vectors are identical (w/in epsilon 0.05) return, no need to turnToFace
		if (Vector3.Distance(toTarget, Forward) <= 0.05 ) return;
		// if vectors are reversed (w/in epsilon 0.05) nudgle alittle
      if (Vector3.Distance(Vector3.Negate(toTarget), Forward) <= 0.05) {
         toTarget.X += 0.05f;
         toTarget.Z += 0.05f;
         toTarget.Normalize();
         } 
      // determine axis for rotation
		axis = Vector3.Cross(toTarget, Forward);  // order of arguments maters
      axis.Normalize();
      // get cosine of rotation
		aCosDot = Math.Acos(Vector3.Dot(toTarget, Forward));
      // test and adjust direction of rotation into radians
      if (aCosDot == 0) radian = Math.PI*2;    
      else if (aCosDot == Math.PI) radian = Math.PI;
		else if (axis.X + axis.Y + axis.Z < 0) radian = (float)(2 * Math.PI - aCosDot);
      else radian = -aCosDot; 
      // stage.setInfo(19, string.Format("radian to rotate = {0,5:f2}, axis for rotation ({1,5:f2}, {2,5:f2}, {3,5:f2})",
      //   radian, axis.X, axis.Y, axis.Z));
      if (Double.IsNaN(radian)) {  // validity check, this should not happen
            stage.setInfo(19, "error:  Object3D.turnToFace() radian is NaN");
				// stage.Trace = string.Format("error:  Object3D.turnToFace() radian is NaN, aCosDot = {0}, axis direction = {1}, toTarget ({2}, {3}, {4}), forward ({5}, {6}, {7})",
				//		aCosDot, toTarget.X, toTarget.Y, toTarget.Z, Forward.X, Forward.Y, Forward.Z);
            return;
           }
         else {  // valid radian perform transformation
            // save location, translate to origin, rotate, translate back to location
            Vector3 objectLocation = Translation;
            orientation *= Matrix.CreateTranslation(-1 * objectLocation);
            // all terrain rotations are really on Y
            orientation *= Matrix.CreateFromAxisAngle(axis, (float) radian);
            orientation.Up = Vector3.Up;  // correct for flipped from negative axis of rotation
            orientation *= Matrix.CreateTranslation(objectLocation);
            }
      }

	/// <summary>
	/// Rotate 1 degree towards the target
	/// Not sure if this should be kept, still a jittery solution.
	/// </summary>
	/// <param name="target"> direction to turn towards </param>
	public void turnTowards(Vector3 target) {
      Vector3 toTarget, toObj;
      toObj = new Vector3(Translation.X, 0, Translation.Z);
      target = new Vector3(target.X, 0, target.Z);
		toTarget = target - toObj; 
      toTarget.Normalize();
		float radian;
		double leftVsRight = Vector3.Distance(Left, toTarget) - Vector3.Distance(Right, toTarget);
		//double frontVsBack = Vector3.Distance(Backward, toTarget) - Vector3.Distance(Forward, toTarget);
		//if ( Math.abs(leftVsRight) < 1.0f && Math.Abs(frontVsBack) < 1.0f) radian = 0.0f;  // looking at target
		//else 
		if (leftVsRight < 0.05f ) radian = MathHelper.ToRadians(0.50f);
		else  if (leftVsRight > 0.05f) radian = MathHelper.ToRadians(-.50f);
		else { 
			turnToFace(target); // set straight
			return; }
		// rotate towards target
      Vector3 objectLocation = Translation;
      orientation *= Matrix.CreateTranslation(-1 * objectLocation);
		orientation *= Matrix.CreateRotationY(radian);    
      orientation *= Matrix.CreateTranslation(objectLocation);     
		}	

   /// <summary>
   /// The location is first saved and the model is translated
   /// to the origin before any rotations are applied.  Objects rotate about their
   /// center.  After rotations, the location is reset and updated iff it is not
   /// outside the range of the stage (stage.withinRange(String name, Vector3 location)).  
   /// When movement would exceed the stage's boundaries the model is not moved 
   /// and a message is displayed.
   /// 
   /// AGMGSK is a terrain based 3D environment, so rotations are on Y.
   /// Therefore an Euler rotation method is adopted below.  MonoGames supports both
   /// Matrix.CreateFromAxisAngle(...) and CreateFromQuaternion(...) rotations 
   /// for more complex 3D environments.
   /// </summary>
   public void updateMovableObject() {

      Vector3 startLocation, stopLocation;
      startLocation = stopLocation = Translation;  // set for current location
      Orientation *= Matrix.CreateTranslation(-1 * Translation);  // move to origin
      // Euler rotations
      Orientation *= Matrix.CreateRotationY(yaw);                 // rotate
      Orientation *= Matrix.CreateRotationX(pitch);
      Orientation *= Matrix.CreateRotationZ(roll);
      stopLocation += ((step * stepSize) * Forward);              // move forward    
      // assume no collision possible if moving off terrain
      if (stage.withinRange(this.Name, stopLocation))  // on terrain, check collision
            if (model.IsCollidable && collision(stopLocation)) // collision
                  Orientation *= Matrix.CreateTranslation(startLocation); // restore position  
               else // no collision and on suface move
                  Orientation *= Matrix.CreateTranslation(stopLocation);  // move forward
         else // off terrain, don't move
            Orientation *= Matrix.CreateTranslation(startLocation);  // restore position       
      }

	/// <summary>
	/// Update the scale matrix for the object's bounding sphere
	/// </summary>
	/// <param name="modelRadius"></param>
   public void updateBoundingSphere() {
      objectBoundingSphereCenter = Translation;  // set center to instance
		objectBoundingSphereWorld = Matrix.CreateScale(objectBoundingSphereRadius * 2);
      objectBoundingSphereWorld *= Matrix.CreateTranslation(objectBoundingSphereCenter);
      }

   }
}
