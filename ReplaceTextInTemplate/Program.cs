using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pulse.VectorEngine;

namespace ReplaceTextInTemplate
{
	class Program
	{
		static void Main(string[] args)
		{
			VectorDesign design = VectorDesign.Load(@"ReplaceTextInTemplate.PV", VectorDesignFileTypes.Pv);

			// VectorDesign.ProcessTextObjects will iterate through the entire design to 
			// find all Pulse.VectorEngine.TextObject objects and call the supplied delegate
			// passing in the text object as the single parameter.
			design.ProcessTextObjects((textObj) =>
			{
				if (textObj.Name == "Text1")
				{
					textObj.Text = "Replaced Text #1";
				}
				else if (textObj.Name == "Text2")
				{
					textObj.Text = "Replaced Text #2";
				}
			});

			design.Render(@"ReplaceTextInTemplate.PNG", RenderFormats.Png, 96, design.ColourContext);
		}
	}
}
