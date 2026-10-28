namespace Doomer
{
    // While on, drops the mouse and keyboard input meant for a form's controls, so nothing in it can be used
    // (the title bar still works, to move, minimize or close it). Shows the wait cursor meanwhile.
    internal sealed class InputBlocker(Form form) : IMessageFilter
    {
        private const int WM_KEYFIRST = 0x0100;
        private const int WM_KEYLAST = 0x0109;
        private const int WM_MOUSEFIRST = 0x0200;
        private const int WM_MOUSELAST = 0x020E;

        private bool _on;

        public void Block()
        {
            if (_on)
                return;

            _on = true;
            Application.AddMessageFilter(this);
            form.UseWaitCursor = true;
        }

        public void Unblock()
        {
            if (!_on)
                return;

            _on = false;
            Application.RemoveMessageFilter(this);

            if (!form.IsDisposed)
                form.UseWaitCursor = false;
        }

        public bool PreFilterMessage(ref Message m) =>
            m.Msg is >= WM_KEYFIRST and <= WM_KEYLAST or >= WM_MOUSEFIRST and <= WM_MOUSELAST &&
            Control.FromChildHandle(m.HWnd)?.FindForm() == form;
    }
}
