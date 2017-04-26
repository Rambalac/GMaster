namespace GMaster.Views
{
    using System;
    using System.IO;
    using System.Numerics;
    using System.Threading;
    using System.Threading.Tasks;
    using Core.Camera;
    using Core.Camera.LumixData;
    using Core.Tools;
    using Microsoft.Graphics.Canvas;
    using Microsoft.Graphics.Canvas.Geometry;
    using Microsoft.Graphics.Canvas.UI;
    using Microsoft.Graphics.Canvas.UI.Xaml;
    using Windows.Foundation;
    using Windows.UI;

    public class FrameRenderer : IDisposable
    {
        private readonly object frameDrawLock = new object();
        private readonly CanvasControl view;
        private FrameData currentFrame;

        private CanvasGeometry geomAim;
        private CanvasGeometry geomBox;
        private CanvasGeometry geomCross;
        private CanvasGeometry geomLine;
        private Rect imageRect;
        private Matrix3x2 imageTransform;

        private int updateBitmapFlag;

        public FrameRenderer(CanvasControl view)
        {
            this.view = view;
            view.CreateResources += View_CreateResources;
        }

        public event Action<Rect> ImageRectChanged2;

        public Rect ImageRect
        {
            get => imageRect;

            private set
            {
                if (value != imageRect)
                {
                    imageRect = value;
                    ImageRectChanged2?.Invoke(imageRect);
                }
            }
        }

        public bool IsReady => currentFrame != null;

        public ILutEffectGenerator LutEffect { get; set; }

        public void Dispose()
        {
            currentFrame?.Dispose();
        }

        public void Draw(CanvasDrawingSession session, double wW, double wH, double aspect, FocusAreas areas)
        {
            if (currentFrame != null)
            {
                lock (frameDrawLock)
                {
                    var iW = currentFrame.Bitmap.SizeInPixels.Width;
                    var iH = currentFrame.Bitmap.SizeInPixels.Height / aspect;

                    var scaleX = wW / iW;
                    var scaleY = wH / iH;

                    var scale = Math.Min(scaleX, scaleY);

                    var rH = iH * scale;
                    var rW = iW * scale;

                    ImageRect = new Rect((wW - rW) / 2, (wH - rH) / 2, rW, rH);
                    var mat = Matrix3x2.CreateScale((float)ImageRect.Width, (float)ImageRect.Height);
                    mat.Translation = new Vector2((float)ImageRect.X, (float)ImageRect.Y);
                    imageTransform = mat;
                    currentFrame.Draw(session, imageRect);
                }

                if (areas != null)
                {
                    foreach (var box in areas.Boxes)
                    {
                        var trans = new Vector2(box.X1, box.Y1);
                        var scale = new Vector2(box.Width, box.Height);
                        CanvasGeometry geom;
                        var col = Colors.White;
                        float strokeThickness = 2;

                        switch (box.Props.Type)
                        {
                            case FocusAreaType.MainFace:
                            case FocusAreaType.MfAssistSelection:
                            case FocusAreaType.Box:
                                geom = geomBox;
                                col = Colors.Gold;
                                break;

                            case FocusAreaType.Eye:
                                geom = geomLine;
                                col = Colors.White;
                                strokeThickness = 1;
                                break;

                            case FocusAreaType.FaceOther:
                            case FocusAreaType.TrackUnlock:
                            case FocusAreaType.MfAssistPinP:
                                geom = geomAim;
                                col = Colors.White;
                                break;

                            case FocusAreaType.OneAreaSelected:
                            case FocusAreaType.TrackLock:
                                geom = geomAim;
                                col = Colors.Gold;
                                break;

                            case FocusAreaType.MfAssistLimit:
                                geom = geomBox;
                                col = Colors.White;
                                break;

                            case FocusAreaType.Cross:
                                geom = geomCross;
                                col = Colors.White;
                                break;

                            default:
                                continue;
                        }

                        if (box.Props.Failed)
                        {
                            col = Colors.Red;
                        }

                        session.DrawGeometry(geom.Transform(GetTransform(trans, scale)), col, strokeThickness);
                    }
                }
            }
            else
            {
                session.Clear(Colors.Transparent);
            }
        }

        public void Reset()
        {
            var cur = currentFrame;
            currentFrame = null;
            if (cur != null)
            {
                var task = Task.Run(() =>
                {
                    try
                    {
                        lock (frameDrawLock)
                        {
                            cur.Dispose();
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Error(new Exception("Frame dispose failed", ex));
                    }
                });
            }
        }

        public async Task<IntPoint?> UpdateBitmap(Stream stream)
        {
            if (Interlocked.CompareExchange(ref updateBitmapFlag, 1, 0) == 0)
            {
                try
                {
                    stream.Position = 0;

                    var newframe = new FrameData();

                    ICanvasImage content = newframe.Bitmap = await CanvasBitmap.LoadAsync(view, stream.AsRandomAccessStream());
                    if (LutEffect != null)
                    {
                        content = newframe.LutImage = LutEffect.GenerateEffect(content);
                    }

                    newframe.Content = content;

                    var oldframe = currentFrame;
                    currentFrame = newframe;

                    if (oldframe != null)
                    {
                        var task = Task.Run(() =>
                        {
                            try
                            {
                                lock (frameDrawLock)
                                {
                                    oldframe.Dispose();
                                }
                            }
                            catch (Exception ex)
                            {
                                Log.Error(new Exception("Frame dispose failed", ex));
                            }
                        });
                    }

                    view.Invalidate();
                    return new IntPoint((int)newframe.Bitmap.SizeInPixels.Width, (int)newframe.Bitmap.SizeInPixels.Height);
                }
                finally
                {
                    updateBitmapFlag = 0;
                }
            }

            return null;
        }

        private void CreateAim(CanvasControl view)
        {
            var builder = new CanvasPathBuilder(view);
            builder.BeginFigure(0f, 0.3f);
            builder.AddLine(0f, 0f);
            builder.AddLine(0.3f, 0f);
            builder.EndFigure(CanvasFigureLoop.Open);

            builder.BeginFigure(0.7f, 0f);
            builder.AddLine(1f, 0f);
            builder.AddLine(1f, 0.3f);
            builder.EndFigure(CanvasFigureLoop.Open);

            builder.BeginFigure(1f, 0.7f);
            builder.AddLine(1f, 1f);
            builder.AddLine(0.7f, 1f);
            builder.EndFigure(CanvasFigureLoop.Open);

            builder.BeginFigure(0.3f, 1f);
            builder.AddLine(0f, 1f);
            builder.AddLine(0f, 0.7f);
            builder.EndFigure(CanvasFigureLoop.Open);

            geomAim = CanvasGeometry.CreatePath(builder);
        }

        private void CreateBox(CanvasControl view)
        {
            var builder = new CanvasPathBuilder(view);
            builder.BeginFigure(0f, 0f);
            builder.AddLine(1f, 0f);
            builder.AddLine(1f, 1f);
            builder.AddLine(0f, 1f);
            builder.EndFigure(CanvasFigureLoop.Closed);

            geomBox = CanvasGeometry.CreatePath(builder);
        }

        private void CreateCrest(CanvasControl view)
        {
            var builder = new CanvasPathBuilder(view);
            builder.BeginFigure(0.5f, 0f);
            builder.AddLine(0.5f, 1f);
            builder.EndFigure(CanvasFigureLoop.Open);
            builder.BeginFigure(0f, 0.5f);
            builder.AddLine(1f, 0.5f);
            builder.EndFigure(CanvasFigureLoop.Open);

            geomCross = CanvasGeometry.CreatePath(builder);
        }

        private void CreateLine(CanvasControl view)
        {
            var builder = new CanvasPathBuilder(view);
            builder.BeginFigure(0f, 0f);
            builder.AddLine(1f, 1f);
            builder.EndFigure(CanvasFigureLoop.Open);

            geomLine = CanvasGeometry.CreatePath(builder);
        }

        private Matrix3x2 GetTransform(Vector2 trans, Vector2 scale)
        {
            var newmat = Matrix3x2.CreateScale(scale);
            newmat.Translation = trans;
            var result = Matrix3x2.Multiply(newmat, imageTransform);
            return result;
        }

        private void View_CreateResources(CanvasControl view, CanvasCreateResourcesEventArgs args)
        {
            if (args.Reason == CanvasCreateResourcesReason.FirstTime)
            {
                CreateCrest(view);
                CreateBox(view);
                CreateAim(view);
                CreateLine(view);
            }
        }

        private class FrameData : IDisposable
        {
            public CanvasBitmap Bitmap { get; set; }

            public ICanvasImage Content { private get; set; }

            public ICanvasImage LutImage { private get; set; }

            public void Dispose()
            {
                Bitmap?.Dispose();
                LutImage?.Dispose();
            }

            public void Draw(CanvasDrawingSession session, Rect imageRect)
            {
                session.DrawImage(Content, imageRect, new Rect(new Point(0, 0), Bitmap.Size), 1.0f, CanvasImageInterpolation.NearestNeighbor);
            }
        }
    }
}