using System;
using System.Collections;
using System.Windows.Forms.Design;

namespace MetroFramework.Design.Controls
{
	internal class MetroProgressBarDesigner : ControlDesigner
	{
		public override System.Windows.Forms.Design.SelectionRules SelectionRules
		{
			get
			{
				return base.SelectionRules;
			}
		}

		public MetroProgressBarDesigner()
		{
		}

		protected override void PreFilterProperties(IDictionary properties)
		{
			properties.Remove("ImeMode");
			properties.Remove("Padding");
			properties.Remove("FlatAppearance");
			properties.Remove("FlatStyle");
			properties.Remove("AutoEllipsis");
			properties.Remove("UseCompatibleTextRendering");
			properties.Remove("Image");
			properties.Remove("ImageAlign");
			properties.Remove("ImageIndex");
			properties.Remove("ImageKey");
			properties.Remove("ImageList");
			properties.Remove("TextImageRelation");
			properties.Remove("BackColor");
			properties.Remove("BackgroundImage");
			properties.Remove("BackgroundImageLayout");
			properties.Remove("UseVisualStyleBackColor");
			properties.Remove("Font");
			properties.Remove("ForeColor");
			properties.Remove("RightToLeft");
			properties.Remove("Text");
			base.PreFilterProperties(properties);
		}
	}
}