/****************************************************************************
 ** 
 ** This demo file is part of yFiles WPF 3.4.
 ** Copyright (c) 2000-2021 by yWorks GmbH, Vor dem Kreuzberg 28,
 ** 72070 Tuebingen, Germany. All rights reserved.
 ** 
 ** yFiles demo files exhibit yFiles WPF functionalities. Any redistribution
 ** of demo files in source code or binary form, with or without
 ** modification, is not permitted.
 ** 
 ** Owners of a valid software license for a yFiles WPF version that this
 ** demo is shipped with are allowed to use the demo source code as basis
 ** for their own yFiles WPF powered applications. Use of such programs is
 ** governed by the rights and conditions as set out in the yFiles WPF
 ** license agreement.
 ** 
 ** THIS SOFTWARE IS PROVIDED ''AS IS'' AND ANY EXPRESS OR IMPLIED
 ** WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF
 ** MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN
 ** NO EVENT SHALL yWorks BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
 ** SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED
 ** TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR
 ** PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF
 ** LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
 ** NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
 ** SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 ** 
 ***************************************************************************/

using System;
using System.Collections;
using System.Windows;
using System.Windows.Controls;
using yWorks.Graph.Styles;

namespace Demo.yFiles.Graph.Editor
{
  public partial class StyleChooser
  {
    public StyleChooser() {
      InitializeComponent();
    }

    public Type ItemType {
      set {
        if (value == typeof (INodeStyle)) {
          StylesList.ItemTemplate = (DataTemplate) this.Resources["NodeTemplate"];
        } else if (value == typeof (IEdgeStyle)) {
          StylesList.ItemTemplate = (DataTemplate) this.Resources["EdgeTemplate"];
        } else if (value == typeof (ILabelStyle)) {
          StylesList.ItemTemplate = (DataTemplate) this.Resources["LabelTemplate"];
        } else if (value == typeof(IPortStyle)) {
          StylesList.ItemTemplate = (DataTemplate)this.Resources["PortTemplate"];
        } else if (value == typeof(IArrow)) {
          StylesList.ItemTemplate = (DataTemplate) this.Resources["ArrowTemplate"];
        } else {
          throw new ArgumentOutOfRangeException("value");
        }
      }
    }

    public IEnumerable ItemsSource {
      get { return this.StylesList.ItemsSource; }
      set { this.StylesList.ItemsSource = value; }
    }

    public event SelectionChangedEventHandler SelectionChanged;

    private void OnSelectionChanged(object sender, SelectionChangedEventArgs e) {
      if (SelectionChanged != null) {
        SelectionChanged(this, e);
      }
    }

    public object SelectedItem {
      get { return StylesList.SelectedItem; }
    }

    public void Deselect() {
      this.StylesList.SelectedItem = null;
    }

    public void SelectFirst() {
      this.StylesList.SelectedIndex = 0;
    }
  }
}