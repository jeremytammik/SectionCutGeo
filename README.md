# SectionCutGeo

Revit C# .NET add-in to retrieve section view cut geometry.

The SectionCutGeo add-in retrieves the geometry resulting from cutting a family instance in a section view.

It was prompted by
the [Revit API discussion forum](http://forums.autodesk.com/t5/revit-api-forum/bd-p/160) thread
on [how to receive intersection of section and `FamilyInstance`](https://forums.autodesk.com/t5/revit-api-forum/how-to-receive-intersection-of-section-and-familyinstance/m-p/8802202).

It is explained in detail 
in [The Building Coder](https://thebuildingcoder.typepad.com) discussion 
on [retrieving section view intersection cut geometry](https://thebuildingcoder.typepad.com/blog/2019/05/retrieving-section-view-intersection-cut-geometry.html).

Sample model 3D view:

![Sample model 3D view](img/section_cut_geo_3d.png)

Plan view showing section location:

![Plan view showing section location](img/section_cut_geo_plan.png)

Cut geometry in section view:

![Cut geometry in section view](img/section_cut_geo_cut.png)

Model lines representing the cut geometry of the window family instance produced by the add-in in section view:

![Model lines representing cut geometry in section view](img/section_cut_geo_cut_geo_window.png)

Model lines representing the cut geometry of walls, door and window isolated in 3D view:

![Model lines representing cut geometry isolated in 3D view](img/section_cut_geo_cut_geo_3d.png)


## Author

Jeremy Tammik, [The Building Coder](http://thebuildingcoder.typepad.com), [ADN](http://www.autodesk.com/adn) [Open](http://www.autodesk.com/adnopen), [Autodesk Inc.](http://www.autodesk.com)


## License

This sample is licensed under the terms of the [MIT License](http://opensource.org/licenses/MIT).
Please see the [LICENSE](LICENSE) file for full details.
