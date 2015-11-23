using System.Windows.Forms;

namespace Rippix {

    class KeyboardSeekControlHelper {
        private Control control;
        public KeyboardSeekControlHelper(Control control) {
            this.control = control;
            control.PreviewKeyDown += control_PreviewKeyDown;
            control.KeyDown += control_KeyDown;
        }
        private ImageSeekController controller;
        public ImageSeekController Controller {
            get { return controller; }
            set {
                if (Equals(controller, value)) return;
                controller = value;
            }
        }
        void control_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e) {
            switch (e.KeyCode) {
                case Keys.Up:
                case Keys.Down:
                case Keys.Left:
                case Keys.Right:
                case Keys.PageUp:
                case Keys.PageDown:
                case Keys.Add:
                case Keys.Subtract:
                case Keys.OemOpenBrackets:
                case Keys.OemCloseBrackets:
                    e.IsInputKey = true;
                    break;
            }
        }
        void control_KeyDown(object sender, KeyEventArgs e) {
            if (Controller == null) return;
            int step = e.Control ? 8 : 1;
            if (e.Shift) {
                switch (e.KeyCode) {
                    case Keys.Up:
                        Controller.Execute(ImageSeekCommand.ChangeHeight, -step);
                        break;
                    case Keys.Down:
                        Controller.Execute(ImageSeekCommand.ChangeHeight, step);
                        break;
                    case Keys.Left:
                        Controller.Execute(ImageSeekCommand.ChangeWidth, -step);
                        break;
                    case Keys.Right:
                        Controller.Execute(ImageSeekCommand.ChangeWidth, +step);
                        break;
                    case Keys.PageUp:
                        break;
                    case Keys.PageDown:
                        break;
                    case Keys.Add:
                    case Keys.Subtract:
                    case Keys.OemOpenBrackets:
                    case Keys.OemCloseBrackets:
                        break;
                    case Keys.Z:
                        Controller.Execute(ImageSeekCommand.ChangeZoom, -1);
                        break;
                }
            } else {
                switch (e.KeyCode) {
                    case Keys.Up:
                        Controller.Execute(ImageSeekCommand.ChangeOffsetLine, -step);
                        break;
                    case Keys.Down:
                        Controller.Execute(ImageSeekCommand.ChangeOffsetLine, step);
                        break;
                    case Keys.Left:
                        Controller.Execute(ImageSeekCommand.ChangeOffsetByte, -step);
                        break;
                    case Keys.Right:
                        Controller.Execute(ImageSeekCommand.ChangeOffsetByte, step);
                        break;
                    case Keys.PageUp:
                        Controller.Execute(ImageSeekCommand.ChangeOffsetFrame, -step);
                        break;
                    case Keys.PageDown:
                        Controller.Execute(ImageSeekCommand.ChangeOffsetFrame, step);
                        break;
                    case Keys.Add:
                    case Keys.Subtract:
                    case Keys.OemOpenBrackets:
                    case Keys.OemCloseBrackets:
                        break;
                    case Keys.Z:
                        Controller.Execute(ImageSeekCommand.ChangeZoom, 1);
                        break;
                }
            }
            control.Refresh();
        }
    }

}
