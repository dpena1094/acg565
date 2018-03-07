/*  
    Copyright (C) 2017 G. Michael Barnes
 
    The file IndexVertexBuffer.cs is part of AGMGSKv9 a port and update of AGXNASKv8 from
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
/// IndexVertexBuffers defines variables and properties shared
/// by all indexed-vertex meshes.
/// Since the vertex type can change, vertices should be defined
/// in subclasses.
/// </summary>
public abstract class IndexVertexBuffers : DrawableGameComponent {
   protected Stage stage;
   protected string name;
   protected int range, nVertices, nIndices;
   protected VertexBuffer vb = null;
   protected IndexBuffer ib = null;
   protected int[] indices;  // indexes for IndexBuffer -- define face vertice indexes clockwise 

   public IndexVertexBuffers(Stage theStage, string label) : base (theStage)  {  
      stage = theStage; 
      name = label;
      }

   // Properties

   public VertexBuffer VB {
      get { return vb; }
      set { vb = value; }
      }

   public IndexBuffer IB {
      get { return ib; }
      set { ib = value; }
      }
   }
}
