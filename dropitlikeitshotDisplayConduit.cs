using Rhino.Display;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dropitlikeitshot
{
    public class dropitlikeitshotDisplayConduit: DisplayConduit
    {
        public Rhino.Geometry.Mesh Mesh { get; set; }

        protected override void CalculateBoundingBox(CalculateBoundingBoxEventArgs e)
        {
            if (null != Mesh)
            {
                Rhino.Geometry.BoundingBox bbox = Mesh.GetBoundingBox(false);
                // Unites a bounding box with the current display bounding box in
                // order to ensure dynamic objects in "box" are drawn.
                e.IncludeBoundingBox(bbox);
            }
        }

        protected override void PostDrawObjects(DrawEventArgs e)
        {
            Rhino.Display.DisplayMaterial material = new Rhino.Display.DisplayMaterial
            {
                Diffuse = System.Drawing.Color.Black,
                Transparency = 0.5
            };
            e.Display.DrawMeshShaded(Mesh, material);
            e.Display.DrawMeshWires(Mesh, System.Drawing.Color.Gray);
        }
    }
}
