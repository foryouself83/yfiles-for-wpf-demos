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
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using yWorks.Controls.Input;
using yWorks.Controls;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.Styles;
using yWorks.GraphML;
using yWorks.Utils;
using yWorks.Layout;
using yWorks.Layout.Hierarchic;
using HL = yWorks.Layout.Hierarchic.HierarchicLayout;

namespace Demo.yFiles.Layout.IncrementalHierarchicLayout
{
  /// <summary>
  /// Sample Form that interactively demonstrates the usage of HierarchicLayout.
  /// </summary>
  /// <remarks>
  /// This demos shows how to incrementally add nodes and edges, dynamically assign port constraints.
  /// Create new nodes and observe how they are inserted into the drawing near the place they have been created.
  /// Create new edges and watch the routings being calculated immediately.
  /// Drag the first and last bend of an edge to interactively assign or reset port constraints.
  /// Use the context menu to reroute selected edges or optimize selected nodes locations.
  /// </remarks>
  public partial class IncrementalHierarchicLayoutDemo
  {
    /// <summary>
    /// Automatically generated by Visual Studio.
    /// Wires up the UI components and adds a 
    /// <see cref="GraphControl"/> to the form.
    /// </summary>
    public IncrementalHierarchicLayoutDemo() 
    {
      InitializeComponent();

      // Configure the GraphMLIOHandler so that a layout runs automatically after loading a graph
      graphControl.GraphMLIOHandler.Parsed += delegate {
        if (!this.updateLayout) {
          // after the file has been loaded - create an initial layout
          graphControl.Graph.ApplyLayout(CreateLayout(), new HierarchicLayoutData { LayerIndices = layerIndices });

          // update the layers
          layerVisualCreator.UpdateLayers(graphControl.Graph, layerIndices);
        }
      };
    }

    /// <summary>
    /// Factory method that creates the layout instances that are used by this demo.
    /// </summary>
    private HL CreateLayout() {
      return new HL { OrthogonalRouting = true, RecursiveGroupLayering = false};
    }

    /// <summary>
    /// Called upon loading of the form.
    /// This method initializes the graph and the input mode.
    /// </summary>
    /// <seealso cref="InitializeInputModes"/>
    /// <seealso cref="InitializeGraph"/>
    protected void OnLoaded(object source, EventArgs e) 
    {
      // initialize the graph
      InitializeGraph();

      // initialize the input mode
      InitializeInputModes();

      // fit it nicely into the control
      graphControl.FitGraphBounds();
    }

    /// <summary>
    /// Calls <see cref="CreateEditorMode"/> and registers
    /// the result as the <see cref="CanvasControl.InputMode"/>.
    /// </summary>
    protected virtual void InitializeInputModes()
    {
      // create the interaction mode
      graphControl.InputMode = CreateEditorMode();

      // display the layers
      graphControl.BackgroundGroup.AddChild(layerVisualCreator);
    }

    /// <summary>
    /// Creates the default input mode for the GraphControl,
    /// a <see cref="GraphEditorInputMode"/>.
    /// </summary>
    /// <returns>a specializes new GraphEditorInputMode instance</returns>
    protected virtual IInputMode CreateEditorMode()
    {
      GraphEditorInputMode mode = new GraphEditorInputMode{AllowGroupingOperations = true};
      // creating bends does not make sense because the routing is calculated
      // immediately after the creation.
      mode.CreateEdgeInputMode.AllowCreateBend = false;

      // register hooks whenever something is dragged or resized
      mode.HandleInputMode.DragFinished += UpdateLayout;
      mode.MoveInputMode.DragFinished += UpdateLayout;
      // ... and when new nodes are created interactively
      mode.NodeCreated += OnNodeCreated;
      // ... or edges
      mode.CreateEdgeInputMode.EdgeCreated += OnEdgeCreated;
      mode.PopulateItemContextMenu += OnPopulateItemContextMenu;
      return mode;
    }

    /// <summary>
    /// Called when an edge has been created interactively.
    /// </summary>
    private void OnEdgeCreated(object sender, ItemEventArgs<IEdge> args) {
      incrementalEdges.Add(args.Item); 
      UpdateLayout(sender, args);
    }

    /// <summary>
    /// Called when the context menu should be populated for a given item.
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="populateItemContextMenuEventArgs">The event argument instance containing the event data.</param>
    private void OnPopulateItemContextMenu(object sender, PopulateItemContextMenuEventArgs<IModelItem> populateItemContextMenuEventArgs) {
      // see if it's a node but not a not empty group node
      INode node = populateItemContextMenuEventArgs.Item as INode;
      if (node != null && graphControl.Graph.GetChildren(node).Count == 0) {
        // see if it's already selected
        var selectedNodes = graphControl.Selection.SelectedNodes;
        if (!selectedNodes.IsSelected(node)) {
          // no - make it the only selected node
          selectedNodes.Clear();
        } 
        // make sure the node is selected
        selectedNodes.SetSelected(node, true);
        graphControl.CurrentItem = node;
        // mark all selected nodes for incremental layout
        var item = new MenuItem{Header = "Reinsert Incrementally"};
        item.Click += delegate(object o, RoutedEventArgs args) {
                        incrementalNodes.AddRange(selectedNodes);
                        UpdateLayout(o, args);
                      };
        populateItemContextMenuEventArgs.Menu.Items.Add(item);
        populateItemContextMenuEventArgs.Handled = true;
      }

      // if it's an edge...
      IEdge edge = populateItemContextMenuEventArgs.Item as IEdge;
      if (edge != null) {
        // update selection state
        var selectedEdges = graphControl.Selection.SelectedEdges;
        if (!selectedEdges.IsSelected(edge)) {
          selectedEdges.Clear();
        }
        selectedEdges.SetSelected(edge, true);
        graphControl.CurrentItem = edge;
        // and offer option to reroute selected edges
        var item = new MenuItem { Header = "Reroute" };
        item.Click += delegate(object o, RoutedEventArgs args) {
          incrementalEdges.AddRange(selectedEdges);
          UpdateLayout(o, args);
        };
        populateItemContextMenuEventArgs.Menu.Items.Add(item);
        populateItemContextMenuEventArgs.Handled = true;
      }
    }


    /// <summary>
    /// Called when a node has been created interactively.
    /// </summary>
    private void OnNodeCreated(object sender, ItemEventArgs<INode> itemEventArgs) {
      int newLayer = layerVisualCreator.GetLayer(itemEventArgs.Item.Layout.GetCenter());
      newLayerMapper[itemEventArgs.Item] = newLayer;
      UpdateLayout(sender, itemEventArgs);
    }

    /// <summary>
    /// Core method that recalculates and updates the layout.
    /// </summary>
    private async void UpdateLayout(object sender, EventArgs e) {
      // make sure we are not reentrant
      if (updateLayout) {
        return;
      }
      updateLayout = true;

      // update the layers for moved nodes
      UpdateMovedNodes();

      // create and configure the HierarchicLayout
      var layout = CreateLayout();
      // rearrange only the incremental graph elements, the
      // remaining elements are not, or only slightly, changed
      layout.LayoutMode = LayoutMode.Incremental;
      // use the GivenLayersLayerer for all non-incremental nodes
      layout.FixedElementsLayerer = new GivenLayersLayerer();

      // provide additional data to configure the HierarchicLayout
      var hierarchicLayoutData = new HierarchicLayoutData {
        // specify the layer of each non-incremental node
        GivenLayersLayererIds = {Mapper = layerIndices },
        // retrieve the layer of each incremental node after the layout run to update the layer visualization
        LayerIndices = layerIndices,
        // specify port constrains for the source of each edge
        SourcePortConstraints = {Mapper = sourcePortConstraints},
        // specify port constrains for the target of each edge
        TargetPortConstraints = {Mapper = targetPortConstraints},
        IncrementalHints =
        {
          // specify the nodes to rearrange
          IncrementalLayeringNodes = {Source = incrementalNodes},
          // specify the edges to rearrange
          IncrementalSequencingItems = {Source = incrementalEdges},
        }
      };

      await graphControl.MorphLayout(layout, TimeSpan.FromSeconds(1), hierarchicLayoutData);

      // forget the nodes and edges for the next run
      incrementalNodes.Clear();
      incrementalEdges.Clear();
      layerVisualCreator.UpdateLayers(graphControl.Graph, layerIndices);
      updateLayout = false;

    }

    private void UpdateMovedNodes() {
      if (newLayerMapper.Entries.GetEnumerator().MoveNext()) {
        // spread out existing layers
        foreach (var node in graphControl.Graph.Nodes) {
          layerIndices[node] *= 2;
        }
        foreach (var pair in newLayerMapper.Entries) {
          INode node = pair.Key;
          // if a node has been moved, reinsert the adjacent edges incrementally and not from sketch
          incrementalEdges.AddRange(graphControl.Graph.EdgesAt(node));
          int newLayerIndex = pair.Value;
          if (newLayerIndex == int.MaxValue) {
            // the node has been dragged outside - mark it as incremental
            incrementalNodes.Add(node);
          } else if (newLayerIndex < 0) {
            int beforeLayer = -(newLayerIndex + 1);
            layerIndices[node] = beforeLayer*2 - 1;
          } else {
            layerIndices[node] = newLayerIndex*2;
          }
        }
        newLayerMapper.Clear();
      }
    }

    /// <summary>
    /// Initializes the graph instance setting default styles
    /// and creating a small sample graph.
    /// </summary>
    protected virtual void InitializeGraph()
    {
      IGraph graph = graphControl.Graph;

      // set some nice defaults
      graph.NodeDefaults.Style = new ShinyPlateNodeStyle { Brush = Brushes.Orange };
      graph.NodeDefaults.Size = new SizeD(60, 30);

      graph.GroupNodeDefaults.Style = new ShapeNodeStyle
      {
        Shape = ShapeNodeShape.RoundRectangle,
        Pen = new Pen(Brushes.DarkBlue, 2),
        Brush = null
      };

      // register a custom PositionHandler for the nodes.
      // this enables interactive layer reassignment with layer preview
      graph.GetDecorator().NodeDecorator.PositionHandlerDecorator.SetImplementationWrapper(
        node => graph.GetChildren(node).Count == 0,
        (node, positionHandler) => new LayerPositionHandler(layerVisualCreator, node, positionHandler, newLayerMapper));

      // register custom handles for the first and last bends of an edge 
      // this enables interactive port constraint assignment.
      graph.GetDecorator().BendDecorator.HandleDecorator.SetImplementationWrapper(
        bend => bend.Owner.Bends.First() == bend || bend.Owner.Bends.Last() == bend, CreateBendHandle);

      // create a small sample graph with given layers
      CreateSampleGraph(graph);
    }


    /// <summary>
    /// Creates the sample graph.
    /// </summary>
    private void CreateSampleGraph(IGraph graph) {
      INode n1, n2, n3, n4;

      layerIndices[n1 = graph.CreateNode()] = 0;
      layerIndices[n2 = graph.CreateNode()] = 1;
      layerIndices[n3 = graph.CreateNode()] = 2;
      layerIndices[n4 = graph.CreateNode()] = 2;

      graph.CreateEdge(n1, n2);
      graph.CreateEdge(n2, n3);
      graph.CreateEdge(n1, n4);

      // create an HierarchicLayout instance to provide an initial layout
      var hl = CreateLayout();
      // use the GivenLayersLayerer to respect the above node to layer assignment
      hl.FromScratchLayerer = new GivenLayersLayerer();

      // provide additional data to configure the layout
      // respect the above node to layer assignment
      var hlData = new HierarchicLayoutData {GivenLayersLayererIds = {Mapper = layerIndices} };
      // run the layout
      graph.ApplyLayout(hl, hlData);

      // and update the layer visualization
      layerVisualCreator.UpdateLayers(graph, layerIndices);
    }

    /// <summary>
    /// Callback that creates the bend IHandle for the first and last bends.
    /// </summary>
    /// <param name="bend">The bend.</param>
    /// <param name="originalImplementation">The original implementation to delegate to.</param>
    /// <returns>The new handle that allows for interactively assign the port constraints.</returns>
    private IHandle CreateBendHandle(IBend bend, IHandle originalImplementation) {
      if (bend.Owner.Bends.First() == bend) {
        // decorate first bend
        originalImplementation = new PortConstraintBendHandle(true, bend, originalImplementation, sourcePortConstraints);
      }
      
      if (bend.Owner.Bends.Last() == bend) {
        // decorate last bend - could be both first and last
        originalImplementation = new PortConstraintBendHandle(false, bend, originalImplementation, targetPortConstraints);
      }
      return originalImplementation;
    }

    #region Standard Actions

    /// <summary>
    /// Exits the demo.
    /// </summary>
    private void ExitMenuItem_Click(object sender, EventArgs e) {
      Application.Current.Shutdown();
    }

    #endregion

    #region Properties

    /// <summary>
    /// Visualizes the layers and manages layer regions and contains tests
    /// </summary>
    private readonly LayerVisualCreator layerVisualCreator = new LayerVisualCreator();

    // holds for each node the layer 
    private IMapper<INode, int> layerIndices = new DictionaryMapper<INode, int>();

    // whether a layout is runnning
    private bool updateLayout;
    
    // holds temporary layer reassignments that will be assigned during the next layout
    private readonly DictionaryMapper<INode, int> newLayerMapper = new DictionaryMapper<INode, int>();

    // holds for each edge a port constraint for the source end
    private WeakDictionaryMapper<IEdge, PortConstraint> sourcePortConstraints = new WeakDictionaryMapper<IEdge, PortConstraint>();

    // holds for each edge a port constraint for the target end
    private WeakDictionaryMapper<IEdge, PortConstraint> targetPortConstraints = new WeakDictionaryMapper<IEdge, PortConstraint>();

    // holds a list of nodes to insert incrementally during the next layout
    private readonly List<INode> incrementalNodes = new List<INode>();

    // holds a list of edges to reroute incrementally during the next layout
    private readonly List<IEdge> incrementalEdges = new List<IEdge>();

    #endregion

    private void DisableCommand(object sender, CanExecuteRoutedEventArgs e) {
      e.CanExecute = false;
      e.Handled = true;
    }
  }
}
