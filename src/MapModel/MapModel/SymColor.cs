/* Copyright (c) 2006-2008, Peter Golde
 * All rights reserved.
 * 
 * Redistribution and use in source and binary forms, with or without 
 * modification, are permitted provided that the following conditions are 
 * met:
 * 
 * 1. Redistributions of source code must retain the above copyright
 * notice, this list of conditions and the following disclaimer.
 * 
 * 2. Redistributions in binary form must reproduce the above copyright
 * notice, this list of conditions and the following disclaimer in the
 * documentation and/or other materials provided with the distribution.
 * 
 * 3. Neither the name of Peter Golde, nor "Purple Pen", nor the names
 * of its contributors may be used to endorse or promote products
 * derived from this software without specific prior written permission.
 * 
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND
 * CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES,
 * INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF
 * MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
 * DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR
 * CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
 * SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING,
 * BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
 * SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS
 * INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY,
 * WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
 * NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE
 * USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY
 * OF SUCH DAMAGE.
 */

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing;

namespace PurplePen.MapModel
{
    using PurplePen.Graphics2D;

    // Which layer is this in. Only the "Normal" layer has a color and so forth. The
    // Layout layer is above all normal layers, and the Image layer is below.
    public enum SymLayer
    {
        Layout, Normal, Image
    }

    public class SymColor
    {
        // For OCAD and PDF purposes, we save the CMYK instead of the RGB.
        private SymLayer layer;
        private CmykColor cmykColor;
        private bool overprint;
        private string name;
        private Map map;
		private short ocadId;
        private Ink spotInk;                                        // The spot ink that this palette color is, or null.
        private ReadOnlyCollection<InkComponent> spotComposition;   // The spot inks of this palette color, or null.

        public string Name { get { return name; } set {this.name = value; }}
        public Map ContainingMap { get { return map; }}

        public SymColor(SymLayer layer)
        {
            this.layer = layer;
        }

        // Make a color of the normal layer, with no spot definition.
        public SymColor(string name, short ocadId, float cyan, float magenta, float yellow, float black, bool overprint)
            : this(SymLayer.Normal)
        {
            this.name = name;
            this.ocadId = ocadId;
            this.cmykColor = CmykColor.FromCmyk(cyan, magenta, yellow, black);
            this.overprint = overprint;
        }

        public SymLayer Layer
        {
            get { return layer; }
        }

        // Does this SymColor denote a special layer -- either the layout layer or the image layer?
        public bool IsSpecialLayer
        {
            get { return (layer != SymLayer.Normal); }
        }

        // The RawColorValue property returns the raw color map, untransformed by the containing map.
        public CmykColor RawColorValue { 
            get {
                Debug.Assert(!IsSpecialLayer);
                return cmykColor;
            } 
        }

        // The ColorValue property returns the color as possibly transformed by the containing map.
        public CmykColor ColorValue { 
            get {
                if (map == null)
                    return RawColorValue;
                else
                    return map.TransformColor(RawColorValue);
            } 
        }

        public object GetBrushKey(IGraphicsTarget g)
        {
            if (!g.HasBrush(this))
                g.CreateSolidBrush(this, ColorValue);
            return this;
        }

		public short OcadId {
			get { return ocadId; }
			set { 
				Debug.Assert(map == null, "Cannot change color after being attached to a map");
				ocadId = value;
			}
		}

		// Copies everything but the containing map from another color.
		public SymColor CopyFrom(SymColor other) {
            this.layer = other.layer;
            this.cmykColor = other.cmykColor;
            this.overprint = other.overprint;
			this.name = other.name;
			this.ocadId = other.ocadId;
            this.spotInk = other.spotInk;
            this.spotComposition = other.spotComposition;
            return this;
		}

        // Update internal values. Only for use by Map.ChangeColorAtIndex
        internal void Update(string name, short ocadId, float cyan, float magenta, float yellow, float black, bool overprint)
        {
			this.name = name;
			this.ocadId = ocadId;
            this.overprint = overprint;
            this.cmykColor = CmykColor.FromCmyk(cyan, magenta, yellow, black);
        }

        public void FreeGdiObjects()
        {
        }

        internal void SetMap(Map newMap)
        {
            if (map != null && newMap != null && map != newMap)
                throw new MapUsageException("Color can't be added to a map; it is already part of another map.");
            map = newMap;
        }

        public void SetCMYK(float cyan, float magenta, float yellow, float black)
        {
			Debug.Assert(map == null, "Cannot change color after being attached to a map");

            Debug.Assert(!IsSpecialLayer);
            this.cmykColor = CmykColor.FromCmyk(cyan, magenta, yellow, black);
        }

		public void GetCMYK(out float cyan, out float magenta, out float yellow, out float black) {
			cyan = cmykColor.Cyan;
			magenta = cmykColor.Magenta;
			yellow = cmykColor.Yellow;
            black = cmykColor.Black;
		}

		public void SetRGB(float red, float green, float blue) {
			Debug.Assert(map == null, "Cannot change color after being attached to a map");

            Debug.Assert(!IsSpecialLayer);
            float cyan, magenta, yellow, black;
            ColorConverter.RgbToCmyk(red, green, blue, out cyan, out magenta, out yellow, out black);

			SetCMYK(cyan, magenta, yellow, black);

#if DEBUG
            Color color = ColorConverter.ToColor(RawColorValue);
			Debug.Assert(color.R == (int)Math.Round(red * 255.0));
			Debug.Assert(color.G == (int)Math.Round(green * 255.0));
			Debug.Assert(color.B == (int)Math.Round(blue * 255.0));
#endif
		}

        public CmykColor CmykColor
        {
            get
            {
                Debug.Assert(!IsSpecialLayer);
                return cmykColor;
            }
        }

        public bool OverPrint
        {
            get { return overprint; }
            set
            {
                Debug.Assert(map == null, "Cannot change overprint after being attached to a map");
                overprint = value;
            }
        }

        // The inks of this palette color, each with its factor. A palette color without a spot definition is made of
        // the four process inks, with its CMYK values as their factors. A palette color that is a spot ink as such is
        // that ink at full tone. An ink that is not in the list is not a component of the palette color, which is not
        // the same as a component with the factor 0. A special layer has no inks.
        public IReadOnlyList<InkComponent> InkComponents
        {
            get
            {
                if (spotInk != null)
                    return Array.AsReadOnly(new InkComponent[] { new InkComponent(spotInk, 1) });
                else if (spotComposition != null)
                    return spotComposition;
                else if (IsSpecialLayer)
                    return Array.AsReadOnly(new InkComponent[0]);
                else
                    return Array.AsReadOnly(new InkComponent[] {
                        new InkComponent(Ink.Cyan, cmykColor.Cyan), new InkComponent(Ink.Magenta, cmykColor.Magenta),
                        new InkComponent(Ink.Yellow, cmykColor.Yellow), new InkComponent(Ink.Black, cmykColor.Black) });
            }
        }

        // Does this palette color have a spot definition: is it a spot ink as such, or made of spot inks? A palette
        // color without one is made of the process inks.
        public bool HasSpotDefinition
        {
            get { return spotInk != null || spotComposition != null; }
        }

        // The spot ink that this palette color is as such, at full tone, or null if it is not one.
        public Ink SpotInk
        {
            get { return spotInk; }
        }

        // Makes this palette color the given spot ink as such, at full tone. Null leaves it without a spot definition.
        public void SetSpotInk(Ink ink)
        {
            Debug.Assert(map == null, "Cannot change inks after being attached to a map");
            Debug.Assert(!IsSpecialLayer);
            if (ink != null && ink.IsProcessInk)
                throw new ArgumentException("A spot definition is made of spot inks.", "ink");

            spotInk = ink;
            spotComposition = null;
        }

        // Makes this palette color a composition of the given spot ink components. An ink that is in the list more than
        // once keeps its first factor. Null or an empty list leaves the palette color without a spot definition.
        public void SetSpotComposition(IEnumerable<InkComponent> components)
        {
            Debug.Assert(map == null, "Cannot change inks after being attached to a map");
            Debug.Assert(!IsSpecialLayer);

            List<InkComponent> distinctComponents = new List<InkComponent>();
            if (components != null) {
                foreach (InkComponent component in components) {
                    if (component.Ink == null || component.Ink.IsProcessInk)
                        throw new ArgumentException("A spot definition is made of spot inks.", "components");
                    if (!distinctComponents.Exists(existing => existing.Ink == component.Ink))
                        distinctComponents.Add(component);
                }
            }

            spotInk = null;
            spotComposition = (distinctComponents.Count > 0) ? distinctComponents.AsReadOnly() : null;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
