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
            int tstep = 1;
            if(e.Control) {
                tstep = Controller.Format.TileColumns;
                if(e.Shift) {
                    tstep *= Controller.Format.TileRows;
                }
            }
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
                        Controller.Execute(ImageSeekCommand.ChangeOffsetFrame, -tstep);
                        break;
                    case Keys.PageDown:
                        Controller.Execute(ImageSeekCommand.ChangeOffsetFrame, tstep);
                        break;
                    case Keys.Add:
                    case Keys.Subtract:
                        break;
                    case Keys.OemOpenBrackets:
                        Controller.Execute(ImageSeekCommand.ChangeTileColumns, -step);
                        break;
                    case Keys.OemCloseBrackets:
                        Controller.Execute(ImageSeekCommand.ChangeTileColumns, step);
                        break;
                    case Keys.OemSemicolon:
                        Controller.Execute(ImageSeekCommand.ChangeTileRows, -step);
                        break;
                    case Keys.OemPeriod:
                        Controller.Execute(ImageSeekCommand.ChangeTileRows, step);
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
                        Controller.Execute(ImageSeekCommand.ChangeOffsetFrame, -tstep);
                        break;
                    case Keys.PageDown:
                        Controller.Execute(ImageSeekCommand.ChangeOffsetFrame, tstep);
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
        public readonly string HelpText = @"
Keyboard controls (focus the image):

Pan horizontally: Left, Right.
Pan vertically: Up, Down.
Pan by frame: PageUp, PageDn.
Pan by tile line: Ctrl+ PageUp, PageDn.
Pan by tile page: Shift+Ctrl+ PageUp, PageDn.

Change width: Shift+ Left, Right.
Change height: Shift+ Up, Down.
Change tile pack columns: Shift+ Brackets.
Change tile pack rows: Shift+ Semicolon, Period.

Hold Shift to step by 8 units.

Zoom in: Z.
Zoom out: Shift+ Z.
";

    }

}
