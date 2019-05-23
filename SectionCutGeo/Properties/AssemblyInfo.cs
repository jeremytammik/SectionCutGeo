using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// General Information about an assembly is controlled through the following
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle( "SectionCutGeo" )]
[assembly: AssemblyDescription( "Revit Add-In Description for SectionCutGeo" )]
[assembly: AssemblyConfiguration( "" )]
[assembly: AssemblyCompany( "Autodesk Inc." )]
[assembly: AssemblyProduct( "SectionCutGeo Revit C# .NET Add-In" )]
[assembly: AssemblyCopyright( "Copyright 2019 (C) Jeremy Tammik, Autodesk Inc." )]
[assembly: AssemblyTrademark( "" )]
[assembly: AssemblyCulture( "" )]

// Setting ComVisible to false makes the types in this assembly not visible
// to COM components.  If you need to access a type in this assembly from
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible( false )]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid( "321044f7-b0b2-4b1c-af18-e71a19252be0" )]

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Build and Revision Numbers
// by using the '*' as shown below:
// [assembly: AssemblyVersion("1.0.*")]
//
// History:
//
// 2019-05-22 2020.0.0.0 first working release, using NewModelCurve and an exception if curve is not in cut plane
// 2019-05-22 2020.0.0.1 added comment and cleanup
// 2019-05-22 2020.0.0.2 cleaned up object counter formatting
// 2019-05-22 2020.0.0.3 added counter for curve types; discovered that we only need to process `Line` objects
// 2019-05-22 2020.0.0.3 implemented IsLineInPlane predicate
// 2019-05-22 2020.0.0.3 implemented read-only curve collection and separate later model curve creation
// 2019-05-23 2020.0.0.4 non-negative distance returned by Plane.Project
//
[assembly: AssemblyVersion( "2020.0.0.4" )]
[assembly: AssemblyFileVersion( "2020.0.0.4" )]
