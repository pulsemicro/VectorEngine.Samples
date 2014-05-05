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
			VectorDesign design = VectorDesign.Load(@"ReplaceMascot.PV", VectorDesignFileTypes.Pv);

			// Replace a group object with objects from another PV file

			// VectorDesign.ProcessObjects with iterate through all objects of an entire design
			// calling the supplied delegate passing each object as the single parameter.
			// The return value is the new (or old) object to use to replace the pre-existing one.
			design.ProcessObjects((obj) =>
			{
				if (obj is GroupObject)
				{
					GroupObject g = (GroupObject)obj;
					if (g.Name == "Mascot")
					{
						// Get the current bounding box of this object (we'll want it later)
						var oldBounds = g.GetBounds(design.Context);

						VectorDesign design2 = VectorDesign.Load(@"Mascot.PV", VectorDesignFileTypes.Pv);

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

							// TODO: Copy design resources (such as bitmaps used by the objects)
						}

						// Get the bounding box of the new object
						var newBounds = g.GetBounds(design.Context);

						// Move the new object so that it's center is where the center
						// of the old object was
						var transform = Matrix3x2.Translate(oldBounds.Center.X - newBounds.Center.X, oldBounds.Center.Y - newBounds.Center.Y);
						g.ApplyTransform(transform);

						// We want to replace the old object with our new one
						obj = g;
					}
				}
				return obj;
			});

			design.Render(@"ReplaceMascot.PNG", RenderFormats.Png, 96, design.ColourContext);

		}
	}
}
