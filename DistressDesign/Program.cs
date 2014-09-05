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
			// Methods 1 & 2 use the exact same source distress pattern

			DistressMethod1();
			DistressMethod2();
		}

		private static void DistressMethod1()
		{
			VectorDesign design = VectorDesign.Load(@"DistressDesign.PV", VectorDesignFileTypes.Pv);
			design.Render(@"DistressDesign-Method1-Original.PNG", RenderFormats.Png, 96, design.ColourContext, new RGBColour(1.0, 1.0, 1.0), 1.0);

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
			design.Render(@"DistressDesign-Method1-Final.PNG", RenderFormats.Png, 96, design.ColourContext, new RGBColour(1.0, 1.0, 1.0), 1.0);
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

			// Add it to the group object and turn on clipping.
			groupObject.IsClipped = true;
			groupObject.AddObject(clipShape);
		}

		private static void DistressMethod2()
		{
			VectorDesign design = VectorDesign.Load(@"DistressDesign.PV", VectorDesignFileTypes.Pv);
			design.Render(@"DistressDesign-Method2-Original.PNG", RenderFormats.Png, 300, design.ColourContext);

			// Distress method #2 is similar to method #1, 
			// however, in this case:
			// 1. Create a distress pattern. This must be a vector design.
			//    It is simply white "random" objects on a transparent background.
			// 2. Combine the distress objects with a bounding rectangle to "invert" them from
			//    a bunch of filled objects, to a bunch of holes in a rectangle.
			// 3. Use the new distress object as a clipping mask over the design.
			//    Some scaling of the distress pattern may be necessary to make it look good.
			// 4. Rendering the merged design.
			//
			// Unlike method #1, this method will work when rendering onto a transparent background.
			
			var designBounds = design.GetBounds();

			
			VectorDesign distress = VectorDesign.Load(@"DistressPattern.PV", VectorDesignFileTypes.Pv);
			var distressBounds = distress.GetBounds();

			// Create a new rectangle shape into which we'll add the distress shapes as "holes".
			Path rectangle = new Path();
			rectangle.Triples.Add(new PathTriple(distressBounds.TopLeft));
			rectangle.Triples.Add(new PathTriple(distressBounds.TopRight));
			rectangle.Triples.Add(new PathTriple(distressBounds.BottomRight));
			rectangle.Triples.Add(new PathTriple(distressBounds.BottomLeft));
			rectangle.IsClosed = true;

			var distressShape = new PathShape();
			distressShape.AddPath(rectangle);

			// Take all the objects from the distress design
			// and copy them to our PathShape instead.
			var distressFirstPage = distress.Pages.First();
			var distressFirstLayer = distressFirstPage.Layers.First();

			foreach (var shapeObject in distress.ShapeObjects)
			{
				PathShape pathShape = shapeObject.Shape as PathShape;
				if (pathShape != null)
				{
					distressShape.AddPaths(pathShape.Paths);
				}
			}

			// At this point, distressShape should be a rectangle with a bunch of "holes" in it.

			// Move the distress so it's center is at the same location as the design's center
			var translate = Matrix3x2.Translate(designBounds.Center.X - distressBounds.Center.X, designBounds.Center.Y - distressBounds.Center.Y);
			distressShape.ApplyTransform(translate);

			// Depending on the distress, you may need to scale it.
			// Calculate an appropriate scaling factor based on your use case.
			//var scale = Matrix3x2.Scale(...);
			//distressShape.ApplyTransform(scale);

			// This will be our clipping mask
			ShapeObject distressObject = new ShapeObject();
			distressObject.Shape = distressShape;
			//distressObject.Fill = new SolidFill(new RGBColour(0, 0, 0));

			// Create a new design to act as our resulting "distressed" design
			VectorDesign result = new VectorDesign();
			result.ColourContext = design.ColourContext;
			design.ResourceManager.CopyContentsTo(result.ResourceManager);
			var resultPage = result.NewPage();
			var resultLayer = resultPage.NewLayer();

			// Copy all items from the source design to our new group object
			GroupObject newGroup = new GroupObject();

			design.ProcessObjects(obj => 
				{
					newGroup.AddObject(obj);
					return obj;
				});

			// Add the clipping mask to the group as our clipping mask
			newGroup.AddObject(distressObject);
			newGroup.IsClipped = true;
			
			resultLayer.AddObject(newGroup);
			

			// Render on a white background
			result.Render(@"DistressDesign-Method2-Final.PNG", RenderFormats.Png, 300, design.ColourContext);
		}
	}
}
