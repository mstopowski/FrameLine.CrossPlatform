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
using Rhino.DocObjects;
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
            EscapeKeyEventHandler handler = new EscapeKeyEventHandler();

            RhinoList<Spacing> spacings = new RhinoList<Spacing>(); //List of spacings

            UserInteract user = new UserInteract(ref spacings);

            user.AskUser();

            // Create object of Class FrameLine with spacings as input
            FrameLine frameLine = new FrameLine(spacings);

            // Backup of current layer
            var layerBackUp = doc.Layers.CurrentLayer;

            //Creating layer for frameline
            Layer flineLayer = new Layer();
            flineLayer.Name = "FRAME LINE";
            doc.Layers.Add(flineLayer);
            doc.Layers.SetCurrentLayerIndex(doc.Layers.FindName(flineLayer.Name).Index, true);

            // Creating layer for labels (child of frameline)
            Layer labelLayer = new Layer();
            labelLayer.Name = "LABELS";
            labelLayer.ParentLayerId = doc.Layers.FindIndex(doc.Layers.FindName(flineLayer.Name).Index).Id;
            doc.Layers.Add(labelLayer);

            // Grupowanie
            var groupName = "FRAME LINE GROUP";
            if (doc.Groups.Count == 0 || doc.Groups.FindName(groupName) == null)
            {
                doc.Groups.Add(groupName);
            }
            if (doc.Groups.FindName(groupName) != null)
            {
                var oldFrameLine = doc.Objects.FindByGroup(doc.Groups.FindName(groupName).Index);
                foreach (var element in oldFrameLine)
                {
                    doc.Objects.Delete(element);
                }
            }

            int frameHeight = 400; // Vertical lines height

            CreateCrossLines(doc, frameLine, groupName, frameHeight);

            doc.Layers.SetCurrentLayerIndex(doc.Layers.FindName(labelLayer.Name).Index, true);

            AddLabels(doc, frameLine, groupName, frameHeight);

            // Adding polyline to document
            doc.Layers.SetCurrentLayerIndex(doc.Layers.FindName(flineLayer.Name).Index, true);
            var polyID = doc.Objects.AddPolyline(frameLine.polyPoints);
            doc.Groups.AddToGroup(doc.Groups.FindName(groupName).Index, polyID);

            // Redrawing views
            doc.Views.Redraw();

            // Restoring previous layer and locking frameline layer
            doc.Layers.SetCurrentLayerIndex(layerBackUp.Index, true);
            doc.Layers.FindIndex(doc.Layers.FindName(labelLayer.Name).Index).IsLocked = true;
            doc.Layers.FindIndex(doc.Layers.FindName(flineLayer.Name).Index).IsLocked = true;

            return Result.Success;
        }

        void CreateCrossLines(Rhino.RhinoDoc doc, FrameLine frameLine, string groupName, int frameHeight)
        {
            // Drawing frameline lines
            for (int i = 0; i < frameLine.polyPoints.Count; i++)
            {
                var line1ID = doc.Objects.AddLine(new Line(new Point3d(frameLine.polyPoints[i][0], frameLine.polyPoints[i][1] - frameHeight / 2, 0),
                                                new Point3d(frameLine.polyPoints[i][0], frameLine.polyPoints[i][1] + frameHeight / 2, 0)));
                var line2ID = doc.Objects.AddLine(new Line(new Point3d(frameLine.polyPoints[i][0], 0, frameLine.polyPoints[i][1] - frameHeight / 2),
                                                new Point3d(frameLine.polyPoints[i][0], 0, frameLine.polyPoints[i][1] + frameHeight / 2)));
                doc.Groups.AddToGroup(doc.Groups.FindName(groupName).Index, line1ID);
                doc.Groups.AddToGroup(doc.Groups.FindName(groupName).Index, line2ID);
            }
        }

        void AddLabels(Rhino.RhinoDoc doc, FrameLine frameLine, string groupName,int frameHeight)
        {
            int textHeight = 150; // Text height

            // Adding labels
            for (int i = 0; i < frameLine.polyPoints.Count; i++)
            {
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

                    var tkstID = doc.Objects.AddText(tkst);
                    var tkstRotatedID = doc.Objects.AddText(tkstRotated);
                    doc.Groups.AddToGroup(doc.Groups.FindName(groupName).Index, tkstID);
                    doc.Groups.AddToGroup(doc.Groups.FindName(groupName).Index, tkstRotatedID);
                }
            }
        }
    }
}