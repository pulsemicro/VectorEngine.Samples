using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pulse.VectorEngine;

namespace ReplaceMascot
{
	class Program
	{
		static void Main(string[] args)
		{
			string mainDesign = "BANNER HORNETS_withmascot.pv";

			string mascotDesign = "BANNER HORNET_HEAD_SIDE_HAPPY.pv";

			VectorDesign design = VectorDesign.Load(mainDesign, VectorDesignFileTypes.Pv);

			// Replace a group object with objects from another PV file

			// VectorDesign.ProcessObjects with iterate through all objects of an entire design
			// calling the supplied delegate passing each object as the single parameter.
			// The return value is the new (or old) object to use to replace the pre-existing one.
			design.ProcessObjects((obj) =>
			{
				if (obj is GroupObject)
				{
					GroupObject g = (GroupObject)obj;
					if (g.Name == "mascot")
					{
						// Get the current bounding box of this object (we'll want it later)
						var oldBounds = g.GetBounds(design.Context);

						VectorDesign design2 = VectorDesign.Load(mascotDesign, VectorDesignFileTypes.Pv);

						g = new GroupObject();
						g.Name = "Mascot";

						// Copy all objects from page 1 of design2 to the new object
						// Assumes a page exists, which may not be the case.
						var page = design2.Pages.First();
						foreach (var layer in page.Layers)
						{
							// Copy all the objects from the layer to our new group object
							// Note that this is a shallow copy, so the objects
							// technically exist in both designs.
							g.AddObjects(layer.Objects);
						}

						// Copy design resources (such as bitmaps used by the objects)
						var resourceManager2 = design2.ResourceManager;
						resourceManager2.CopyContentsTo(design.ResourceManager);

						// Get the bounding box of the new object
						var newBounds = g.GetBounds(design.Context);

						// Move the new object so that it's center is where the center
						// of the old object was

						// If we know the design name is NEW_MASCOT.pv
						// Then we can add or subtract POINTS to the newBounds.Center.X and/or the newBounds.Center.Y
						// double adjustednewCenterX = newBounds.Center.X;
						// double adjustednewCentery = newBounds.Center.Y;
						double adjustednewCenterX = newBounds.Center.X;
						double adjustednewCenterY = newBounds.Center.Y;

						if (mascotDesign == "BANNER HORNET_HEAD_SIDE_HAPPY.pv")
						{
							adjustednewCenterX = newBounds.Center.X + 10.0;
							adjustednewCenterY = newBounds.Center.Y - 10.0;
						}

						var transform = Matrix3x2.Translate(oldBounds.Center.X - adjustednewCenterX, oldBounds.Center.Y - adjustednewCenterY);
						g.ApplyTransform(transform);

						// Scale the new mascot so that the new mascot fits completely inside
						// the bounding box of the original mascot

						// Start by scaling in the Y direction so the height of the
						// new mascot will be the same as the height of the old mascot
						double scale = oldBounds.Height / newBounds.Height;
						
						// But will the new mascot fit in the X direction too?
						if (newBounds.Width * scale > oldBounds.Width)
						{
							// No it would not, so scale by the X direction instead.
							// This should make the width of the new mascot smaller than
							// the width of the old mascot
							scale = oldBounds.Width / newBounds.Width;

							System.Diagnostics.Debug.Assert(newBounds.Height * scale <= oldBounds.Height);
						}

						transform = Matrix3x2.Scale(scale, scale, oldBounds.Center);
						g.ApplyTransform(transform);

						// Verification
						newBounds = g.GetBounds(design.Context);
						//System.Diagnostics.Debug.Assert(newBounds.Center.X == oldBounds.Center.X);
						//System.Diagnostics.Debug.Assert(newBounds.Center.Y == oldBounds.Center.Y);
						//System.Diagnostics.Debug.Assert(newBounds.Width <= oldBounds.Width);
						//System.Diagnostics.Debug.Assert(newBounds.Height <= oldBounds.Height);

						// We want to replace the old object with our new one
						obj = g;
					}
				}
				return obj;
			});

			design.Render(@"ReplaceMascot.PNG", RenderFormats.Png, 200, design.ColourContext);

		}
	}
}
