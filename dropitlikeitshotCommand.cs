using System;
using System.Collections.Generic;
using System.Linq;
using Rhino;
using Rhino.Commands;
using Rhino.Geometry;
using Rhino.DocObjects;
using Rhino.Input;
using Rhino.Input.Custom;

namespace dropitlikeitshot
{
    [System.Runtime.InteropServices.Guid("7B59053A-88B0-4CC7-BFCE-A099E7F4EA5C")]

    public class dropitlikeitshotCommand : Command
    {
        public dropitlikeitshotCommand()
        {
            // Rhino only creates one instance of each command class defined in a
            // plug-in, so it is safe to store a refence in a static property.
            Instance = this;
        }

        ///<summary>The only instance of this command.</summary>
        public static dropitlikeitshotCommand Instance
        {
            get; private set;
        }

        ///<returns>The command name as it appears on the Rhino command line.</returns>
        public override string EnglishName
        {
            get { return "DropItLikeItsHot"; }
        }

        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {
 
            Mesh M;

            GetObject getSomething = new GetObject();
            
            getSomething.SetCommandPrompt("Select a closed mesh");
            if (getSomething.Get() != GetResult.Object)
            {
                RhinoApp.WriteLine("No object selected");
                return getSomething.CommandResult();
            }
            
            GetResult result = getSomething.Get();
            var obj = getSomething.Objects();
            M = obj[0].Mesh();         

            var Pts = M.Vertices.ToPoint3dArray();

            var VM = Rhino.Geometry.VolumeMassProperties.Compute(M);
            Plane Pln = new Plane(VM.Centroid, Vector3d.ZAxis);
            Point3d[] PtArray = new Point3d[Pts.Length + 3];
            PtArray[0] = Pln.Origin;
            PtArray[1] = Pln.PointAt(1, 0);
            PtArray[2] = Pln.PointAt(0, 1);

            for (int i = 0; i < Pts.Length; i++)
            {
                PtArray[i + 3] = Pts[i];
            }

            var Ix = new int[PtArray.Length];

            for (int i = 0; i < PtArray.Length; i++)
            {
                Ix[i] = i;
            }

            var Goals = new List<KangarooSolver.IGoal>();
            var G = new KangarooSolver.Goals.RigidBody(PtArray, M, 100);
            G.PIndex = Ix;
            Goals.Add(G);
            Goals.Add(new KangarooSolver.Goals.FloorPlane(100));

            for (int i = 3; i < PtArray.Length; i++)
            {
                Goals.Add(new KangarooSolver.Goals.Unary(i, new Vector3d(0, 0, -0.1))); //gravity
            }

            var PS = new KangarooSolver.PhysicalSystem();

            PS.SetParticleList(PtArray.ToList());

            //var conduit = new dropitlikeitshotDisplayConduit();
            //conduit.Enabled = true;

            //Action redraw = dropitlikeitshotDoOnMainThread.Redraw;
            Action show = dropitlikeitshotDoOnMainThread.Show;
            Action delete = dropitlikeitshotDoOnMainThread.Delete;

            int counter = 0;
            double threshold = 1e-9;
            do
            {
                PS.Step(Goals, true, threshold); // The step will iterate until either reaching 15ms or the energy threshold
                //conduit.Mesh = PS.GetOutput(Goals)[0] as Mesh;

                dropitlikeitshotDoOnMainThread.Mesh = PS.GetOutput(Goals)[0] as Mesh; ;

                RhinoApp.InvokeAndWait(show);

                counter++;

            } while (PS.GetvSum() > threshold && counter < 200); //GetvSum returns the current kinetic energy

            //conduit.Enabled = false;
            RhinoApp.InvokeAndWait(delete);

            Mesh A = PS.GetOutput(Goals)[0] as Mesh;

            doc.Objects.AddMesh(A);
            doc.Views.Redraw();

            return Result.Success;             
        }
    }

    public static class dropitlikeitshotDoOnMainThread
    {
        public static Mesh Mesh { get; set; }
        public static Guid MeshObjId { get; set; }
        public static void Redraw()
        {
            RhinoDoc.ActiveDoc.Views.Redraw();
        }
        public static void Delete()
        {
            RhinoDoc.ActiveDoc.Objects.Delete(MeshObjId, true);
        }

        public static void Show()
        {
            if (MeshObjId != null)
                Delete();
            MeshObjId = RhinoDoc.ActiveDoc.Objects.AddMesh(Mesh);
            Redraw();
        }
    }
}
