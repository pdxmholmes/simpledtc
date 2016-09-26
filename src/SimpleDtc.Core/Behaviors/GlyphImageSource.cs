#region License

// Copyright (c) 2016, Matt Holmes
// All rights reserved.
// 
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are met:
//     * Redistributions of source code must retain the above copyright
//       notice, this list of conditions and the following disclaimer.
//     * Redistributions in binary form must reproduce the above copyright
//       notice, this list of conditions and the following disclaimer in the
//       documentation and/or other materials provided with the distribution.
//     * Neither the name of the project nor the
//       names of its contributors may be used to endorse or promote products
//       derived from this software without specific prior written permission.
// 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND 
// ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT  LIMITED TO, THE IMPLIED WARRANTIES 
// OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL 
// THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, 
// SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT  LIMITED TO, PROCUREMENT 
// OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) 
// HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR 
// TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, 
// EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

#endregion

using System.Windows;
using System.Windows.Controls.Ribbon;
using System.Windows.Interactivity;
using System.Windows.Media;

namespace SimpleDtc.Core.Behaviors {
    public enum GlyphImageSourceSize {
        Small,
        Large
    }

    public class GlyphImageSource : Behavior<RibbonMenuButton> {
        public static readonly DependencyProperty GlyphProperty = DependencyProperty.Register (
            "Glyph", typeof (string), typeof (GlyphImageSource), new FrameworkPropertyMetadata (default(string), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, PropertyChangedCallback));

        public static readonly DependencyProperty BrushProperty = DependencyProperty.Register (
            "Brush", typeof (Brush), typeof (GlyphImageSource), new PropertyMetadata (new SolidColorBrush (Color.FromRgb (0, 0, 0)), BrushChanged));

        public static readonly DependencyProperty DisabledBrushProperty = DependencyProperty.Register (
            "DisabledBrush", typeof (Brush), typeof (GlyphImageSource), new PropertyMetadata (new SolidColorBrush (Color.FromRgb (64, 64, 64)), BrushChanged));

        public static readonly DependencyProperty SizeProperty = DependencyProperty.Register (
            "Size", typeof (GlyphImageSourceSize), typeof (GlyphImageSource), new PropertyMetadata (GlyphImageSourceSize.Small, SizeChanged));

        public string Glyph {
            get { return (string) GetValue (GlyphProperty); }
            set { SetValue (GlyphProperty, value); }
        }

        public Brush Brush {
            get { return (Brush) GetValue (BrushProperty); }
            set { SetValue (BrushProperty, value); }
        }

        public Brush DisabledBrush {
            get { return (Brush) GetValue (DisabledBrushProperty); }
            set { SetValue (DisabledBrushProperty, value); }
        }

        public GlyphImageSourceSize Size {
            get { return (GlyphImageSourceSize) GetValue (SizeProperty); }
            set { SetValue (SizeProperty, value); }
        }

        protected override void OnAttached () {
            base.OnAttached ();
            if (AssociatedObject != null) {
                SetupImage ();
            }
        }

        protected override void OnDetaching () {
            base.OnDetaching ();
            if (AssociatedObject != null) {
                ClearImage ();
            }
        }

        private static void PropertyChangedCallback (
            DependencyObject dependencyObject,
            DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs) {
        }

        private static void BrushChanged (object sender, DependencyPropertyChangedEventArgs e) {
            (sender as GlyphImageSource)?.SetupImage ();
        }

        private static void SizeChanged (object sender, DependencyPropertyChangedEventArgs e) {
            (sender as GlyphImageSource)?.SetupImage ();
        }

        private void SetupImage () {
            if (AssociatedObject == null) {
                return;
            }

            switch (Size) {
                case GlyphImageSourceSize.Large:
                    AssociatedObject.SmallImageSource = null;
                    AssociatedObject.LargeImageSource = CreateGlpyh ();
                    break;

                default:
                    AssociatedObject.LargeImageSource = null;
                    AssociatedObject.SmallImageSource = CreateGlpyh ();
                    break;
            }
        }

        private void ClearImage () {
            if (AssociatedObject == null) {
                return;
            }

            AssociatedObject.SmallImageSource = null;
            AssociatedObject.LargeImageSource = null;
        }

        private ImageSource CreateGlpyh () {
            return GlyphHelper.CreateGlyph (Glyph, new FontFamily ("/Vacuum.Core;component/Fonts/#FontAwesome"), FontStyles.Normal, FontWeights.Normal, FontStretches.Normal, AssociatedObject.IsEnabled ? Brush : DisabledBrush);
        }
    }
}
