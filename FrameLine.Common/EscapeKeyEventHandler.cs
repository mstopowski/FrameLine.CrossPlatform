using System;
using Rhino;

namespace FrameLine.Common
{
    public class EscapeKeyEventHandler : IDisposable
    {
        private bool m_escape_key_pressed = false;

        public EscapeKeyEventHandler(string message)
        {
            RhinoApp.EscapeKeyPressed += RhinoApp_EscapeKeyPressed;
            RhinoApp.WriteLine(message);
        }

        public bool EscapeKeyPressed
        {
            get
            {
                RhinoApp.Wait(); // "pumps" the Rhino message queue
                return m_escape_key_pressed;
            }
        }

        private void RhinoApp_EscapeKeyPressed(object sender, EventArgs e)
        {
            m_escape_key_pressed = true;
            RhinoApp.WriteLine("Escape");
        }

        public void Dispose()
        {
            RhinoApp.EscapeKeyPressed -= RhinoApp_EscapeKeyPressed;
        }
    }
}
