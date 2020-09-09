using System;
using System.Threading;
using Rhino;
using Rhino.Commands;
using Rhino.Collections;
using Rhino.Display;
using Rhino.Geometry;
using Rhino.Input;
using Rhino.Input.Custom;
using Rhino.UI;
using Rhino.Runtime;
#if ON_RUNTIME_APPLE
using Eto.Drawing;
using Eto.Forms;
#endif

namespace FrameLine.Common
{
    class EscapeKeyEventHandler : IDisposable
    {
        bool m_escape_key_pressed = false;

        public EscapeKeyEventHandler()
        {
            RhinoApp.EscapeKeyPressed += RhinoApp_EscapeKeyPressed;
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

    public class FrameLineCommonCommand : Rhino.Commands.Command
    {
        public override string EnglishName => "FrameLine";

        protected override Result RunCommand(Rhino.RhinoDoc doc, RunMode mode)
        {
            int frameHeight = 400; // Vertical lines height
            int textHeight = 150; // Text height

            EscapeKeyEventHandler handler = new EscapeKeyEventHandler();

            //List of spacings
            RhinoList<Spacing> spacings = new RhinoList<Spacing>();

            var dupa = new Dupa(ref spacings);

            while(!handler.EscapeKeyPressed && !dupa.zmienna)
            {
                dupa.AskUser();
            }

            if(handler.EscapeKeyPressed)
            {
                return Result.Cancel;
            }

            // Create object of Class FrameLine with spacings as input
            FrameLine frameLine = new FrameLine(spacings);

            // Backup of current layer
            Rhino.DocObjects.Layer layerBackUp = RhinoDoc.ActiveDoc.Layers.CurrentLayer;
            int indexDef = layerBackUp.Index;

            //Creating layer for frameline
            Rhino.DocObjects.Layer flineLayer = new Rhino.DocObjects.Layer
            {
                Name = "Frameline"
            };
            int indexFL = doc.Layers.Add(flineLayer);
            RhinoDoc.ActiveDoc.Layers.SetCurrentLayerIndex(indexFL, true);

            // Drawing frameline & adding labels
            for (int i = 0; i < frameLine.polyPoints.Count; i++)
            {
                doc.Objects.AddLine(new Line(new Point3d(frameLine.polyPoints[i][0], frameLine.polyPoints[i][1] - frameHeight / 2, 0),
                                                new Point3d(frameLine.polyPoints[i][0], frameLine.polyPoints[i][1] + frameHeight / 2, 0)));
                doc.Objects.AddLine(new Line(new Point3d(frameLine.polyPoints[i][0], 0, frameLine.polyPoints[i][1] - frameHeight / 2),
                                                new Point3d(frameLine.polyPoints[i][0], 0, frameLine.polyPoints[i][1] + frameHeight / 2)));
                if (frameLine.ifLabelList[i])
                {
                    Text3d tkst = new Text3d("Fr " + frameLine.framesList[i].ToString());
                    Text3d tkstRotated = new Text3d("Fr " + frameLine.framesList[i].ToString());

                    tkst.Height = textHeight;
                    tkstRotated.Height = textHeight;

                    tkst.HorizontalAlignment = Rhino.DocObjects.TextHorizontalAlignment.Center;
                    tkst.VerticalAlignment = Rhino.DocObjects.TextVerticalAlignment.Middle;

                    tkstRotated.HorizontalAlignment = Rhino.DocObjects.TextHorizontalAlignment.Center;
                    tkstRotated.VerticalAlignment = Rhino.DocObjects.TextVerticalAlignment.Middle;

                    tkst.TextPlane = new Plane(new Point3d(frameLine.polyPoints[i][0], -frameHeight, 0), new Vector3d(0.0, 0.0, 1.0));
                    Plane rotPlane = new Plane(new Point3d(frameLine.polyPoints[i][0], 0, -frameHeight), new Vector3d(0.0, 0.0, 1.0));
                    rotPlane.Rotate(Math.PI / 2, new Vector3d(1.0, 0.0, 0.0));
                    tkstRotated.TextPlane = rotPlane;

                    doc.Objects.AddText(tkst);
                    doc.Objects.AddText(tkstRotated);
                }
            }

            // Adding polyline to document
            doc.Objects.AddPolyline(frameLine.polyPoints);
            RhinoDoc.ActiveDoc.Views.Redraw();

            // Backing up to previous layer and locking frameline layer
            RhinoDoc.ActiveDoc.Layers.SetCurrentLayerIndex(indexDef, true);
            RhinoDoc.ActiveDoc.Layers.FindIndex(indexFL).IsLocked = true;

            return Result.Success;
        }
    }

    public class Dupa
    {  
        int startFrame = 0;
        int endFrame = 0;
        int spacing = 0;
        bool modify = false;
        public bool zmienna = false;

        RhinoList<Spacing> spacingsDupa = new RhinoList<Spacing>();

        public Dupa(ref RhinoList<Spacing> spacingsList)
        {
            this.spacingsDupa = spacingsList;
        }

        public void AskUser()
        {
            RhinoGet.GetInteger("Enter starting frame number", false, ref startFrame);
            RhinoGet.GetInteger("Enter end frame number", false, ref endFrame);
            RhinoGet.GetInteger("Enter typical spacing", false, ref spacing);
            RhinoGet.GetBool("Do you want to insert local modification of spacing?", true, "No", "Yes", ref modify);

            Spacing spaceMain = new Spacing(startFrame, endFrame, spacing);
            spacingsDupa.Add(spaceMain);

            while (modify)
            {
                RhinoGet.GetInteger("Enter starting frame of modification", false, ref startFrame);
                RhinoGet.GetInteger("Enter end frame of modification", false, ref endFrame);
                RhinoGet.GetInteger("Enter spacing of modification", false, ref spacing);

                Spacing spaceMod = new Spacing(startFrame, endFrame, spacing);
                spacingsDupa.Add(spaceMod);

                RhinoGet.GetBool("Do you want to add another local modification?", true, "No", "Yes", ref modify);
            }
            this.zmienna = true;
        }
    }
}