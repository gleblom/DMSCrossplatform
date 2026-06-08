using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Data.Converters;
using Avalonia.Media;
using DMSCrossplatform.Models.Dto;

namespace DMSCrossplatform.Views;

public partial class RouteGraphView : UserControl
{
    public static readonly StyledProperty<bool> IsEditorModeProperty =
        AvaloniaProperty.Register<RouteGraphView, bool>(nameof(IsEditorMode), false);

    private const double NodeWidth = 220;
    private const double NodeHeight = 86;
    private const double VerticalGap = 70;
    private const double LeftMargin = 24;
    private const double TopMargin = 16;

    public bool IsEditorMode
    {
        get => GetValue(IsEditorModeProperty);
        set => SetValue(IsEditorModeProperty, value);
    }

    public RouteGraphView()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
    }

    private void OnDataContextChanged(object? sender, EventArgs e)
    {
        DrawGraph(DataContext as RouteGraphDto);
    }

    private void DrawGraph(RouteGraphDto? graph)
    {
        if (GraphCanvas == null)
            return;

        GraphCanvas.Children.Clear();

        if (graph?.Nodes == null || graph.Nodes.Count == 0)
        {
            GraphCanvas.Width = NodeWidth + LeftMargin * 2;
            GraphCanvas.Height = NodeHeight + TopMargin * 2;
            return;
        }

        var orderedNodes = graph.Nodes.OrderBy(n => n.StepIndex).ToList();
        GraphCanvas.Width = NodeWidth + LeftMargin * 2;
        GraphCanvas.Height = TopMargin * 2 + orderedNodes.Count * NodeHeight + Math.Max(0, orderedNodes.Count - 1) * VerticalGap;

        foreach (var edge in graph.Edges)
        {
            var fromNode = graph.Nodes.FirstOrDefault(n => n.Id == edge.FromNodeId);
            var toNode = graph.Nodes.FirstOrDefault(n => n.Id == edge.ToNodeId);

            if (fromNode == null || toNode == null)
                continue;

            var from = GetNodeTopLeft(fromNode.StepIndex);
            var to = GetNodeTopLeft(toNode.StepIndex);

            GraphCanvas.Children.Add(new Line
            {
                StartPoint = new Point(from.X + NodeWidth / 2, from.Y + NodeHeight),
                EndPoint = new Point(to.X + NodeWidth / 2, to.Y),
                Stroke = Brushes.LightGray,
                StrokeThickness = 2
            });
        }

        foreach (var node in orderedNodes)
        {
            var border = CreateNode(node);
            var position = GetNodeTopLeft(node.StepIndex);
            Canvas.SetLeft(border, position.X);
            Canvas.SetTop(border, position.Y);
            GraphCanvas.Children.Add(border);
        }
    }

    private static Point GetNodeTopLeft(int stepIndex)
        => new(LeftMargin, TopMargin + stepIndex * (NodeHeight + VerticalGap));

    private static Border CreateNode(RouteGraphNodeDto node)
    {
        return new Border
        {
            Width = NodeWidth,
            MinHeight = NodeHeight,
            Background = new SolidColorBrush(Color.Parse("#7a7a7a")),
            BorderBrush = new SolidColorBrush(Color.Parse("#616161")),
            BorderThickness = new Thickness(2),
            CornerRadius = new CornerRadius(8),
            Padding = new Thickness(16, 12),
            Child = new StackPanel
            {
                Spacing = 4,
                Children =
                {
                    new TextBlock
                    {
                        Text = string.IsNullOrWhiteSpace(node.ApproverFullName) ? "[Не назначен]" : node.ApproverFullName,
                        FontWeight = FontWeight.SemiBold,
                        FontSize = 16,
                        TextWrapping = TextWrapping.Wrap
                    },
                    new TextBlock
                    {
                        Text = node.ApproverEmail ?? string.Empty,
                        FontSize = 14,
                        Opacity = 0.8,
                        FontWeight = FontWeight.SemiBold,
                        TextWrapping = TextWrapping.Wrap
                    },
                    new TextBlock
                    {
                        Text = $"Этап {node.StepIndex + 1}",
                        FontSize = 14,
                        FontWeight = FontWeight.SemiBold,
                        Opacity = 0.6,
                        Margin = new Thickness(0, 4, 0, 0)
                    }
                }
            }
        };
    }
}

public class LevelToXConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
    {
        if (value is int level)
        {
            const double nodeWidth = 220;
            const double horizontalSpacing = 100;
            return level * (nodeWidth + horizontalSpacing);
        }

        return 0;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class StepToYConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
    {
        if (value is int stepIndex)
        {
            const double nodeHeight = 80;
            const double verticalSpacing = 120;
            return stepIndex * (nodeHeight + verticalSpacing);
        }

        return 0;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
