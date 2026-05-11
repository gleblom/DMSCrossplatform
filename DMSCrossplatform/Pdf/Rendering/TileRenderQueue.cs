// using System;
// using System.Threading.Tasks;
// using Avalonia.Threading;
// using System.Collections.Concurrent;
// using System.Threading;
//
// namespace AvaloniaApplication1.Pdf.Rendering;
//
//
//
// public class TileRenderQueue
// {
//     private readonly SemaphoreSlim _semaphore = new(2);
//     private readonly ConcurrentQueue<Func<Task>> _queue = new();
//     public int PageIndex { get; set; }
//     private const int TileSize = 512;
//
//     public void Enqueue(Func<Task> job)
//     {
//         _queue.Enqueue(job);
//         Process();
//     }
//
//     private async void Process()
//     {
//         if (!_semaphore.Wait(0))
//             return;
//
//         try
//         {
//             while (_queue.TryDequeue(out var job))
//             {
//                 await job();
//             }
//         }
//         finally
//         {
//             _semaphore.Release();
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