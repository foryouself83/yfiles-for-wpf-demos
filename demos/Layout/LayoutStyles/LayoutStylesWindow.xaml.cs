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
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Demo.yFiles.Layout.Configurations;
using Demo.yFiles.Toolkit.OptionHandler;
using yWorks.Controls;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.Styles;
using Random = System.Random;

namespace Demo.yFiles.Layout.LayoutStyles
{
  /// <summary>
  /// Demo window that shows several sample graphs and allows to try out different layout algorithms
  /// along with their configuration.
  /// </summary>
  public partial class LayoutStylesWindow
  {
    /// <summary>
    /// Instance of System.Random that can be re-used
    /// </summary>
    private static readonly Random random = new Random();

    /// <inheritdoc />
    /// <summary>
    /// Automatically generated by Visual Studio.
    /// Wires up the UI components, adds a 
    /// <see cref="P:Demo.yFiles.Layout.LayoutStyles.LayoutStylesWindow.GraphControl" /> to the form,
    /// and displays settings for the config of the edgeRouter.
    /// </summary>
    public LayoutStylesWindow() {
      //auto-created by visual studio
      InitializeComponent();
    }

    /// <summary>
    /// Initializes the graph and the input mode.
    /// </summary>
    /// <see cref="InitializeGraph"/>
    protected virtual void OnLoaded(object source, EventArgs e) {
      // Initialize the graph
      InitializeGraph();

      // Load the algorithms because they are needed by the sample combobox
      InitializeLayoutAlgorithms();

      // Initialize the input mode
      CreateEditorInputMode();

      //Initialise the Sample ComboBox last because it will run an layout algorithm
      InitializeSampleComboBox();
    }

    private readonly object[] layouts = {
      new LayoutConfigurationData("Hierarchic", new HierarchicLayoutConfig()),
      new LayoutConfigurationData("Organic", new OrganicLayoutConfig()),
      new LayoutConfigurationData("Orthogonal", new OrthogonalLayoutConfig()),
      new LayoutConfigurationData("Circular", new CircularLayoutConfig()),
      new LayoutConfigurationData("Tree", new TreeLayoutConfig()),
      new LayoutConfigurationData("Classic Tree", new ClassicTreeLayoutConfig()),
      new LayoutConfigurationData("Balloon", new BalloonLayoutConfig()),
      new LayoutConfigurationData("Radial", new RadialLayoutConfig()),
      new LayoutConfigurationData("Series-Parallel", new SeriesParallelLayoutConfig()),
      new LayoutConfigurationData("Components", new ComponentLayoutConfig()),
      new LayoutConfigurationData("Tabular", new TabularLayoutConfig()),
      new Separator(),
      new LayoutConfigurationData("Edge Router", new PolylineEdgeRouterConfig()),
      new LayoutConfigurationData("Channel Router", new ChannelEdgeRouterConfig()),
      new LayoutConfigurationData("Bus Router", new BusEdgeRouterConfig()),
      new LayoutConfigurationData("Organic Router", new OrganicEdgeRouterConfig()),
      new LayoutConfigurationData("Parallel Router", new ParallelEdgeRouterConfig()),
      new Separator(),
      new LayoutConfigurationData("Labeling", new LabelingConfig()),
      new LayoutConfigurationData("Partial", new PartialLayoutConfig()),
      new LayoutConfigurationData("Graph Transform", new GraphTransformerConfig()),
    };

    private void InitializeLayoutAlgorithms() {
      LayoutComboBox.ItemsSource = layouts;
    }

    private void InitializeSampleComboBox() {
      var lcd = layouts.OfType<LayoutConfigurationData>().ToDictionary(d => d.Name, d => d);
      var samples = new object[] {
        new SampleData("Hierarchic", lcd["Hierarchic"]),
        new SampleData("Grouping", lcd["Hierarchic"]),
        new SampleData("Organic", lcd["Organic"]),
        new SampleData("Orthogonal", lcd["Orthogonal"]),
        new SampleData("Circular", lcd["Circular"]),
        new SampleData("Tree", lcd["Tree"]),
        new SampleData("Classic Tree", lcd["Classic Tree"]),
        new SampleData("Balloon", lcd["Balloon"]),
        new SampleData("Radial", lcd["Radial"]),
        new SampleData("Series-Parallel", lcd["Series-Parallel"]),
        new SampleData("Edge Router", lcd["Edge Router"]),
        new SampleData("Bus Router", lcd["Bus Router"]),
        new SampleData("Label Placement", lcd["Labeling"]),
        new SampleData("Components", lcd["Components"]),
        new SampleData("Tabular", lcd["Tabular"]),
        new Separator(),
        new SampleData("Organic with Sub-structures", lcd["Organic"]),
        new SampleData("Hierarchic with Sub-components", lcd["Hierarchic"]),
        new SampleData("Orthogonal with Sub-structures", lcd["Orthogonal"]),
        new SampleData("Hierarchic with Buses", lcd["Hierarchic"]),
        new SampleData("Edge Router with Buses", lcd["Edge Router"]),
      };
      SampleComboBox.ItemsSource = samples;
      SampleComboBox.SelectedIndex = 0;
    }

    private void ResetConfig() {
      var data = (LayoutConfigurationData) LayoutComboBox.SelectedItem;
      // Re-create the same LayoutConfiguration object from scratch
      data.Configuration = (LayoutConfiguration) Activator.CreateInstance(data.Configuration.GetType());
      Editor.GetBindingExpression(ConfigurationEditor.ConfigurationProperty).UpdateTarget();
    }

    /// <summary>
    /// Called when the current layout changes, updates UI and applies special settings
    /// </summary>
    private void OnLayoutChanged(object sender, EventArgs e) {
      if (SampleComboBox.SelectedItem == null || LayoutComboBox.SelectedItem == null) {
        return;
      }
      var sampleGraphKey = ((SampleData) SampleComboBox.SelectedItem).Name;
      var data = (LayoutConfigurationData) LayoutComboBox.SelectedItem;
      var config = data.Configuration;
      SetShowTargetArrowState(IsLayoutDirected(data.Name));
      if (data.Name.StartsWith("Hierarchic")) {
        // enable edge-thickness buttons only for Hierarchic Layout
        SetExtraButtonState(true);
      } else {
        // disable edge-thickness buttons only for all other layouts
        SetExtraButtonState(false);
        ResetEdgeDirectedness();
        ResetEdgeThickness();
      }
      if (sampleGraphKey == "Organic with Sub-structures" && data.Name == "Organic") {
        ((OrganicLayoutConfig) config).EnableSubstructures();
      }
      if (sampleGraphKey == "Hierarchic with Sub-components" && data.Name == "Hierarchic") {
        ((HierarchicLayoutConfig) config).EnableSubComponents();
      }
      if (sampleGraphKey == "Orthogonal with Sub-structures" && data.Name == "Orthogonal") {
        ((OrthogonalLayoutConfig) config).EnableSubstructures();
      }
      if (sampleGraphKey == "Hierarchic with Buses" && data.Name == "Hierarchic") {
        ((HierarchicLayoutConfig) config).EnableAutomaticBusRouting();
      }
      if (sampleGraphKey == "Edge Router with Buses" && data.Name == "Edge Router") {
        ((PolylineEdgeRouterConfig) config).BusRoutingItem = PolylineEdgeRouterConfig.EnumBusRouting.ByColor;
      }
    }

    private void SetShowTargetArrowState(bool isLayoutDirected) {
      var currentStyle = Graph.EdgeDefaults.Style as PolylineEdgeStyle;
      if (currentStyle == null) {
        Graph.EdgeDefaults.Style = new PolylineEdgeStyle();
      }
      if (currentStyle != null) {
        currentStyle.TargetArrow = isLayoutDirected ? Arrows.Default : Arrows.None;
        Graph.EdgeDefaults.Style = currentStyle;
      }
    }

    /// <summary>
    /// Returns whether or not the current layout algorithm considers edge directions.
    /// </summary>
    private static bool IsLayoutDirected(string key) {
      return key != "Organic" && key != "Orthogonal" && key != "Circular";
    }

    private void SetExtraButtonState(bool enabled) {
      GenerateRandomEdgeThicknessButton.IsEnabled = enabled;
      ResetAllEdgeThicknessButton.IsEnabled = enabled;
      GenerateRandomEdgeDirectionsButton.IsEnabled = enabled;
      ResetAllEdgeDirectionsButton.IsEnabled = enabled;
    }

    /// <summary>
    /// Actually applies the layout.
    /// </summary>
    /// <param name="clearUndo">A value determining whether to clear the Undo queue after layout.</param>
    private async Task ApplyLayout(bool clearUndo) {
      var data = (LayoutConfigurationData) LayoutComboBox.SelectedItem;
      var config = data.Configuration;
      SetUserInteractionState(false);
      await config.Apply(graphControl, () => { });
      SetUserInteractionState(true);
      CheckSampleButtonStates();
      if (clearUndo) {
        Graph.GetUndoEngine().Clear();
      }
    }

    private void SetUserInteractionState(bool state) {
      PreviousSample.IsEnabled = state;
      NextSample.IsEnabled = state;
      ApplyButton.IsEnabled = state;
      ResetButton.IsEnabled = state;
      SampleComboBox.IsEnabled = state;
    }

    private void CheckSampleButtonStates() {
      var maxReached = SampleComboBox.SelectedIndex == SampleComboBox.Items.Count - 1;
      var minReached = SampleComboBox.SelectedIndex == 0;
      NextSample.IsEnabled = true;
      PreviousSample.IsEnabled = true;
      if (maxReached) {
        NextSample.IsEnabled = false;
      } else if (minReached) {
        PreviousSample.IsEnabled = false;
      }
    }

    private async void OnSampleChanged(object sender, SelectionChangedEventArgs e) {
      var data = SampleComboBox.SelectedItem as SampleData;
      if (data == null) {
        return;
      }
      var selectedKey = data.Name;
      var fileName = "Resources/" + selectedKey.ToLower().Replace(" ", "").Replace("-", "") +
                     ".graphml";
      graphControl.GraphMLIOHandler.ClearGraphBeforeRead = true;
      graphControl.GraphMLIOHandler.Read(Graph, fileName);
      graphControl.FitGraphBounds();
      // Force re-evaluation of special samples and their layouts for sub-structure layouts.
      OnLayoutChanged(sender, e);
      await ApplyLayout(true);
    }

    /// <summary>
    /// Creates the default input mode for the GraphControl, a <see cref="GraphEditorInputMode" />.
    /// </summary>
    /// <returns>a new GraphEditorInputMode instance with added event listeners and demo-specific settings such as no labels</returns>
    protected virtual IInputMode CreateEditorInputMode() {
      // create default interaction with snapping and orthogonal edge editing
      var mode = new GraphEditorInputMode {
        AllowGroupingOperations = true,
        SnapContext = new GraphSnapContext { Enabled = false, GridSnapType = GridSnapTypes.None },
        LabelSnapContext = new LabelSnapContext { Enabled = false },
        OrthogonalEdgeEditingContext = new OrthogonalEdgeEditingContext { Enabled = false },
        NavigationInputMode = { AllowCollapseGroup = false, AllowExpandGroup = false }
      };

      // make bend creation more important than moving of selected edges
      // this has the effect that dragging a selected edge (not its bends)
      // will create a new bend instead of moving all bends
      // This is especially nicer in conjunction with orthogonal
      // edge editing because this creates additional bends every time
      // the edge is moved otherwise
      mode.CreateBendInputMode.Priority = mode.MoveInputMode.Priority - 1;

      // also we add a context menu
      InitializeContextMenu(mode);

      //set the input mode
      graphControl.InputMode = mode;

      return mode;
    }

    private void InitializeContextMenu(GraphEditorInputMode mode) {
      mode.PopulateItemContextMenu += (sender, args) => {
        args.Menu.Items.Clear();
        var hitNode = args.Item as INode;
        var hitEdge = args.Item as IEdge;
        var selection = GraphEditorInputMode.GraphSelection;
        if (hitNode != null) {
          var selectNodeMenu = new MenuItem { Header = "Select All Nodes" };
          selectNodeMenu.Click += delegate {
            selection.Clear();
            foreach (var node in Graph.Nodes) {
              selection.SetSelected(node, true);
            }
          };
          args.Menu.Items.Add(selectNodeMenu);
        } else if (hitEdge != null) {
          var selectEdgeMenu = new MenuItem { Header = "Select All Edges" };
          selectEdgeMenu.Click += delegate {
            selection.Clear();
            foreach (var edge in Graph.Edges) {
              selection.SetSelected(edge, true);
            }
          };
          args.Menu.Items.Add(selectEdgeMenu);
        } else {
          args.Menu.Items.Add(new MenuItem { Command = ApplicationCommands.SelectAll, CommandTarget = graphControl });
        }
        // if one or more nodes are selected: add options to cut and copy
        if (selection.SelectedNodes.Count > 0) {
          args.Menu.Items.Add(new MenuItem { Command = ApplicationCommands.Copy, CommandTarget = graphControl });
          args.Menu.Items.Add(new MenuItem { Command = ApplicationCommands.Cut, CommandTarget = graphControl });
        }
        if (!graphControl.Clipboard.Empty) {
          args.Menu.Items.Add(new MenuItem { Command = ApplicationCommands.Paste, CommandTarget = graphControl });
        }
        // finally, if the context menu has at least one entry, set the showMenu flag
        if (args.Menu.Items.Count > 0) {
          args.ShowMenu = true;
        }
      };
    }

    /// <summary>
    /// Initializes the graph instance setting default styles,
    /// load the sample graph and route its edges.
    /// </summary>
    protected virtual void InitializeGraph() {
      // Enable undo support
      Graph.SetUndoEngineEnabled(true);

      // set some nice default styles
      Graph.NodeDefaults.Style = new ShinyPlateNodeStyle {Brush = Brushes.Orange};

      // get a hold of the group node defaults
      var groupNodeDefaults = Graph.GroupNodeDefaults;

      // configure the group node style.
      //PanelNodeStyle is a nice style especially suited for group nodes
      Color groupNodeColor = Color.FromArgb(255, 214, 229, 248);
      groupNodeDefaults.Style = new PanelNodeStyle {
          Color = groupNodeColor,
          Insets = new InsetsD(5),
          LabelInsetsColor = groupNodeColor
      };
    }

    /// <summary>
    /// Apply the layout and keep undo state
    /// </summary>
    private async void ApplyButtonClick(object sender, RoutedEventArgs e) {
      await ApplyLayout(false);
    }

    /// <summary>
    /// Reset the settings by creating a new config instance 
    /// </summary>
    private void ResetButtonClick(object sender, RoutedEventArgs e) {
      ResetConfig();
    }

    /// <summary>
    /// Returns the GraphControl instance used in the form.
    /// </summary>
    private GraphControl GraphControl {
      get { return graphControl; }
    }

    /// <summary>
    /// Gets the currently registered IGraph instance from the GraphControl.
    /// </summary>
    private IGraph Graph {
      get { return GraphControl.Graph; }
    }

    private void NewFileButtonClick(object sender, ExecutedRoutedEventArgs e) {
      Graph.Clear();
    }

    private void PreviousSample_OnClick(object sender, RoutedEventArgs e) {
      if (SampleComboBox.SelectedIndex <= 0) {
        return;
      }
      //make sure you can not get into invalid state
      do {
        SampleComboBox.SelectedIndex--;
      } while (SampleComboBox.SelectedItem is Separator);
    }

    private void NextSample_OnClick(object sender, RoutedEventArgs e) {
      if (SampleComboBox.SelectedIndex >= SampleComboBox.Items.Count - 1) {
        return;
      }
      //make sure you can not get into invalid state
      do {
        SampleComboBox.SelectedIndex++;
      } while (SampleComboBox.SelectedItem is Separator);
    }

    private void ToggleSnapLines(object sender, RoutedEventArgs e) {
      GraphSnapContext.Enabled = !GraphSnapContext.Enabled;
      LabelSnapContext.Enabled = !LabelSnapContext.Enabled;
    }

    private void ToggleOrthogonalEdges(object sender, RoutedEventArgs e) {
      var newValue = !GraphEditorInputMode.OrthogonalEdgeEditingContext.Enabled;
      GraphEditorInputMode.OrthogonalEdgeEditingContext.Enabled = newValue;
    }

    /// <summary>
    ///  Returns the <see cref="yWorks.Controls.Input.GraphSnapContext"/> of the <see cref="GraphEditorInputMode" />.
    /// </summary>
    internal GraphSnapContext GraphSnapContext {
      get { return (GraphSnapContext) GraphEditorInputMode.SnapContext; }
    }

    /// <summary>
    ///  Returns the <see cref="yWorks.Controls.Input.LabelSnapContext"/> of the <see cref="GraphEditorInputMode" />.
    /// </summary>
    internal LabelSnapContext LabelSnapContext {
      get { return (LabelSnapContext) GraphEditorInputMode.LabelSnapContext; }
    }

    /// <summary>
    /// The <see cref="GraphEditorInputMode" /> used for graph editing.
    /// </summary>
    internal GraphEditorInputMode GraphEditorInputMode {
      get { return (GraphEditorInputMode) graphControl.InputMode; }
    }

    private void RemoveLabels(List<ILabelOwner> items) {
      //remove all existing item labels
      items.ForEach(item => {
        //store labels in a seperate list to not modify while iterating
        foreach (var label in item.Labels.ToList()) {
          Graph.Remove(label);
        }
      });
    }

    private void GenerateRandomNodeLabels(object sender, RoutedEventArgs routedEventArgs) {
      GenerateRandomLabels(Graph.Nodes.Select(item => (ILabelOwner) item).ToList());
    }

    private void GenerateRandomEdgeLabels(object sender, RoutedEventArgs routedEventArgs) {
      GenerateRandomLabels(Graph.Edges.Select(item => (ILabelOwner) item).ToList());
    }

    private void RemoveAllLabels(object sender, RoutedEventArgs routedEventArgs) {
      RemoveLabels(Graph.Edges.Select(item => (ILabelOwner) item).ToList());
      RemoveLabels(Graph.Nodes.Select(item => (ILabelOwner) item).ToList());
    }

    private void GenerateRandomEdgeThickness(object sender, RoutedEventArgs routedEventArgs) {
      foreach (var edge in Graph.Edges) {
        var oldStyle = edge.Style;
        var thickness = random.NextDouble() * 4 + 1;
        var style = (PolylineEdgeStyle) oldStyle.Clone();
        style.Pen = style.Pen.Clone();
        style.Pen.Thickness = thickness;
        if (style.Pen.CanFreeze) {
          style.Pen.Freeze();
        }
        Graph.SetStyle(edge, style);
      }
    }

    private void ResetEdgeThickness(object sender, RoutedEventArgs e) {
      ResetEdgeThickness();
    }

    private void ResetEdgeThickness() {
      foreach (var edge in Graph.Edges) {
        var polyStyle = edge.Style as PolylineEdgeStyle;
        if (polyStyle == null) {
          continue;
        }
        var newStyle = (PolylineEdgeStyle) polyStyle.Clone();
        newStyle.Pen = newStyle.Pen.Clone();
        newStyle.Pen.Thickness = 1;
        if (newStyle.Pen.CanFreeze) {
          newStyle.Pen.Freeze();
        }
        Graph.SetStyle(edge, newStyle);
      }
    }

    private void GenerateRandomEdgeDirectedness(object sender, RoutedEventArgs routedEventArgs) {
      foreach (var edge in Graph.Edges) {
        var directed = random.Next(2) != 0;
        var style = edge.Style as PolylineEdgeStyle;
        if (style != null) {
          var newStyle = (PolylineEdgeStyle) style.Clone();
          newStyle.TargetArrow = directed ? Arrows.Default : Arrows.None;
          Graph.SetStyle(edge, newStyle);
        }
      }
    }

    private void ResetEdgeDirectedness(object sender, RoutedEventArgs e) {
      ResetEdgeDirectedness(true);
    }

    private void ResetEdgeDirectedness(bool directed = false) {
      foreach (var edge in Graph.Edges) {
        var style = edge.Style as PolylineEdgeStyle;
        if (style != null) {
          style.TargetArrow = directed || !style.TargetArrow.Equals(Arrows.None) ? Arrows.Default : Arrows.None;
        }
      }
      Graph.InvalidateDisplays();
    }

    private void GenerateRandomLabels(List<ILabelOwner> items) {
      const int wordCountMin = 1;
      const int wordCountMax = 3;
      const double labelPercMin = 0.2;
      const double labelPercMax = 0.7;
      var labelCount = random.Next((int) (items.Count * labelPercMin), (int) (items.Count * labelPercMax));

      RemoveLabels(items);

      var itemsToLabel = items.OrderBy(item => random.Next()).Take(labelCount);

      // add random item labels
      foreach (var item in itemsToLabel) {
        var wordCount = random.Next(wordCountMin, wordCountMax + 1);
        var label = string.Join(" ", Enumerable.Range(0, wordCount).Select(i => LoremIpsum[random.Next(LoremIpsum.Length)]));
        Graph.AddLabel(item, label);
      }
    }

    private static string[] LoremIpsum = {
      "lorem", "ipsum", "dolor", "sit", "amet", "consectetur", "adipiscing", "elit", "donec", "felis", "erat",
      "malesuada", "quis", "ipsum", "et", "condimentum", "ultrices", "orci", "nullam", "interdum", "vestibulum", "eros",
      "sed", "porta", "donec", "ac", "eleifend", "dolor", "at", "dictum", "ipsum", "pellentesque", "vel", "suscipit",
      "mi", "nullam", "aliquam", "turpis", "et", "dolor", "porttitor", "varius", "nullam", "vel", "arcu", "rutrum",
      "iaculis", "est", "sit", "amet", "rhoncus", "turpis", "vestibulum", "lacinia", "sollicitudin", "urna", "nec",
      "vestibulum", "nulla", "id", "lacinia", "metus", "etiam", "ac", "felis", "rutrum", "sollicitudin", "erat",
      "vitae", "egestas", "tortor", "curabitur", "quis", "libero", "aliquet", "mattis", "mauris", "nec", "tempus",
      "nibh", "in", "at", "lectus", "luctus", "mattis", "urna", "pretium", "eleifend", "lacus", "sed", "interdum",
      "sapien", "nec", "justo", "vestibulum", "non", "scelerisque", "nibh", "sollicitudin", "interdum", "et",
      "malesuada", "fames", "ac", "ante", "ipsum", "primis", "in", "faucibus", "vivamus", "congue", "tristique",
      "magna", "quis", "elementum", "phasellus", "sit", "amet", "tristique", "massa", "vestibulum", "eu", "leo",
      "vitae", "quam", "dictum", "venenatis", "eu", "id", "nibh", "donec", "eget", "eleifend", "felis", "nulla", "ac",
      "suscipit", "ante", "et", "sollicitudin", "dui", "mauris", "in", "pulvinar", "tortor", "vestibulum", "pulvinar",
      "arcu", "vel", "tellus", "maximus", "blandit", "morbi", "sed", "sem", "vehicula", "fermentum", "nisi", "eu",
      "fringilla", "metus", "duis", "ut", "quam", "eget", "odio", "hendrerit", "finibus", "ut", "a", "lectus", "cras",
      "ullamcorper", "turpis", "in", "purus", "facilisis", "vestibulum", "donec", "maximus", "ac", "tortor", "tempus",
      "egestas", "aenean", "est", "diam", "dictum", "et", "sodales", "vel", "efficitur", "ac", "libero", "vivamus",
      "vehicula", "ligula", "eu", "diam", "auctor", "at", "dapibus", "nulla", "pellentesque", "morbi", "et", "dapibus",
      "dolor", "quis", "auctor", "turpis", "nunc", "sed", "pretium", "diam", "quisque", "non", "massa", "consectetur",
      "tempor", "augue", "vel", "volutpat", "ex", "vivamus", "vestibulum", "dolor", "risus", "quis", "mollis", "urna",
      "fermentum", "sed", "sed", "porttitor", "venenatis", "volutpat", "nulla", "facilisi", "donec", "aliquam", "mi",
      "vitae", "ligula", "dictum", "ornare", "suspendisse", "finibus", "ligula", "vitae", "congue", "iaculis", "donec",
      "vestibulum", "erat", "vel", "tortor", "iaculis", "tempor", "vivamus", "et", "purus", "eu", "ipsum", "rhoncus",
      "pretium", "sit", "amet", "nec", "nisl", "nunc", "molestie", "consectetur", "rhoncus", "duis", "ex", "nunc",
      "interdum", "at", "molestie", "quis", "blandit", "quis", "diam", "nunc", "imperdiet", "lorem", "vel",
      "scelerisque", "facilisis", "eros", "massa", "auctor", "nisl", "vitae", "efficitur", "leo", "diam", "vel",
      "felis", "aliquam", "tincidunt", "dapibus", "arcu", "in", "pulvinar", "metus", "tincidunt", "et", "etiam",
      "turpis", "ligula", "sodales", "a", "eros", "vel", "fermentum", "imperdiet", "purus", "fusce", "mollis", "enim",
      "sed", "volutpat", "blandit", "arcu", "orci", "iaculis", "est", "non", "iaculis", "lorem", "sapien", "sit",
      "amet", "est", "morbi", "ut", "porttitor", "elit", "aenean", "ac", "sodales", "lectus", "morbi", "ut", "bibendum",
      "arcu", "maecenas", "tincidunt", "erat", "vel", "maximus", "pellentesque", "ut", "placerat", "quam", "sem", "a",
      "auctor", "ligula", "imperdiet", "quis", "pellentesque", "gravida", "consectetur", "urna", "suspendisse", "vitae",
      "nisl", "et", "ante", "ornare", "vulputate", "sed", "a", "est", "lorem", "ipsum", "dolor", "sit", "amet",
      "consectetur", "adipiscing", "elit", "sed", "eu", "facilisis", "lectus", "nullam", "iaculis", "dignissim", "eros",
      "eget", "tincidunt", "metus", "viverra", "at", "donec", "nec", "justo", "vitae", "risus", "eleifend", "imperdiet",
      "eget", "ut", "ante", "ut", "arcu", "ex", "convallis", "in", "lobortis", "at", "mattis", "sed", "velit", "ut",
      "viverra", "ultricies", "lacus", "suscipit", "feugiat", "eros", "luctus", "et", "vestibulum", "et", "aliquet",
      "mauris", "quisque", "convallis", "purus", "posuere", "aliquam", "nulla", "sit", "amet", "posuere", "orci",
      "nullam", "sed", "iaculis", "mauris", "ut", "volutpat", "est", "suspendisse", "in", "vestibulum", "felis",
      "nullam", "gravida", "nulla", "at", "varius", "fringilla", "ipsum", "ipsum", "finibus", "lectus", "nec",
      "vestibulum", "lorem", "arcu", "ut", "magna", "aliquam", "aliquam", "erat", "erat", "ac", "euismod", "orci",
      "iaculis", "blandit", "morbi", "tincidunt", "posuere", "mi", "non", "eleifend", "vivamus", "accumsan", "dolor",
      "magna", "in", "cursus", "eros", "malesuada", "eu", "sed", "auctor", "consectetur", "tempus", "maecenas",
      "luctus", "turpis", "a"
    };

    private class LayoutConfigurationData
    {
      public string Name { get; private set; }
      public LayoutConfiguration Configuration { get; set; }

      public LayoutConfigurationData(string name, LayoutConfiguration configuration) {
        Name = name;
        Configuration = configuration;
      }
    }

    private class SampleData
    {
      public string Name { get; private set; }

      public LayoutConfigurationData ConfigurationData { get; private set; }

      public SampleData(string name, LayoutConfigurationData configurationData) {
        Name = name;
        ConfigurationData = configurationData;
      }
    }
  }
}
