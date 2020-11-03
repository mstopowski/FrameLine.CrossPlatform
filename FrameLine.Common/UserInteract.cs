using Rhino.Collections;
using Rhino.Input;
using Rhino.Commands;
using Rhino.Input.Custom;

namespace FrameLine.Common
{
    public class UserInteract
    {
        int StartFrame = 0;
        int EndFrame = 0;
        int Spacing = 0;
        int StretchStart = 0;
        int StretchInterval = 0;
        bool Modify = true;
        bool Stretch = true;
        readonly bool Start = true;

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
            var prompt = "This command will add frame line to the document - press ENTER to continue";
            var go = new GetOption();
            go.SetCommandPrompt(prompt);
            go.AcceptNothing(true);

            var modOption = new OptionToggle(false, "Off", "On");
            var extOption = new OptionToggle(false, "Off", "On");
            go.AddOptionToggle("Modification", ref modOption);
            go.AddOptionToggle("Extension", ref extOption);

            while (Start)
            {
                var res = go.Get();

                if (res == GetResult.Nothing) // user pressed enter
                    break;

                if (res == GetResult.Cancel) // user pressed ESC
                    return false;

                var option = go.Option();
                if (null == option)
                    return false;
            }

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
                if (RhinoGet.GetInteger("Enter typical frame spacing in milimeters [mm]", false, ref Spacing) == Result.Cancel) return UserPressedESC;
                if (Spacing < 0)
                {
                    Rhino.UI.Dialogs.ShowMessage("Spacing cannot be less than zero.", "Warning");
                }
                else break;
            }

            Spacing spaceMain = new Spacing(StartFrame, EndFrame, Spacing);
            UserSpacings.Add(spaceMain);

            while (modOption.CurrentValue && Modify)
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
                    if (RhinoGet.GetInteger("Enter spacing for modification in milimeters [mm]", false, ref Spacing) == Result.Cancel) return UserPressedESC;
                    if (Spacing < 0)
                    {
                        Rhino.UI.Dialogs.ShowMessage("Spacing cannot be less than zero.", "Warning");
                    }
                    else break;
                }

                Spacing spaceMod = new Spacing(StartFrame, EndFrame, Spacing);
                UserSpacings.Add(spaceMod);

                if (RhinoGet.GetBool("Do you want to add another local modification?", true, "No", "Yes", ref Modify) == Result.Cancel) return UserPressedESC;
            }

            while (extOption.CurrentValue && Stretch)
            {
                Stretch = false;
                if (RhinoGet.GetInteger("Enter frame after which you want to insert stretch", false, ref StretchStart) == Result.Cancel) return UserPressedESC;
                if (RhinoGet.GetInteger("Amount of frames of stretch", false, ref StretchInterval) == Result.Cancel) return UserPressedESC;
                Stretch stretch = new Stretch(StretchStart, StretchInterval);
                UserStretch.Add(stretch);
            }

            return UserFieldForm;
        }
    }
}
