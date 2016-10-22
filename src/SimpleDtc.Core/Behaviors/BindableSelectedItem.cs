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
using System.Windows.Controls;
using System.Windows.Interactivity;

namespace SimpleDtc.Core.Behaviors {
    public class BindableSelectedItem : Behavior<TreeView> {
        public static readonly DependencyProperty SelectedItemProperty =
            DependencyProperty.Register ("SelectedItem", typeof (object), typeof (BindableSelectedItem), new FrameworkPropertyMetadata (null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSelectedItemChanged));


        public object SelectedItem {
            get { return GetValue (SelectedItemProperty); }
            set { SetValue (SelectedItemProperty, value); }
        }

        protected override void OnAttached () {
            base.OnAttached ();

            AssociatedObject.SelectedItemChanged += OnTreeViewSelectedItemChanged;
        }

        protected override void OnDetaching () {
            base.OnDetaching ();

            if (AssociatedObject != null) {
                AssociatedObject.SelectedItemChanged -= OnTreeViewSelectedItemChanged;
            }
        }

        private void OnTreeViewSelectedItemChanged (object sender, RoutedPropertyChangedEventArgs<object> e) {
            SelectedItem = e.NewValue;
        }

        private static void OnSelectedItemChanged (DependencyObject sender, DependencyPropertyChangedEventArgs e) {
            var item = e.NewValue as TreeViewItem;
            item?.SetValue (TreeViewItem.IsSelectedProperty, true);
        }
    }
}
