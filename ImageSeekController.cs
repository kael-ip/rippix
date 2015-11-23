using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rippix {

    enum ImageSeekCommand {
        ChangeWidth,
        ChangeHeight,
        ChangeOffsetByte,
        ChangeOffsetLine,
        ChangeOffsetFrame,
        ChangeZoom
    }

    class ImageSeekController {
        private IPictureAdapter format;
        public IPictureAdapter Format {
            get { return format; }
            set {
                if (Equals(format, value)) return;
                format = value;
            }
        }
        public void Execute(ImageSeekCommand cmd, int value) {
            if (Format == null || Format.MaxOffset == 0) return;
            int step = value;//e.Control ? 8 : 1;
            CorrectOffset(0);
            int oldOffset = Format.PicOffset;
            int tWidth = Format.Width;
            int tHeight = Format.Height;
            switch (cmd) {
                case ImageSeekCommand.ChangeHeight:
                    tHeight += step;
                    break;
                case ImageSeekCommand.ChangeWidth:
                    tWidth += step;
                    break;
                case ImageSeekCommand.ChangeOffsetByte:
                    Format.PicOffset += step;
                    break;
                case ImageSeekCommand.ChangeOffsetLine:
                    Format.PicOffset += Format.LineStride * step;
                    break;
                case ImageSeekCommand.ChangeOffsetFrame:
                    Format.PicOffset += Format.FrameStride * step;
                    break;
                case ImageSeekCommand.ChangeZoom:
                    Format.Zoom += value;
                    break;
                default:
                    throw new InvalidOperationException();
            }
            if (tWidth > 0) Format.Width = tWidth;
            if (tHeight > 0) Format.Height = tHeight;
            CorrectOffset(oldOffset);
        }
        void CorrectOffset(int oldOffset) {
            if (Format.PicOffset < 0 || Format.PicOffset >= Format.MaxOffset) {
                Format.PicOffset = oldOffset;
            }
        }
    }
}
