using System;
using Rhino;
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
        bool modify = true;

        readonly bool userPressedESC = false;
        readonly bool userFieldForm = true;

        readonly RhinoList<Spacing> userSpacings = new RhinoList<Spacing>();

        public UserInteract(ref RhinoList<Spacing> spacingsList)
        {
            userSpacings = spacingsList;
        }

        public bool AskUser()
        {
            while (modify)
            {
                modify = false;

                if (RhinoGet.GetInteger("Enter starting frame", false, ref startFrame) == Result.Cancel) return userPressedESC;
                if (RhinoGet.GetInteger("Enter end frame number", false, ref endFrame) == Result.Cancel) return userPressedESC;
                if (RhinoGet.GetInteger("Enter spacing", false, ref spacing) == Result.Cancel) return userPressedESC;

                Spacing spaceMain = new Spacing(startFrame, endFrame, spacing);
                userSpacings.Add(spaceMain);

                if (RhinoGet.GetBool("Do you want to insert local modification?", true, "No", "Yes", ref modify) == Result.Cancel) return userPressedESC;
            }
            return userFieldForm;
        }
    }
}
