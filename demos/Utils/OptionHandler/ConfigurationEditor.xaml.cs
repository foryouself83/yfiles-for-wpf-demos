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
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Markup;

namespace Demo.yFiles.Toolkit.OptionHandler
{
    /// <summary>
    /// Creates an UI view for an option, to adjust configuration of it
    /// </summary>
    public partial class ConfigurationEditor : UserControl
    {
        /// <summary>
        /// The underlying configuration
        /// </summary>
        private object _configuration;

        /// <summary>
        /// List contains a tuple of the element and the relevant property that has a binding
        /// The binding will be refreshed on any property change on UI level
        /// </summary>
        private readonly List<Tuple<FrameworkElement, DependencyProperty>> _elementsToCheckList = new List<Tuple<FrameworkElement, DependencyProperty>>();

        /// <summary>
        /// Automatically generated by Visual Studio.
        /// </summary>
        public ConfigurationEditor()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty ConfigurationProperty = DependencyProperty.Register(
          "Configuration", typeof(object), typeof(ConfigurationEditor), new PropertyMetadata(default(object), OnConfigurationChanged));

        public object Configuration {
          get { return (object) GetValue(ConfigurationProperty); }
          set { SetValue(ConfigurationProperty, value); }
        }

        private static void OnConfigurationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
          var self = (ConfigurationEditor) d;
          //reset ui view
          self.Content.Children.Clear();

          if (e.NewValue == null) {
            return;
          }

          //convert the given object into an option
          var option = new ConfigConverter().Convert(e.NewValue);

          //get a ui element with the create method
          var uiElement = self.Create(option);

          //add the ui element to our view
          self.Content.Children.Add(uiElement);

          var itemsControl = uiElement as Panel;

          //if we have a panel, set its first expander to expanded
          if (itemsControl != null)
          {
            var expander = itemsControl.Children[0] as Expander;
            if (expander != null)
            {
              expander.IsExpanded = true;
            }
          }

          //Called to initialise the UI view, refreshes the isEnabled-states of relevant elements
          self.SomethingChanged(self, EventArgs.Empty);
        }

        /// <summary>
        /// The public configuration property that should be set with a configuration object,
        /// Will update view, when set.
        /// </summary>

        /// <summary>
        /// Create an UI element that represents the given option with its option.ComponentType
        /// </summary>
        private UIElement Create(Option option, int groupDepth = 0)
        {
            switch (option.ComponentType)
            {
                case ComponentTypes.OptionGroup:
                    var group = option as OptionGroup;
                    var stackPanel = new StackPanel();
                    foreach (var child in group.ChildOptions)
                    {
                        stackPanel.Children.Add(Create(child, groupDepth + 1));
                    }

                    switch (groupDepth)
                    {
                        case 0:
                            return stackPanel;
                        case 1:
                            return new Expander
                            {
                                Header = group.Label,
                                Content = stackPanel
                            };
                        default:
                            return new GroupBox {Header = group.Label, Content = stackPanel};
                    }
                case ComponentTypes.Checkbox:
                    var checkBox = new CheckBox {DataContext = option};
                    checkBox.Content = option.Label;
                    
                    //property changed will be called if clicked
                    checkBox.Checked += SomethingChanged;
                    checkBox.Unchecked += SomethingChanged;

                    //add bindings for isChecked value and isEnabled
                    BindingOperations.SetBinding(checkBox, ToggleButton.IsCheckedProperty,
                        new Binding("Value") {Mode = BindingMode.TwoWay});
                    BindingOperations.SetBinding(checkBox, IsEnabledProperty,
                      new Binding("IsEnabled"));

                    _elementsToCheckList.Add(Tuple.Create((FrameworkElement)checkBox, ToggleButton.IsCheckedProperty));
                    _elementsToCheckList.Add(Tuple.Create((FrameworkElement)checkBox, IsEnabledProperty));

                    return checkBox;
                case ComponentTypes.Combobox:
                  var comboBoxGrid = new Grid {Margin = new Thickness(0, 4, 0, 4)};
                  ColumnDefinition labelCol = new ColumnDefinition() {MinWidth = 50};
                  ColumnDefinition comboBoxCol = new ColumnDefinition() {Width = GridLength.Auto};
                  comboBoxGrid.ColumnDefinitions.Add(labelCol);
                  comboBoxGrid.ColumnDefinitions.Add(comboBoxCol);

                  var boxLabel = new TextBlock {Text = option.Label, TextWrapping = TextWrapping.Wrap, Margin = new Thickness(5)};
                  var comboBox = new ComboBox
                  {
                        DataContext = option,
                        ItemsSource = option.EnumValues,
                        SelectedIndex = 0,
                        DisplayMemberPath = "Name",
                        SelectedValuePath = "Value"
                  };
                  Grid.SetColumn(comboBox,1);
                    
                  //add bindings for selected value and isEnabled
                  BindingOperations.SetBinding(comboBox, Selector.SelectedValueProperty,
                        new Binding("Value") {Mode = BindingMode.TwoWay});
                  BindingOperations.SetBinding(comboBox, IsEnabledProperty, new Binding("IsEnabled"));
                  
                  _elementsToCheckList.Add(Tuple.Create((FrameworkElement) comboBox, Selector.SelectedValueProperty));
                  _elementsToCheckList.Add(Tuple.Create((FrameworkElement) comboBox, IsEnabledProperty));

                  comboBox.SelectionChanged += SomethingChanged;
                  comboBoxGrid.Children.Add(boxLabel);
                  comboBoxGrid.Children.Add(comboBox);
                  return comboBoxGrid;
                case ComponentTypes.FormattedText:
                    //parse the formattedText as a FlowDocument
                    var document = (FlowDocument) XamlReader.Parse(
                        "<FlowDocument xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\">" +
                        option.Value + "</FlowDocument>");
                    var block = new RichTextBox {Document = document};
                    return block;
                case ComponentTypes.Slider:
                  var sliderGrid = new Grid {Margin = new Thickness(0, 0, 0, 5)};
                  sliderGrid.RowDefinitions.Add(new RowDefinition());
                  sliderGrid.RowDefinitions.Add(new RowDefinition());
                  sliderGrid.ColumnDefinitions.Add(new ColumnDefinition { MinWidth = 50 });
                  sliderGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(45)});

                    Label label = new Label
                    {
                        Content = option.Label
                    };
                    var step = option.MinMax.Step.Equals(0.0) ? 1.0 : option.MinMax.Step;

                    Slider slider = new Slider
                    {
                        Minimum = option.MinMax.Min,
                        Maximum =  option.MinMax.Max,
                        Value = option.MinMax.Min,
                        TickFrequency = step,
                        IsSnapToTickEnabled = true,
                        DataContext = option,
                        HorizontalAlignment = HorizontalAlignment.Stretch
                    };

                    var box = new TextBox
                    {
                        TextAlignment = TextAlignment.Right,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        Width = 35
                    };

                    //value and isEnabled binding for the slider
                    var sliderBinding = new Binding("Value") {Mode = BindingMode.TwoWay, Converter = new IntConverter(), ConverterParameter = option.ValueType};
                    BindingOperations.SetBinding(slider, RangeBase.ValueProperty, sliderBinding);
                    BindingOperations.SetBinding(slider, IsEnabledProperty, new Binding("IsEnabled"));

                    //bind textBox to slider
                    Binding textBoxBinding = new Binding("Value"){Source = slider};
                    BindingOperations.SetBinding(box, TextBox.TextProperty, textBoxBinding);

                    //bind enabled state of box to slider
                    BindingOperations.SetBinding(box, IsEnabledProperty, new Binding("IsEnabled"){Source = slider});

                    //add tuple to the check list
                    _elementsToCheckList.Add(Tuple.Create((FrameworkElement)slider, RangeBase.ValueProperty));
                    _elementsToCheckList.Add(Tuple.Create((FrameworkElement)slider, IsEnabledProperty));
                    Grid.SetColumn(box, 1);
                    
                    Grid.SetRow(label, 0);
                    Grid.SetRow(slider, 1);
                    Grid.SetRow(box, 1);
                    Grid.SetColumnSpan(label,2);

                    sliderGrid.Children.Add(slider);
                    sliderGrid.Children.Add(box);

                    sliderGrid.Children.Add(label);
                    return sliderGrid;

                case ComponentTypes.Spinner:
                    var spinnerGrid = new Grid { Margin = new Thickness(0, 0, 0, 5) };
                    spinnerGrid.RowDefinitions.Add(new RowDefinition());
                    spinnerGrid.ColumnDefinitions.Add(new ColumnDefinition { MinWidth = 50 });
                    spinnerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(45) });

                    Label spinnerLabel = new Label
                    {
                        Content = option.Label
                    };

                    // there is no spinner control in standard WPF so use a Textbox instead
                    TextBox tb = new TextBox(){IsEnabled = false, DataContext = option};
                    // restrict input to something that is parseable into a Double
                    var spinnerRule = new SpinnerRule();
                    if (option.MinMax != null) {
                      spinnerRule.Min = option.MinMax.Min;
                      spinnerRule.Max = option.MinMax.Max;
                    }
                    var spinnerBinding = new Binding("Value") {Mode = BindingMode.TwoWay, Converter = new IntConverter(), ConverterParameter = option.ValueType, ValidationRules = {spinnerRule}};
                    BindingOperations.SetBinding(tb, TextBox.TextProperty,  spinnerBinding);
                    BindingOperations.SetBinding(tb, IsEnabledProperty, new Binding("IsEnabled"));

                    _elementsToCheckList.Add(Tuple.Create((FrameworkElement) tb, IsEnabledProperty));

                    Grid.SetRow(spinnerLabel, 0);
                    Grid.SetColumn(spinnerLabel, 0);
                    Grid.SetRow(tb, 0);
                    Grid.SetColumn(tb, 1);

                    spinnerGrid.Children.Add(spinnerLabel);
                    spinnerGrid.Children.Add(tb);

                    return spinnerGrid;
                default:
                    return new Label {Content = option.Label};
            }
        }

        /// <summary>
        /// Called when a setting is changed which may result in a UIElements isEnabledProperty to change
        /// </summary>
        private void SomethingChanged(object sender, EventArgs e) {
            foreach (var tuple in _elementsToCheckList) {
                var element = tuple.Item1;
                var property = tuple.Item2;

                element.GetBindingExpression(property).UpdateTarget();
            }
        }

        private class SpinnerRule : ValidationRule
        {
          private bool _rangeset = false;
          private double _min = 0;
          private double _max = 0;
          public double Max {
            get {
              return _max;
            }
            set {
              _max = value;
              _rangeset = true;
            }
          }
          public double Min {
            get {
              return _min;
            }
            set { _min = value;
              _rangeset = true;
            }
          }
          public override ValidationResult Validate(object value, CultureInfo cultureInfo) {
            double result = 0;
            try {
              if (((string) value).Length > 0)
                result = Double.Parse((String) value);
            } catch (Exception e) {
              return new ValidationResult(false, "Illegal characters or " + e.Message);
            }

            if (_rangeset & ((result < Min) || (result > Max))) {
              return new ValidationResult(false,
                  "Value out of range: " + Min + " - " + Max + ".");
            } else {
              return ValidationResult.ValidResult;
            }
          }
        }

        /// <summary>
        /// Helper class to use with Bindings and custom Types (not only double)
        /// </summary>
        private sealed class IntConverter : IValueConverter
        {
            public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            {
                return value;
            }

            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            {
                return System.Convert.ChangeType(value, (Type) parameter, culture);
            }
        }
    }
}
