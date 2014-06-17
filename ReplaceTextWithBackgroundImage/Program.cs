using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pulse.VectorEngine;

namespace ReplaceTextWithBackgroundImage
{
	class Program
	{
		// This sample will update a text object and resize a bitmap object that is
		// being used to "fill" the text.
		// If you do not resize the bitmap object, then if the text gets longer, then it may overrun the edge of the bitmap.
		//
		// However, if the text height stays the same (which will be most cases), then personalizing the text with longer text
		// will result in a wider aspect ratio of the visible portion of the bitmap. So while the same amount of the 
		// bitmap is visible in the X direction, less will be visible in the Y direction.
		static void Main(string[] args)
		{
			VectorDesign design = VectorDesign.Load(@"ReplaceTextWithBackgroundImage.PV", VectorDesignFileTypes.Pv);

			// Render before any changes simply for comparison
			design.Render(@"ReplaceTextWithBackgroundImage-Original.PNG", RenderFormats.Png, 96, design.ColourContext);

			BitmapObject backgroundObj = null;
			
			// Find the background image
			design.ProcessObjects(obj =>
				{
					if (obj.Name == "Text1Background" &&
						obj is BitmapObject)
					{
						backgroundObj = (BitmapObject)obj;
					}
					return obj;
				});
			System.Diagnostics.Debug.Assert(backgroundObj != null);

			Bounds textBoundsBefore = null;
			Bounds textBoundsAfter = null;
			Point textAnchor = null;
			
			// VectorDesign.ProcessTextObjects will iterate through the entire design to 
			// find all Pulse.VectorEngine.TextObject objects and call the supplied delegate
			// passing in the text object as the single parameter.
			design.ProcessTextObjects((obj) =>
			{
				if (obj.Name == "Text1")
				{
					// We "know" the position is anchor based because we're in a sandbox sample.
					AnchorPosition position = obj.Position as AnchorPosition;
					System.Diagnostics.Debug.Assert(position != null);

					// Keep this for later. We'll use it as the reference point of the scaling.
					textAnchor = position.Location;

					// Save the text bounds before and after the personalization
					textBoundsBefore = obj.GetBounds(design.Context);
					obj.Text = "Longer Text";
					textBoundsAfter = obj.GetBounds(design.Context);
				}
			});

			System.Diagnostics.Debug.Assert(textBoundsBefore != null);
			System.Diagnostics.Debug.Assert(textBoundsAfter != null);

			// What is the size difference between the original and the new text?
			double scaleX = textBoundsAfter.Width / textBoundsBefore.Width;
			double scaleY = textBoundsAfter.Height / textBoundsBefore.Height;

			// Use the bigger of the 2 scales
			double scale = scaleX;
			if (scaleY > scaleX)
				scale = scaleY;

			// Scale the bitmap image by the same amount
			backgroundObj.ApplyTransform(Matrix3x2.Scale(scale, scale, textAnchor));

			// Render the final result.
			design.Render(@"ReplaceTextWithBackgroundImage-Final.PNG", RenderFormats.Png, 96, design.ColourContext);
		}
	}
}
