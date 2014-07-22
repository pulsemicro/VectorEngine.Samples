using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pulse.VectorEngine;

namespace DistressSample
{
	class Program
	{
		static void Main(string[] args)
		{
			VectorDesign design = VectorDesign.Load(@"DistressDesign.PV", VectorDesignFileTypes.Pv);

			// Adding distress to a design is simply the following:
			// 1. Create a distress pattern. This can be a bitmap or vector design. 
			//    It is simply white "random" objects on a transparent background.
			// 2. Merge the distress pattern into your existing design.
			//    Some scaling of the distress pattern may be necessary to make it look good.
			// 3. Rendering the merged design.
			//
			// Assumption: the distress pattern is white. This assumes that it will be placed on a white background.
			// If this is not the case, then you should colourize the distress to match the background or use a different distress
			// pattern.

			var designBounds = design.GetBounds();

			{
				VectorDesign distress = VectorDesign.Load(@"DistressPattern.PV", VectorDesignFileTypes.Pv);
				var distressBounds = distress.GetBounds();

				// Move the distress so it's center is at the same location as the design's center
				var translate = Matrix3x2.Translate(designBounds.Center.X - distressBounds.Center.X, designBounds.Center.Y - distressBounds.Center.Y);
				distress.ApplyTransform(translate);

				// Depending on the distress, you may need to scale it.
				// Calculate an appropriate scaling factor based on your use case.
				//var scale = Matrix3x2.Scale(...);
				//distress.ApplyTransform(scale);

				var distressFirstPage = distress.Pages.First();
				var distressFirstLayer = distressFirstPage.Layers.First();

				// Create a new group object for the distress pattern so we can clip it
				GroupObject distressGroup = new GroupObject();

				foreach (var obj in distressFirstLayer.Objects)
				{
					distressGroup.AddObject(obj);
				}

				// Optional: Clip the distress to the bounding box of the design
				ClipObjectTo(distressGroup, designBounds);

				// Create a new layer for the distress and add the group object to it.
				// Finally, add that new layer to the first page of our design.
				Layer distressLayer = new Layer()
				{
					Name = "Distress" // Not required
				};
				distressLayer.AddObject(distressGroup);
				design.Pages.First().AddLayer(distressLayer);
			}

			// Render on a white background
			design.Render(@"DistressDesign.PNG", RenderFormats.Png, 96, design.ColourContext, new RGBColour(1.0, 1.0, 1.0), 1.0);
		}

		static void ClipObjectTo(GroupObject groupObject, Bounds boundingBox)
		{
			List<PathTriple> triples = new List<PathTriple>()
				{
					new PathTriple(boundingBox.TopLeft),
					new PathTriple(boundingBox.TopRight),
					new PathTriple(boundingBox.BottomRight),
					new PathTriple(boundingBox.BottomLeft)
				};
			PathShape clipPathShape = new PathShape();
			clipPathShape.AddPath(new Path(triples)
			{
				IsClosed = true
			});
			// clipPathShape is now a rectangle with coordinates matching our bounding box.

			// We turn that into a ShapeObject
			ShapeObject clipShape = new ShapeObject();
			clipShape.Shape = clipPathShape;

			// Add it to the group object (at the front) and turn on clipping.
			groupObject.IsClipped = true;
			groupObject.AddObjectAtFront(clipShape);
		}
	}
}
