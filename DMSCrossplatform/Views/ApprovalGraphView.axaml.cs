using System;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Media;
using DMSCrossplatform.Models.Dto;
using DMSCrossplatform.ViewModels;

namespace DMSCrossplatform.Views;

public partial class ApprovalGraphView : UserControl
{
    public static readonly StyledProperty<DocumentFullReadDto?> DocumentProperty =
        AvaloniaProperty.Register<ApprovalGraphView, DocumentFullReadDto?>(nameof(Document));

    public static readonly StyledProperty<ObservableCollection<MvDocumentApprovalReadDto>?> ApprovalHistoryProperty =
        AvaloniaProperty.Register<ApprovalGraphView, ObservableCollection<MvDocumentApprovalReadDto>?>(nameof(ApprovalHistory));

    private const double NodeWidth = 240;
    private const double NodeHeight = 92;
    private const double VerticalGap = 74;
    private const double LeftMargin = 24;
    private const double TopMargin = 16;

    public static readonly StyledProperty<RouteGraphDto> GraphDataProperty =
        AvaloniaProperty.Register<ApprovalGraphView, RouteGraphDto>(nameof(GraphData));

    public RouteGraphDto GraphData
    {
        get => GetValue(GraphDataProperty);
        set => SetValue(GraphDataProperty, value);
    }
    
    public DocumentFullReadDto? Document
    {
        get => GetValue(DocumentProperty);
        set => SetValue(DocumentProperty, value);
    }

    public ObservableCollection<MvDocumentApprovalReadDto>? ApprovalHistory
    {
        get => GetValue(ApprovalHistoryProperty);
        set => SetValue(ApprovalHistoryProperty, value);
    }

    public ApprovalGraphView()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
        PropertyChanged += OnPropertyChanged;
    }
    

    private void OnPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property == DocumentProperty 
            || e.Property == ApprovalHistoryProperty
            || e.Property == GraphDataProperty)
            DrawGraph(GraphData);
    }

    private void OnDataContextChanged(object? sender, EventArgs e)
    {
        DrawGraph(GraphData);
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
            border.Tapped += OnNodeTapped;

            var position = GetNodeTopLeft(node.StepIndex);
            Canvas.SetLeft(border, position.X);
            Canvas.SetTop(border, position.Y);
            GraphCanvas.Children.Add(border);
        }
    }

    private Border CreateNode(RouteGraphNodeDto node)
    {
        var (background, borderBrush) = GetNodeBrushes(node);

        return new Border
        {
            Width = NodeWidth,
            MinHeight = NodeHeight,
            Background = background,
            BorderBrush = borderBrush,
            BorderThickness = new Thickness(2),
            CornerRadius = new CornerRadius(8),
            Padding = new Thickness(16, 12),
            Cursor = new Cursor(StandardCursorType.Hand),
            DataContext = node,
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
                        TextWrapping = TextWrapping.Wrap,
                    },
                    new TextBlock
                    {
                        Text = $"Этап {node.StepIndex + 1}",
                        FontSize = 14,
                        Opacity = 0.6,
                        Margin = new Thickness(0, 6, 0, 0)
                    }
                }
            }
        };
    }

    private (IBrush Background, IBrush Border) GetNodeBrushes(RouteGraphNodeDto node)
    {
        var lastVersion = ApprovalHistory?.Max(a => a.VersionNumber) ?? 0;
        var approval = ApprovalHistory?.FirstOrDefault(
            a => a.StepIndex == node.StepIndex && a.VersionNumber == lastVersion);

        if (approval?.IsApproved == true)
            return (new SolidColorBrush(Color.Parse("#43b549")), new SolidColorBrush(Color.Parse("#4CAF50")));

        if (approval?.IsApproved == false)
            return (new SolidColorBrush(Color.Parse("#e35b5b")), new SolidColorBrush(Color.Parse("#F44336")));

        return (new SolidColorBrush(Color.Parse("#7a7a7a")), new SolidColorBrush(Color.Parse("#616161")));
    }

    private static Point GetNodeTopLeft(int stepIndex)
        => new(LeftMargin, TopMargin + stepIndex * (NodeHeight + VerticalGap));

    private void OnNodeTapped(object? sender, TappedEventArgs e)
    {
        if (sender is Border { DataContext: RouteGraphNodeDto node })
            ShowStepHistory(node.StepIndex);
    }

    private void ShowStepHistory(int stepIndex)
    {
        if (ApprovalHistory == null || StepHistoryControl == null || HistoryPanel == null)
            return;

        var stepApprovals = ApprovalHistory.Where(a => a.StepIndex == stepIndex).ToList();
        StepHistoryControl.ItemsSource = stepApprovals;
        HistoryPanel.IsVisible = true;
    }

    private void OnCloseHistory(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (HistoryPanel != null)
            HistoryPanel.IsVisible = false;
    }
}
