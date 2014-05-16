using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pulse.VectorEngine;

namespace ChangeColourProfile
{
	class Program
	{
		static void Main(string[] args)
		{
			VectorDesign design = VectorDesign.Load(@"ColourProfile1.PV", VectorDesignFileTypes.Pv);

			// First render using default colour profiles
			design.Render(@"ChangeColourProfile-Before.PNG", RenderFormats.Png, 96, design.ColourContext);

			// UncoatedFOGRA29.icc is included in this sample as per http://www.adobe.com/support/downloads/detail.jsp?ftpID=3682

			// Set the CMYK colour profile to be Uncoated FOGRA29
			using (var fileStream = System.IO.File.OpenRead(@"UncoatedFOGRA29.icc"))
			{
				var resource = design.ResourceManager.AddColourProfileResource(fileStream);
				design.ColourContext.SetCmykProfile(resource);
			}

			design.Render(@"ChangeColourProfile-After.PNG", RenderFormats.Png, 96, design.ColourContext);

			// The results of the 2 PNGs is subtle, but it's present when compared side-by-side
		}
	}
}
