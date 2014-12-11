using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;
using Pulse.VectorEngine;

namespace ReplaceBackgroundImage
{
	class Program
	{
		static void Main(string[] args)
		{
			string mainDesign = "VIKINGS_COMOUFLAGE.pv";

			string backgroundDesign = "leopard.png";

			VectorDesign design = VectorDesign.Load(mainDesign, VectorDesignFileTypes.Pv);

			// Replace an image in a clipped group within a PV file with another image

			BitmapObject backgroundObj = null;

			// VectorDesign.ProcessObjects with iterate through all objects of an entire design
			// calling the supplied delegate passing each object as the single parameter.
			// The return value is the old image file provided the Name matches what is in the PV file
			// and the object found is a Bitmap Object.
			design.ProcessObjects(obj =>
			{
				if (obj.Name == "<Linked File>" &&
					obj is BitmapObject)
				{
					// Cast the obj to a BitmapObject as we will want information later from this
					backgroundObj = (BitmapObject)obj;

					// Read the new image file into a byte array
					var image = System.IO.File.ReadAllBytes(backgroundDesign);

					Pulse.VectorEngine.Resources.BitmapResource bitmapResource;
					// Read the image in the byte array into a Memory Stream
					using (var stream = new System.IO.MemoryStream(image))
					{
						// Add the stream into the design's resources as a Bitmap Resource
						bitmapResource = design.ResourceManager.AddBitmapResource(stream);	
					}

					// Create a new Bitmap Object
					// Set the BitmapResourceId to the newly created Bitmap Resource from above
					// Give the new object a name other than the original name in the PV file 
					// as the same name would cause the Processing of Objects to get caught in an endless loop
					// Set a file name
					// Set the Matrix of the new Bitmap Object to the Matrix of the original 
					// image as this will set the location of the new image to the old image
					BitmapObject bitmapObject = new BitmapObject();
					bitmapObject.BitmapResourceId = bitmapResource.Id;
					bitmapObject.Name = "leopard";
					bitmapObject.Filename = backgroundDesign;
					bitmapObject.Matrix = backgroundObj.Matrix;

					// We want to replace the old object with our new Bitmap Object
					obj = bitmapObject;
				}
				return obj;
			});
			System.Diagnostics.Debug.Assert(backgroundObj != null);

			design.Render(@"ReplaceBackgroundImage.PNG", RenderFormats.Png, 200, design.ColourContext);
		}
	}
}
