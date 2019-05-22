#region Namespaces
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
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
      string key = null == obj
        ? "null"
        : obj.GetType().Name;

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
        Debug.Print( "{0,5} {1}", this[key], key );
      }
    }
  }

  [Transaction( TransactionMode.Manual )]
  public class Command : IExternalCommand
  {
    /// <summary>
    ///  Maximum distance for line to be considered 
    ///  to lie in plane
    /// </summary>
    const double _eps = 1.0e-6;

    /// <summary>
    /// User instructions for running this external command
    /// </summary>
    const string _instructions = "Please launch this "
      + "command in a section view with fine level of "
      + "detail and far bound clipping set to 'Clip with line'";

    /// <summary>
    /// Predicate returning true if the given line 
    /// lies in the given plane
    /// </summary>
    static bool IsLineInPlane( 
      Line line, 
      Plane plane )
    {
      XYZ p0 = line.GetEndPoint( 0 );
      XYZ p1 = line.GetEndPoint( 1 );
      UV uv0, uv1;
      double d0, d1;

      plane.Project( p0, out uv0, out d0 );
      plane.Project( p1, out uv1, out d1 );

      return _eps > Math.Abs( d0 ) 
        && _eps > Math.Abs( d1 );
    }

    /// <summary>
    /// Recursively handle geometry element
    /// </summary>
    static void GetCurvesInPlane( 
      SketchPlane plane3,
      JtObjCounter geoCounter,
      GeometryElement geo )
    {
      geoCounter.Increment( geo );

      if( null != geo )
      {
        foreach( GeometryObject obj in geo )
        {
          geoCounter.Increment( obj );

          Solid sol = obj as Solid;

          if( null != sol )
          {
            Document doc = plane3.Document;

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

              Curve curve = edge.AsCurve();

              Debug.Assert( curve is Line, 
                "we currently only support lines here" );

              geoCounter.Increment( curve );

              if( IsLineInPlane( curve as Line, plane3.GetPlane() ) )
              {
                doc.Create.NewModelCurve( edge.AsCurve(), plane3 );
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
