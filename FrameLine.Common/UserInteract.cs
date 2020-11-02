using Rhino.Collections;
using Rhino.Input;
using Rhino.Commands;

namespace FrameLine.Common
{
    public class UserInteract
    {
        int StartFrame = 0;
        int EndFrame = 0;
        int Spacing = 0;
        int StretchStart = 0;
        int StretchInterval = 0;
        bool Modify = false;
        bool Stretch = false;

        readonly bool ValidStart = false;
        readonly bool ValidEnd = false;
        readonly bool ValidSpac = false;

        readonly bool ValidStartMod = false;
        readonly bool ValidEndMod = false;
        readonly bool ValidSpacMod = false;

        readonly bool UserPressedESC = false;
        readonly bool UserFieldForm = true;

        readonly RhinoList<Spacing> UserSpacings = new RhinoList<Spacing>();
        readonly RhinoList<Stretch> UserStretch = new RhinoList<Stretch>();

        public UserInteract(ref RhinoList<Spacing> SpacingsList, ref RhinoList<Stretch> stretches)
        {
            UserSpacings = SpacingsList;
            UserStretch = stretches;
        }

        public bool AskUser()
        {
            while (!ValidStart)
            {
                if (RhinoGet.GetInteger("Enter starting frame", false, ref StartFrame) == Result.Cancel) return UserPressedESC;
                if (StartFrame > 0)
                {
                    Rhino.UI.Dialogs.ShowMessage("Starting frame should be less or equal to zero.", "Warning");
                }
                else break;
            }

            while (!ValidEnd)
            {
                if (RhinoGet.GetInteger("Enter ending frame", false, ref EndFrame) == Result.Cancel) return UserPressedESC;
                if (EndFrame < StartFrame)
                {
                    Rhino.UI.Dialogs.ShowMessage("End frame cannot be less than start frame.", "Warning");
                }
                else break;
            }

            while (!ValidSpac)
            {
                if (RhinoGet.GetInteger("Enter typical Spacing in milimeters [mm]", false, ref Spacing) == Result.Cancel) return UserPressedESC;
                if (Spacing < 0)
                {
                    Rhino.UI.Dialogs.ShowMessage("Spacing cannot be less than zero.", "Warning");
                }
                else break;
            }

            Spacing spaceMain = new Spacing(StartFrame, EndFrame, Spacing);
            UserSpacings.Add(spaceMain);

            if (RhinoGet.GetBool("Do you want to insert local modification?", true, "No", "Yes", ref Modify) == Result.Cancel) return UserPressedESC;

            while (Modify)
            {
                Modify = false;
                while (!ValidStartMod)
                {
                    if (RhinoGet.GetInteger("Enter starting frame of modification", false, ref StartFrame) == Result.Cancel) return UserPressedESC;
                    if (StartFrame < spaceMain.Start || StartFrame > spaceMain.End)
                    {
                        Rhino.UI.Dialogs.ShowMessage("Start of modification cannot be outside frame line limits.", "Warning");
                    }
                    else break;
                }

                while (!ValidEndMod)
                {
                    if (RhinoGet.GetInteger("Enter end frame of modification", false, ref EndFrame) == Result.Cancel) return UserPressedESC;
                    if (EndFrame < spaceMain.Start || EndFrame > spaceMain.End)
                    {
                        Rhino.UI.Dialogs.ShowMessage("End of modification cannot be outside frame line limits.", "Warning");
                    }
                    else break;
                }

                while (!ValidSpacMod)
                {
                    if (RhinoGet.GetInteger("Enter Spacing for modification", false, ref Spacing) == Result.Cancel) return UserPressedESC;
                    if (Spacing < 0)
                    {
                        Rhino.UI.Dialogs.ShowMessage("Spacing cannot be less than zero.", "Warning");
                    }
                    else break;
                }

                Spacing spaceMod = new Spacing(StartFrame, EndFrame, Spacing);
                UserSpacings.Add(spaceMod);

                if (RhinoGet.GetBool("Do you want to insert another local modification?", true, "No", "Yes", ref Modify) == Result.Cancel) return UserPressedESC;
            }

            if (RhinoGet.GetBool("Do you want to locally stretch frame line?", true, "No", "Yes", ref Stretch) == Result.Cancel) return UserPressedESC;

            while (Stretch)
            {
                Stretch = false;
                if (RhinoGet.GetInteger("Enter frame to stretch", false, ref StretchStart) == Result.Cancel) return UserPressedESC;
                if (RhinoGet.GetInteger("Amount of frames to stretch", false, ref StretchInterval) == Result.Cancel) return UserPressedESC;
                Stretch stretch = new Stretch(StretchStart, StretchInterval);
                UserStretch.Add(stretch);
            }
            return UserFieldForm;
        }
    }
}
