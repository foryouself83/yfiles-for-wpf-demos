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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using yWorks.Controls.Input;
using yWorks.Controls;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.Styles;
using yWorks.Graph.LabelModels;
using yWorks.Graph.PortLocationModels;
using yWorks.Utils;


namespace Tutorial.GettingStarted
{

  /// <summary>
  /// Getting Started - 12 Binding Data to Graph Elements
  /// This demo shows how to bind arbitrary data to graph elements.
  /// </summary>
  /// <remarks>The usual way to bind arbitrary data to graph elements 
  /// is with the help of instances of the <see cref="IMapper{K,V}"/>
  /// interface. This demo uses class <see cref="DictionaryMapper{K,V}"/> 
  /// to store the creation time of each node. It also shows
  /// how to display tool tips with this data and a context menu to 
  /// manipulate it.
  /// <para>Note: Since we are using folding, there are actually two graph 
  /// instances to which we could bind data: the folding manager's master graph, 
  /// and the managed view that we display in the GraphControl.. This demo binds data to the items 
  /// of the master graph, since we want a timestamp to be associated with each logical
  /// entity in the graph, independent of its presence in the current managed view. 
  /// However, since we want to access these values through instances of the managed view,
  /// we can't just use a simple map, but have to query the data through a symbolic name.</para>
  /// <para>Note: An alternative approach would be to use the <see cref="ITagOwner"/> 
  /// interface of the <see cref="IModelItem"/>s to bind a single datum to 
  /// graph elements.</para>
  /// <para>Note: Custom data persistence is shown later in this tutorial.</para>
  /// </remarks>
  public partial class SampleApplication
  {

    public void OnLoaded(object source, EventArgs args) {

      ConfigureGroupNodeStyles();

      CustomizePortHandling();

      // Configure interaction
      // Note that this step has been moved before EnableDataBinding, because
      // one approach for setting up the custom data depends on the input mode
      // being already initialized. Creating the input mode does not depend on
      // one of the other setup steps.
      ConfigureInteraction();

      ///////////////// New in this Sample /////////////////

      // Sets up the data binding after folding has been enabled.
      // Order is important here, since we want to subscribe to events on the master graph
      EnableDataBinding();

      //////////////////////////////////////////////////////

      // From now on, everything can be done on the actual managed view instance
      EnableFolding();

      // Enables GraphML IO
      EnableGraphMLIO();

      ///////////////// New in this Sample /////////////////

      // Displays tooltips for the stored data items, so that something is visible to the user
      SetupTooltips();

      // Add a context menu to nodes
      SetupContextMenu();

      //////////////////////////////////////////////////////

      // Configures default label model parameters for newly created graph elements
      SetDefaultLabelParameters();

      // Configures default styles for newly created graph elements
      SetDefaultStyles();

      // Populates the graph
      PopulateGraph();

      // Enables the undo engine (disabled by default)
      EnableUndo();

      // Manages the viewport
      UpdateViewport();
    }

    /// <summary>
    /// Symbolic name for the mapper that allows transparent access to the correct implementation even across
    /// wrapped graphs.
    /// </summary>
    private const string DateTimeMapperKey = "DateTimeMapperKey";

    /// <summary>
    /// Sets up simple data binding - creates an IMapper, registers it and subscribe to the node creation event
    /// on the master graph.
    /// </summary>
    private void EnableDataBinding() {
      // Creates a specialized IMapper instance, and registers it under a symbolic name.
      // Uses WeakDictionaryMapper so that in case a node is removed from the graph,
      // we don't have to care about the reference through the IMapper.

      IMapper<INode, DateTime> dateMapper = Graph.MapperRegistry.CreateWeakMapper<INode, DateTime>(DateTimeMapperKey);
      
      //Subscribes to the node creation event to record the node creation time.
      //Note that since this event is triggered after undo/redo, the time will
      //be updated when re-doing node creations and undoing node deletions.
      //If this is unwanted behavior, you can customize the node creation itself
      //to associate this data with the element at the time of its initial creation,
      //e.g. by listening to the NodeCreated event of GraphEditorInputMode, see below
      Graph.NodeCreated += delegate(object source, ItemEventArgs<INode> eventArgs) {
        // Stores the current time as node creation time.
        dateMapper[eventArgs.Item] = DateTime.Now;
      };

      // Alternatively (or in addition) we could use the event for
      // interactive node creation as follows, provided that the input mode
      // for the graph control is already set.
      //                      ((GraphEditorInputMode)graphControl.InputMode).NodeCreated +=
      //                        delegate(object sender, ItemEventArgs<INode> e) {
      //                          IFoldingView foldingView = Graph.GetFoldingView();
      //                          if (foldingView != null) {
      //                            // Store the data at the master graph as in the original approach.
      //                            INode master = foldingView.GetMaster(e.Item);
      //                            dateMapper[master] = DateTime.Now;
      //                          }
      //                        };
    }

    /// <summary>
    /// Setup tooltips that return the value that is stored in the mapper.
    /// </summary>
    /// <remarks>
    /// Dynamic tooltips are implemented by adding a tooltip provider as an event handler for the
    /// <see cref="GraphEditorInputMode.QueryItemToolTip" /> event of the GraphEditorInputMode using the
    /// <see cref="QueryItemToolTipEventArgs{T}" /> parameter. This parameter provides three relevant
    /// properties:
    /// <list type="bullet">
    /// <item>
    /// <description>
    /// The <see cref="ToolTipQueryEventArgs.Handled" /> property is a flag which indicates
    /// whether the tooltip was already set by one of possibly several tooltip providers.
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// The <see cref="QueryItemToolTipEventArgs{T}.Item" /> property contains the <see cref="IModelItem" />
    /// the mouse hovers over.
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// The tooltip is set by setting the <see cref="ToolTipQueryEventArgs.ToolTip" />
    /// property.
    /// </description>
    /// </item>
    /// </list>
    /// </remarks>
    private void SetupTooltips() {
      GraphEditorInputMode geim = graphControl.InputMode as GraphEditorInputMode;
      if (geim != null) {
        geim.ToolTipItems = GraphItemTypes.Node;
        geim.QueryItemToolTip +=
          delegate(object src, QueryItemToolTipEventArgs<IModelItem> eventArgs) {
            if (eventArgs.Handled) {
              // A tooltip has already been assigned -> nothing to do.
              return;
            }
            INode hitNode = eventArgs.Item as INode;
            if (hitNode == null) {
              return;
            }
            // Since the node the user interacts with is an instance on the managed view,
            // rather than on the master graph to which we've bound our data,
            // we retrieve the mapper indirectly through its symbolic name.
            // The folding framework automagically returns an IMapper instance that translates
            // to the original elements. If we were not using folding, this step would be unnecessary
            // and we could use the mapper instance directly on the original nodes.
            IMapperRegistry registry = Graph.MapperRegistry;
            IMapper<INode, DateTime> dateMapper =
              registry.GetMapper<INode, DateTime>(DateTimeMapperKey);
            if (dateMapper != null) {
              //Found a suitable mapper.
              //Finds out if a node is under the current location.
              // Set the tooltip.
              eventArgs.ToolTip = dateMapper[hitNode].ToString();

              // Indicate that the tooltip has been set.
              eventArgs.Handled = true;
            }
          };

        // Add a little offset to the tooltip such that it is not obscured by the mouse pointer.
        geim.MouseHoverInputMode.ToolTipLocationOffset = new PointD(20, 20);
      }
    }

    /// <summary>
    /// Adds a context menu for nodes
    /// </summary>
    private void SetupContextMenu() {
      GraphEditorInputMode mode = graphControl.InputMode as GraphEditorInputMode;
      if (mode != null) {
        mode.ContextMenuItems = GraphItemTypes.Node;
        mode.PopulateItemContextMenu += (sender, e) =>
        {
          // the events' ContextMenu property carries the Context Menu instance that we instantiate during initialization
          // depending on the exact menu implementation, adding items needs to be done differently.
          var node = e.Item as INode;
          if (node != null) {
            // add a context menu entry
            var menuItem = new MenuItem() { Header = "Set to now" };
            menuItem.Click += (o, args) => SetToNow(node);
            e.Menu.Items.Add(menuItem);
            e.Handled = true;
          }
        }; 
      }
    }

    /// <summary>
    /// Sets the mapped DateTime object of a node to the current value.
    /// </summary>
    /// <param name="node">the node to map the DateTime instance to.</param>
    private void SetToNow(INode node) {
      IMapper<INode, DateTime> mapper = Graph.MapperRegistry.GetMapper<INode, DateTime>(DateTimeMapperKey);
      if (mapper != null)
      {
        mapper[node] = DateTime.Now;
      }
    }

    #region Configure Folding

    private FoldingManager manager;

    /// <summary>
    /// Enables folding - changes the GraphControl's graph to a managed view
    /// that provides the actual collapse/expand state.
    /// </summary>
    private void EnableFolding() {
      // Creates the folding manager and sets its master graph to
      // the single graph that has served for all purposes up to this point
      manager = new FoldingManager(Graph);
      // Creates a managed view from the master graph and 
      // replaces the existing graph view with a managed view
      graphControl.Graph = manager.CreateFoldingView().Graph;
      // Change the default style for group nodes. We use <see cref="CollapsibleNodeStyleDecorator"/> 
      // to wrap the <see cref="PanelNodeStyle"/> from the last demo, since we want to have nice
      // +/- buttons for collapse/expand. Note that if you haven't defined
      // a custom group node style, you don't have to do anything at all, since
      // <see cref="FoldingManager"/> already
      // provides such a decorated group node style by default.
      Graph.GroupNodeDefaults.Style = new CollapsibleNodeStyleDecorator(Graph.GroupNodeDefaults.Style);
    }


    #endregion

    #region Customized Undo for Folding

    /// <summary>
    /// Enables the Undo functionality.
    /// </summary>
    private void EnableUndo() {
      // Enables undo on the folding manager's master graph instead of on the managed view.
      manager.MasterGraph.SetUndoEngineEnabled(true);
    }

    #endregion

    #region Configure grouping

    /// <summary>
    /// Configures the default style for group nodes.
    /// </summary>
    private void ConfigureGroupNodeStyles() {
      // PanelNodeStyle is a style especially suited to group nodes
      // Creates a panel with a light blue background
      Color groupNodeColor = Color.FromArgb(255, 214, 229, 248);
      var groupNodeDefaults = Graph.GroupNodeDefaults;
      groupNodeDefaults.Style = new PanelNodeStyle {
        Color = groupNodeColor,
        // Specifies insets that provide space for a label at the top
        // For a solution how to determine these insets automatically, please
        // see the yEd WPF demo application.
        Insets = new InsetsD(5, 18, 5, 5),
        LabelInsetsColor = groupNodeColor,
      };

      // Sets a label style with right-aligned text
      groupNodeDefaults.Labels.Style = new DefaultLabelStyle { TextAlignment = TextAlignment.Right };

      // Places the label at the top inside of the panel.
      // For PanelNodeStyle, InteriorStretchLabelModel is usually the most appropriate label model
      groupNodeDefaults.Labels.LayoutParameter = InteriorStretchLabelModel.North;
    }

    /// <summary>
    /// Shows how to create group nodes programmatically.
    /// </summary>
    /// <remarks>Creates two nodes and puts them into a group node.</remarks>
    private INode CreateGroupNodes(params INode[] childNodes) {
      //Creates a group node that encloses the given child nodes
      INode groupNode = Graph.GroupNodes(childNodes);

      // Creates a label for the group node 
      Graph.AddLabel(groupNode, "Group Node");

      // Adjusts the layout of the group nodes
      Graph.AdjustGroupNodeLayout(groupNode);
      return groupNode;
    }

    #endregion

    #region Custom Lookup Chain Link for port candidates

    /// <summary>
    /// Configure custom port handling with the help of <see cref="ILookup"/>
    /// </summary>
    private void CustomizePortHandling() {
      // Sets auto cleanup to false, since we don't want to remove unoccupied ports.
      Graph.NodeDefaults.Ports.AutoCleanUp = false;

      // First we create a GraphDecorator from the IGraph.
      // GraphDecorator is a utility class that aids in decorating model items from a graph instance.

      // Here, we call NodeDecorator.PortCandidateProviderDecorator
      // to access the lookup decorator for ports - the thing we want to change.

      // One way to decorate the graph is to use the factory design pattern.
      // We set the factory to a lambda expression which
      // returns instances that implement the IPortCandidateProvider interface.

      // Here we can create a CompositePortCandidateProvider that combines various port candidate providers.
      // The ExistingPortsCandidateProvider provides port candidates at the locations of the already existing ports.
      // The NodeCenterPortCandidateProvider provides a single port candidate at the center of the node.
      // The ShapeGeometryPortCandidateProvider provides several port candidates based on the shape of the node.
      Graph.GetDecorator().NodeDecorator.PortCandidateProviderDecorator.SetFactory(
        node => PortCandidateProviders.Combine(
          PortCandidateProviders.FromExistingPorts(node),
          PortCandidateProviders.FromNodeCenter(node),
          PortCandidateProviders.FromShapeGeometry(node)));

      // To modify the existing lookup for a graph element, typically we decorate it with the help
      // of one of graph's Get...Decorator() extension methods,
      // which allows to dynamically insert custom implementations for the specified types.
      // Doing this can be seen as dynamically subclassing
      // the class in question (the INode implementation in this case), but only
      // for the node instances that live in the graph in question and then
      // overriding just their Lookup(Type) method. The only difference to traditional
      // subclassing is that you get the "this" passed in as a parameter.
      // Doing this more than once is like subclassing more and more, so the order matters.
    }

    #endregion

    #region Enable command bindings for GraphML I/O

    /// <summary>
    /// Enables GraphML I/O command bindings.
    /// </summary>
    private void EnableGraphMLIO() {
      // Usually, this would be done in XAML, we just show it here for convenience
      graphControl.FileOperationsEnabled = true;
    }

    #endregion

    #region InputMode creation and configuration

    /// <summary>
    /// Configure basic interaction.
    /// </summary>
    /// <remarks>Interaction is handled by so called InputModes. <see cref="GraphEditorInputMode"/> is the main
    /// InputMode that already provides a large number of graph interaction possibilities, such as moving, deleting, creating,
    /// resizing graph elements. Note that to create or edit a label, just press F2. Also, try to move a label around and see what happens
    /// </remarks>
    private void ConfigureInteraction() {
      // Creates a new GraphEditorInputMode instance and registers it as the main
      // input mode for the graphControl
      var inputMode = new GraphEditorInputMode();
      // enable grouping operations on the input mode
      inputMode.AllowGroupingOperations = true;
      graphControl.InputMode = inputMode;
    }

    #endregion

    #region Default label model parameters

    /// <summary>
    /// Set up default label model parameters for graph elements.
    /// </summary>
    /// <remarks>
    /// Label model parameters control the actual label placement as well as the available
    /// placement candidates when moving the label interactively.
    /// </remarks>
    private void SetDefaultLabelParameters() {
      #region Default node label model parameter

      // For node labels, the default is a label position at the node center
      // Let's keep the default.  Here is how to set it manually
      Graph.NodeDefaults.Labels.LayoutParameter = InteriorLabelModel.Center;

      #endregion

      #region Default edge label parameter

      // For edge labels, the default is a label that is rotated to match the associated edge segment
      // We'll start by creating a model that is similar to the default:
      EdgeSegmentLabelModel edgeSegmentLabelModel = new EdgeSegmentLabelModel();
      // However, by default, the rotated label is centered on the edge path.
      // Let's move the label off of the path:
      edgeSegmentLabelModel.Distance = 10;
      // Finally, we can set this label model as the default for edge labels using a location at the center of the first segment
      Graph.EdgeDefaults.Labels.LayoutParameter = edgeSegmentLabelModel.CreateParameterFromSource(0, 0.5, EdgeSides.RightOfEdge);

      #endregion
    }

    #endregion

    #region Sample graph creation
    /// <summary>
    /// Creates a sample graph and introduces all important graph elements present in
    /// yFiles WPF. Additionally, this method now overrides the label placement for some specific labels.
    /// </summary>
    private void PopulateGraph() {
      #region Sample Graph creation

      // Creates two nodes with the default node size
      // The location is specified for the _center_
      INode node1 = Graph.CreateNode(new PointD(50, 50));
      INode node2 = Graph.CreateNode(new PointD(150, 50));
      // Creates a third node with a different size of 80x40
      // In this case, the location of (360,280) describes the _upper left_
      // corner of the node bounds
      INode node3 = Graph.CreateNode(new RectD(260, 180, 80, 40));
      
      // Creates some edges between the nodes
      IEdge edge1 = Graph.CreateEdge(node1, node2);
      IEdge edge2 = Graph.CreateEdge(node2, node3);

      // Creates the first bend for edge2 at (400, 50)
      IBend bend1 = Graph.AddBend(edge2, new PointD(300, 50));

      // Actually, edges connect "ports", not nodes directly.
      // If necessary, you can manually create ports at nodes
      // and let the edges connect to these.
      // Creates a port in the center of the node layout
      IPort port1AtNode1 = Graph.AddPort(node1, FreeNodePortLocationModel.NodeCenterAnchored);

      // Creates a port at the middle of the left border
      // Note to use absolute locations when placing ports using PointD.
      IPort port1AtNode3 = Graph.AddPort(node3, new PointD(node3.Layout.X, node3.Layout.GetCenter().Y));

      // Creates an edge that connects these specific ports
      IEdge edgeAtPorts = Graph.CreateEdge(port1AtNode1, port1AtNode3);

      // Adds labels to several graph elements
      Graph.AddLabel(node1, "Node 1");
      Graph.AddLabel(node2, "Node 2");
      Graph.AddLabel(node3, "Node 3");
      Graph.AddLabel(edgeAtPorts, "Edge at Ports");

      // Add some more elements to have a larger graph to edit
      var n4 = Graph.CreateNode(new PointD(50, -50));
      Graph.AddLabel(n4, "Node 4");
      var n5 = Graph.CreateNode(new PointD(50, -150));
      Graph.AddLabel(n5, "Node 5");
      var n6 = Graph.CreateNode(new PointD(-50, -50));
      Graph.AddLabel(n6, "Node 6");
      var n7 = Graph.CreateNode(new PointD(-50, -150));
      Graph.AddLabel(n7, "Node 7");
      var n8 = Graph.CreateNode(new PointD(150, -50));
      Graph.AddLabel(n8, "Node 8");

      Graph.CreateEdge(n4, node1);
      Graph.CreateEdge(n5, n4);
      Graph.CreateEdge(n7, n6);
      var e6_1 = Graph.CreateEdge(n6, node1);
      Graph.AddBend(e6_1, new PointD(-50, 50), 0);

      // Creates a group node programmatically which groups the child nodes n4, n5, and n8
      var groupNode = CreateGroupNodes(n4, n5, n8);
      // creates an edge between the group node and node 2
      var eg_2 = Graph.CreateEdge(groupNode, node2);
      Graph.AddBend(eg_2, new PointD(100, 0), 0);
      Graph.AddBend(eg_2, new PointD(150, 0), 1);

      #endregion
    }

    #endregion

    #region Default style setup

    /// <summary>
    /// Set up default styles for graph elements.
    /// </summary>
    /// <remarks>
    /// Default styles apply only to elements created after the default style has been set,
    /// so typically, you'd set these as early as possible in your application.
    /// </remarks>
    private void SetDefaultStyles() {
      
      #region Default Node Style
      // Sets the default style for nodes
      // Creates a nice ShinyPlateNodeStyle instance, using an orange Brush.
      INodeStyle defaultNodeStyle = new ShinyPlateNodeStyle { Brush = new SolidColorBrush(Color.FromArgb(255, 255, 140, 0)) };

      // Sets this style as the default for all nodes that don't have another
      // style assigned explicitly
      Graph.NodeDefaults.Style = defaultNodeStyle;

      #endregion

      #region Default Edge Style
      // Sets the default style for edges:
      // Creates an edge style that will apply a gray pen with thickness 1
      // to the entire line using PolyLineEdgeStyle,
      // which draws a polyline determined by the edge's control points (bends)
      var defaultEdgeStyle = new PolylineEdgeStyle { Pen = Pens.Gray };

      // Sets the source and target arrows on the edge style instance
      // (Actually: no source arrow)
      // Note that IEdgeStyle itself does not have these properties
      // Also note that by default there are no arrows
      defaultEdgeStyle.TargetArrow = Arrows.Default;

      // Sets the defined edge style as the default for all edges that don't have
      // another style assigned explicitly:
      Graph.EdgeDefaults.Style = defaultEdgeStyle;
      #endregion

      #region Default Label Styles
      // Sets the default style for labels
      // Creates a label style with the label text color set to dark red
      ILabelStyle defaultLabelStyle = new DefaultLabelStyle { Typeface = new Typeface("Tahoma"), TextSize = 12, TextBrush = Brushes.DarkRed };

      // Sets the defined style as the default for both edge and node labels:
      Graph.EdgeDefaults.Labels.Style = Graph.NodeDefaults.Labels.Style = defaultLabelStyle;

      #endregion

      #region Default Node size
      // Sets the default size explicitly to 40x40
      Graph.NodeDefaults.Size = new SizeD(40, 40);

      #endregion

    }

    #endregion

    #region Viewport handling

    /// <summary>
    /// Updates the content rectangle to encompass all existing graph elements.
    /// </summary>
    /// <remarks>If you create your graph elements programmatically, the content rectangle 
    /// (i.e. the rectangle in <b>world coordinates</b>
    /// that encloses the graph) is <b>not</b> updated automatically to enclose these elements. 
    /// Typically, this manifests in wrong/missing scrollbars, incorrect <see cref="GraphOverviewControl"/> 
    /// behavior and the like.
    /// <para>
    /// This method demonstrates several ways to update the content rectangle, with or without adjusting the zoom level 
    /// to show the whole graph in the view.
    /// </para>
    /// <para>
    /// Note that updating the content rectangle only does not change the current Viewport (i.e. the world coordinate rectangle that
    /// corresponds to the currently visible area in view coordinates)
    /// </para>
    /// <para>
    /// Uncomment various combinations of lines in this method and observe the different effects.
    /// </para>
    /// <para>The following demos in this tutorial will assume that you've called <c>graphControl.FitGraphBounds();</c>
    /// in this method.</para>
    /// </remarks>
    private void UpdateViewport() {
      // Uncomment the following line to update the content rectangle 
      // to include all graph elements
      // This should result in correct scrolling behavior:

      //graphControl.UpdateContentRect();

      // Additionally, we can also set the zoom level so that the
      // content rectangle fits exactly into the viewport area:
      // Uncomment this line in addition to UpdateContentRect:
      // Note that this changes the zoom level (i.e. the graph elements will look smaller)

      //graphControl.FitContent();

      // The sequence above is equivalent to just calling:
      graphControl.FitGraphBounds();
    }

    #endregion

    #region Standard Event handlers

    private void ExitMenuItem_Click(object sender, RoutedEventArgs e) {
      Application.Current.Shutdown();
    }

    #endregion

    #region Convenience Properties

    public IGraph Graph {
      get { return graphControl.Graph; }
    }

    #endregion

    #region Constructor
    public SampleApplication() {
      InitializeComponent();
    }

    #endregion
  }
}
