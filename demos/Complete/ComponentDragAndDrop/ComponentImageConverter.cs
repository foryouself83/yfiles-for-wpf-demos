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
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using yWorks.Controls;
using yWorks.Graph;

namespace Demo.yFiles.Graph.ComponentDragAndDrop
{
  /// <summary>
  /// Converts from a <see cref="IGraph">component</see> to an image that shows the visual
  /// representation of the component.  
  /// </summary>
  /// <remarks>
  /// Used in the palette view.
  /// </remarks>
  [ValueConversion(typeof(IGraph), typeof(DrawingImage))]
  public class ComponentImageConverter : IValueConverter {
    private readonly GraphControl graphControl = new GraphControl();

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
      var graph = value as IGraph;
      if (graph == null) {
        return null;
      }

      graphControl.Graph = graph;
      graphControl.FitGraphBounds();
      var size = graphControl.ContentRect;
      var cc = new ContextConfigurator(graphControl.ContentRect);
      cc.Scale = Math.Min(cc.CalculateScaleForWidth(size.Width), cc.CalculateScaleForHeight(size.Height));

      var renderContext = cc.CreateRenderContext(graphControl);
      Transform transform = cc.CreateWorldToIntermediateTransform();
      Geometry clip = cc.CreateClip();

      var visualContent = graphControl.ExportContent(renderContext);
      var container = new VisualGroup {
          Transform = transform,
          Clip = clip,
          Children = { visualContent }
      };
      var brush = new VisualBrush(container);
      return new DrawingImage(new GeometryDrawing(brush, null,new RectangleGeometry(new Rect(0, 0, size.Width, size.Height))));
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
      throw new NotImplementedException();
    }
  }
}
