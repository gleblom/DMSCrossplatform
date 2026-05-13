// using System;
// using System.Collections.Generic;
// using System.Threading;
// using Avalonia;
// using Avalonia.Controls;
// using Avalonia.Media;
//
// namespace DMSCrossplatform.Pdf.Rendering;
//
// public class TiledPdfPageControl : Control
// {
//     private const int TileSize = 512;
//
//     private readonly Dictionary<(int x, int y), PdfTile> _tiles = new();
//
//     public int PageIndex { get; set; }
//
//     public double Scale { get; set; } = 1.0;
//
//     public TileCache Cache { get; set; } = new();
//
//     public override void Render(DrawingContext context)
//     {
//         base.Render(context);
//
//         var viewport = new Rect(Bounds.Size);
//
//         int cols = (int)Math.Ceiling(Bounds.Width / TileSize);
//         int rows = (int)Math.Ceiling(Bounds.Height / TileSize);
//
//         for (int y = 0; y < rows; y++)
//         for (int x = 0; x < cols; x++)
//         {
//             var key = (x, y);
//
//             if (!_tiles.TryGetValue(key, out var tile))
//             {
//                 tile = new PdfTile(x, y, Scale);
//                 _tiles[key] = tile;
//             }
//
//             var rect = tile.GetBounds(TileSize);
//
//             if (!rect.Intersects(viewport))
//                 continue;
//
//             if (tile.Bitmap != null)
//             {
//                 context.DrawBitmap(
//                     tile.Bitmap,
//                     new Rect(0, 0, rect.Width, rect.Height),
//                     rect);
//             }
//             else
//             {
//                 RequestTile(tile);
//             }
//         }
//     }
//     private void RequestTile(PdfTile tile)
//     {
//         if (tile.IsRendering)
//             return;
//
//         tile.IsRendering = true;
//
//         _queue.Enqueue(async () =>
//         {
//             var rect = tile.GetBounds(TileSize);
//
//             var bmp = await _tileRasterizer.RasterizeTileAsync(
//                 PageIndex,
//                 rect,
//                 Scale,
//                 CancellationToken.None);
//
//             await Dispatcher.UIThread.InvokeAsync(() =>
//             {
//                 tile.Bitmap = bmp;
//                 tile.IsRendering = false;
//
//                 InvalidateVisual();
//             });
//         });
//     }
// }