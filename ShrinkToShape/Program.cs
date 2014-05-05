using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pulse.VectorEngine;

namespace ShrinkToShape
{
	class Program
	{
		static void Main(string[] args)
		{
			VectorDesign design = VectorDesign.Load(@"ShrinkToFit.PV", VectorDesignFileTypes.Pv);

			// Let the engine iterate through the entire design applying Shrink to Fit logic
			// to all anchor-based text objects. It will look for shape objects of the same
			// name as the text object.

			ShrinkTextToFitEngine engine = new ShrinkTextToFitEngine()
			{
				ShrinkToFitType = ShrinkToFitTypes.TextSizeThenHorizontalScale,
				MinTextSizeScale = 0.75,
				MinHorizontalScale = 0.25,
				IsHideTargetObject = true,
				CenterInBounds = CenterInBoundsTypes.Both
			};

			engine.ShrinkAllTextObjectsToFit(design);

			design.Render(@"ShrinkToFit.PNG", RenderFormats.Png, 96, design.ColourContext);
		}
	}
}
