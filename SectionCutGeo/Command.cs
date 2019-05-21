#region Namespaces
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System.Linq;
#endregion

namespace SectionCutGeo
{
  [Transaction( TransactionMode.Manual )]
  public class Command : IExternalCommand
  {
    const string _instructions = "Please launch this "
      + "command in a section view with fine level of "
      + "detail and far bound clipping set to 'Clip with line'";

    public Result Execute(
      ExternalCommandData commandData,
      ref string message,
      ElementSet elements )
    {
      UIApplication uiapp = commandData.Application;
      UIDocument uidoc = uiapp.ActiveUIDocument;
      Application app = uiapp.Application;
      Document doc = uidoc.Document;
      View section_view = commandData.View;
      Parameter p = section_view.get_Parameter(
        BuiltInParameter.VIEWER_BOUND_FAR_CLIPPING );

      if( ViewType.Section != section_view.ViewType
        || ViewDetailLevel.Fine != section_view.DetailLevel
        || 1 != p.AsInteger() )
      {
        message = _instructions;
        return Result.Failed;
      }

      //ICollection<ElementId> ids = uidoc.Selection
      //  .GetElementIds();

      //int n = ids.Count;

      //if( 0 == n )
      //{
      //  message = "Please select some elements before launching this command";
      //  return Result.Failed;
      //}

      FilteredElementCollector a 
        = new FilteredElementCollector( 
          doc, section_view.Id );

      Options opt = new Options() {
        ComputeReferences = false,
        IncludeNonVisibleObjects = false,
        View = section_view
      };

      SketchPlane plane1 = section_view.SketchPlane; // this is null

      Plane plane2 = Plane.CreateByNormalAndOrigin( 
        section_view.ViewDirection, 
        section_view.Origin );

      SketchPlane plane3 = SketchPlane.Create( doc, plane2 );

      int geo_count = 0;
      int null_geo_count = 0;
      int curve_count = 0;
      int solid_count = 0;

      using( Transaction tx = new Transaction( doc ) )
      {
        tx.Start( "Create Section Cut Model Curves" );

        foreach( Element e in a )
        {
          GeometryElement geo = e.get_Geometry( opt );

          if( null == geo )
          {
            ++null_geo_count;
          }
          else
          {
            ++geo_count;

            IEnumerable<GeometryObject> curves 
              = geo.Where<GeometryObject>( 
                o => o is Curve );

            foreach( Curve curve in curves )
            {
              ++curve_count;
              doc.Create.NewModelCurve( curve, plane3 );
            }

            foreach( GeometryObject obj in geo )
            {
              if( obj is Solid )
              {
                ++solid_count;

                Solid sol = obj as Solid;

                EdgeArray edges = sol.Edges;

                foreach( Edge edge in edges )
                {
                  doc.Create.NewModelCurve( edge.AsCurve(), plane3 );
                }
              }
            }
          }
        }
        tx.Commit();
      }
      return Result.Succeeded;
    }
  }
}
