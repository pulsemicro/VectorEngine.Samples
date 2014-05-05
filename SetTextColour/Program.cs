using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pulse.VectorEngine;

namespace SetTextColour
{
	class Program
	{
		static void Main(string[] args)
		{
			VectorDesign design = VectorDesign.Load(@"SetTextColour.PV", VectorDesignFileTypes.Pv);

			// Assign a red fill to all text objects
			design.ProcessTextObjects((textObj) =>
			{
				textObj.Fill = new SolidFill(new RGBColour(255, 0, 0));
			});

			design.Render(@"SetTextColour.PNG", RenderFormats.Png, 96, design.ColourContext);

		}
	}
}
