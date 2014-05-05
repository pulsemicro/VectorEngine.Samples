using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pulse.VectorEngine;

namespace SetPrimaryAndSecondaryColours
{
	class Program
	{
		static void Main(string[] args)
		{
			VectorDesign design = VectorDesign.Load(@"SetPrimaryAndSecondaryColours.PV", VectorDesignFileTypes.Pv);

			// Replace all spot colours called "Primary" with red, and "Secondary" with blue

			// VectorDesign.ProcessColours with iterate through all colours of all objects
			// in the design calling the supplied delegate passing each colour as the single parameter.
			// The return value is the new (or old) colour to use to replace the pre-exising one.
			design.ProcessColours((c) =>
			{
				if (c is SpotColour)
				{
					SpotColour s = (SpotColour)c;

					switch (s.Name)
					{
						case "Primary":
							c = new SpotColour(s.Name, s.Tint, new RGBColour(255, 0, 0));
							break;
						case "Secondary":
							c = new SpotColour(s.Name, s.Tint, new RGBColour(0, 0, 255));
							break;
						default:
							// Otherwise no change to colour
							break;
					}
				}

				return c;
			});

			design.Render(@"SetPrimaryAndSecondaryColours.PNG", RenderFormats.Png, 96, design.ColourContext);
		}
	}
}
