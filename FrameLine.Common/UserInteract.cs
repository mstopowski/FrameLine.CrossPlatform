using System;
using Rhino;
using Rhino.Collections;
using Rhino.Input;

namespace FrameLine.Common
{
    public class UserInteract
    {
        int startFrame = 0;
        int endFrame = 0;
        int spacing = 0;
        bool modify = false;
        public bool zmienna = false;

        RhinoList<Spacing> userSpacings = new RhinoList<Spacing>();

        public UserInteract(ref RhinoList<Spacing> spacingsList)
        {
            this.userSpacings = spacingsList;
        }

        public void AskUser()
        {
            RhinoGet.GetInteger("Enter starting frame number", false, ref startFrame);
            RhinoGet.GetInteger("Enter end frame number", false, ref endFrame);
            RhinoGet.GetInteger("Enter typical spacing", false, ref spacing);
            RhinoGet.GetBool("Do you want to insert local modification of spacing?", true, "No", "Yes", ref modify);

            Spacing spaceMain = new Spacing(startFrame, endFrame, spacing);
            userSpacings.Add(spaceMain);

            while (modify)
            {
                RhinoGet.GetInteger("Enter starting frame of modification", false, ref startFrame);
                RhinoGet.GetInteger("Enter end frame of modification", false, ref endFrame);
                RhinoGet.GetInteger("Enter spacing of modification", false, ref spacing);

                Spacing spaceMod = new Spacing(startFrame, endFrame, spacing);
                userSpacings.Add(spaceMod);

                RhinoGet.GetBool("Do you want to add another local modification?", true, "No", "Yes", ref modify);
            }
            this.zmienna = true;
        }
    }
}
