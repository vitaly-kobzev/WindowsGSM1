using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using WindowsGSM1.Postprocessing.BlurEffects;

namespace WindowsGSM1.Postprocessing
{
	internal class GraphicalPostprocessor
	{
		public BloomComponent Bloom { get; private set; }

		public GraphicalPostprocessor(bool enablePostprocessing)
		{
			PostProcessingEnabled = enablePostprocessing;
		}

		public void Initialize(Game game)
		{
			Bloom = new BloomComponent(game);
			Bloom.Settings = BloomSettings.PresetSettings[0];

			if(PostProcessingEnabled)
				game.Components.Add(Bloom);
		}

		public bool PostProcessingEnabled { get; private set; }

		public void BeginDraw()
		{
			if(PostProcessingEnabled)
				Bloom.BeginDraw();
		}
	}
}
