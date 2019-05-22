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
  /// <summary>
  /// A class to count and report the number of objects 
  /// encountered.
  /// </summary>
  class JtObjCounter : Dictionary<string, int>
  {
    /// <summary>
    /// Count a new occurence of an object
    /// </summary>
    public void Increment( object obj )
    {
      string key = obj.GetType().Name;

      if( !ContainsKey(key))
      {
        Add( key, 0 );
      }
      ++this[key];
    }

    /// <summary>
    /// Report the number of objects encountered.
    /// </summary>
    public void Print()
    {
      List<string> keys = new List<string>( Keys );
      keys.Sort();
      foreach(string key in keys)
      {
        Debug.Print( "{0,5} {1}", key, this[key] );
      }
    }
  }

  [Transaction( TransactionMode.Manual )]
  public class Command : IExternalCommand
  {
    const string _instructions = "Please launch this "
      + "command in a section view with fine level of "
      + "detail and far bound clipping set to 'Clip with line'";

    /// <summary>
    /// Recursively handle geometry element
    /// </summary>
    static void GetCurvesInPlane( 
      SketchPlane plane3,
      JtObjCounter geoCounter,
      GeometryElement geo )
    {
      Document doc = plane3.Document;

      if( null == geo )
      {
        geoCounter.Increment( "GeometryElement == null" );
      }
      else
      {
        geoCounter.Increment( "GeometryElement non-null" );

        //IEnumerable<GeometryObject> curves 
        //  = geo.Where<GeometryObject>( 
        //    o => o is Curve );

        //foreach( Curve curve in curves )
        //{
        //  ++curve_count;
        //  doc.Create.NewModelCurve( curve, plane3 );
        //}

        foreach( GeometryObject obj in geo )
        {
          geoCounter.Increment( obj );

          Solid sol = obj as Solid;

          if( null != sol )
          {
            EdgeArray edges = sol.Edges;

            foreach( Edge edge in edges )
            {
              // Here we simply try to create a model 
              // curve in the given plane. This throws
              // an exception if the curve does not lie
              // in the plane. That is bad. Better would 
              // be to check whether the curve is in the
              // plane programmatically instead of throwing
              // an exception. How can we determine whether 
              // a curve lies in a plane?

              try
              {
                doc.Create.NewModelCurve( edge.AsCurve(), plane3 );
              }
              catch( Autodesk.Revit.Exceptions.ArgumentException )
              {
                // Thrown if curve does not lie in the plane
              }
            }
          }
          else
          {
            GeometryInstance inst = obj as GeometryInstance;

            if( null != inst )
            {
              GetCurvesInPlane( plane3, geoCounter, 
                inst.GetInstanceGeometry() );
            }
          }
        }
      }
    }

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

      //int geo_count = 0;
      //int null_geo_count = 0;
      //int curve_count = 0;
      //int solid_count = 0;

      using( Transaction tx = new Transaction( doc ) )
      {
        tx.Start( "Create Section Cut Model Curves" );

        JtObjCounter geoCounter = new JtObjCounter();

        SketchPlane plane3 = SketchPlane.Create( doc, plane2 );

        foreach( Element e in a )
        {
          geoCounter.Increment( e );

          GeometryElement geo = e.get_Geometry( opt );

          GetCurvesInPlane( plane3, geoCounter, geo );
        }
        geoCounter.Print();

        tx.Commit();
      }
      return Result.Succeeded;
    }
  }
}
