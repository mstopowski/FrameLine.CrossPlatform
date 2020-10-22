using Rhino.Collections;
using Rhino.Input;
using Rhino.Commands;

namespace FrameLine.Common
{
    public class UserInteract
    {
        int startFrame = 0;
        int endFrame = 0;
        int spacing = 0;
        bool modify = false;

        readonly bool validStart = false;
        readonly bool validEnd = false;
        readonly bool validSpac = false;

        readonly bool validStartMod = false;
        readonly bool validEndMod = false;
        readonly bool validSpacMod = false;

        readonly bool userPressedESC = false;
        readonly bool userFieldForm = true;

        readonly RhinoList<Spacing> userSpacings = new RhinoList<Spacing>();

        public UserInteract(ref RhinoList<Spacing> spacingsList)
        {
            userSpacings = spacingsList;
        }

        public bool AskUser()
        {
            while (!validStart)
            {
                if (RhinoGet.GetInteger("Enter starting frame", false, ref startFrame) == Result.Cancel) return userPressedESC;
                if (startFrame > 0)
                {
                    Rhino.UI.Dialogs.ShowMessage("Starting frame should be less or equal to zero.", "Warning");
                }
                else break;
            }

            while (!validEnd)
            {
                if (RhinoGet.GetInteger("Enter ending frame", false, ref endFrame) == Result.Cancel) return userPressedESC;
                if (endFrame < startFrame)
                {
                    Rhino.UI.Dialogs.ShowMessage("End frame cannot be less than start frame.", "Warning");
                }
                else break;
            }

            while (!validSpac)
            {
                if (RhinoGet.GetInteger("Enter typical spacing in milimeters [mm]", false, ref spacing) == Result.Cancel) return userPressedESC;
                if (spacing < 0)
                {
                    Rhino.UI.Dialogs.ShowMessage("Spacing cannot be less than zero.", "Warning");
                }
                else break;
            }

            Spacing spaceMain = new Spacing(startFrame, endFrame, spacing);
            userSpacings.Add(spaceMain);

            if (RhinoGet.GetBool("Do you want to insert local modification?", true, "No", "Yes", ref modify) == Result.Cancel) return userPressedESC;

            while (modify)
            {
                modify = false;
                while (!validStartMod)
                {
                    if (RhinoGet.GetInteger("Enter starting frame of modification", false, ref startFrame) == Result.Cancel) return userPressedESC;
                    if (startFrame < spaceMain.Start || startFrame > spaceMain.End)
                    {
                        Rhino.UI.Dialogs.ShowMessage("Start of modification cannot be outside frame line limits.", "Warning");
                    }
                    else break;
                }

                while (!validEndMod)
                {
                    if (RhinoGet.GetInteger("Enter end frame of modification", false, ref endFrame) == Result.Cancel) return userPressedESC;
                    if (endFrame < spaceMain.Start || endFrame > spaceMain.End)
                    {
                        Rhino.UI.Dialogs.ShowMessage("End of modification cannot be outside frame line limits.", "Warning");
                    }
                    else break;
                }

                while (!validSpacMod)
                {
                    if (RhinoGet.GetInteger("Enter spacing for modification", false, ref spacing) == Result.Cancel) return userPressedESC;
                    if (spacing < 0)
                    {
                        Rhino.UI.Dialogs.ShowMessage("Spacing cannot be less than zero.", "Warning");
                    }
                    else break;
                }

                Spacing spaceMod = new Spacing(startFrame, endFrame, spacing);
                userSpacings.Add(spaceMod);

                if (RhinoGet.GetBool("Do you want to insert another local modification?", true, "No", "Yes", ref modify) == Result.Cancel) return userPressedESC;
            }
            return userFieldForm;
        }
    }
}
