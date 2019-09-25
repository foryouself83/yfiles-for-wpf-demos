/****************************************************************************
 ** 
 ** This demo file is part of yFiles WPF 3.2.
 ** Copyright (c) 2000-2019 by yWorks GmbH, Vor dem Kreuzberg 28,
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
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using yWorks.Controls.Input;
using yWorks.Controls;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.Styles;
using yWorks.Graph.LabelModels;

namespace Tutorial.CustomStyles
{
  /// <summary>
  /// This demo shows how to create and use a relatively simple, non-interactive custom style
  /// for nodes, labels, edges, and ports, as well as a custom arrow.
  /// </summary>
  public partial class SimpleCustomStyleForm
  {

    #region demo specific

    // The following class members exist only in this tutorial step in order to 
    // point out the difference in rendering performance

    private Animator animator;
    // The number of rows and columns of nodes
    private static readonly int NodeCountSqrt = 14;

    private void CheckBox_Click(object sender, RoutedEventArgs e) {
      if (sender is CheckBox) {
        ((MySimpleNodeStyle)graphControl.Graph.NodeDefaults.Style).DrawPrerenderedShadow = ((CheckBox)sender).IsChecked ?? true;
        graphControl.Invalidate();
      }
    }

    private async void Button_Click(object sender, RoutedEventArgs e) {
      await StartAnimation();
    }

    private async Task StartAnimation() {
      // animates the nodes in random fashion
      Random r = new Random(DateTime.Now.TimeOfDay.Milliseconds);
      await animator.Animate(Animations.CreateGraphAnimation(graphControl.Graph, Mappers.FromDelegate<INode, IRectangle>(node => new RectD(r.NextDouble() * NodeCountSqrt * 40, r.NextDouble() * NodeCountSqrt * 40, node.Layout.Width, node.Layout.Height)), null, null, null, TimeSpan.FromSeconds(5)));
    }

    #endregion

    #region Constructor

    /// <summary>
    /// Automatically generated by Visual Studio.
    /// Wires up the UI components and adds a 
    /// <see cref="GraphControl"/> to the form.
    /// </summary>
    public SimpleCustomStyleForm() {
      InitializeComponent();
    }

    #endregion

    #region Initialization

    /// <summary>
    /// Called upon loading of the form.
    /// This method initializes the graph and the input mode.
    /// </summary>
    /// <seealso cref="CreateEditorMode"/>
    /// <seealso cref="InitializeGraph"/>
    protected virtual void OnLoaded(object src, RoutedEventArgs e) {
      // initialize the graph
      InitializeGraph();

      // initialize the input mode
      graphControl.InputMode = CreateEditorMode();

      graphControl.FitGraphBounds();

      animator = new Animator(graphControl);

      // stop animations when closing the window
      Closing += (sender, args) => animator.Stop();
    }

    /// <summary>
    /// Sets a custom NodeStyle instance as a template for newly created
    /// nodes in the graph.
    /// </summary>
    protected void InitializeGraph() {
      IGraph graph = graphControl.Graph;

      // Create a new style and use it as default node style
      graph.NodeDefaults.Style = new MySimpleNodeStyle();
      // Create a new style and use it as default label style
      graph.NodeDefaults.Labels.Style = graph.EdgeDefaults.Labels.Style = new DefaultLabelStyle
      {
        BackgroundPen = Pens.Black,
        BackgroundBrush = Brushes.White,
      };
      graph.NodeDefaults.Labels.LayoutParameter = ExteriorLabelModel.North;

      graph.NodeDefaults.Size = new SizeD(50, 50);

      // Create some graph elements with the above defined styles.
      CreateSampleGraph();
    }

    /// <summary>
    /// Creates the default input mode for the GraphControl,
    /// a <see cref="GraphEditorInputMode"/>.
    /// </summary>
    /// <returns>a new GraphEditorInputMode instance</returns>
    protected virtual IInputMode CreateEditorMode() {
      GraphEditorInputMode mode = new GraphEditorInputMode
                                    {
                                      //We enable label editing
                                      AllowEditLabel = true
                                    };
      return mode;
    }

    #endregion

    #region Graph creation
    /// <summary>
    /// Creates the initial sample graph.
    /// </summary>
    private void CreateSampleGraph() {
      IGraph graph = graphControl.Graph;

      for (int i = 1; i <= NodeCountSqrt; i++) {
        for (int j = 1; j <= NodeCountSqrt; j++) {
          graph.CreateNode(new RectD(40 * i, 40 * j, 30, 30));
        }
      }
    }

    #endregion

  }
}
