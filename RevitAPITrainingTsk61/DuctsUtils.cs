using System;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB.Mechanical;

namespace RevitAPITrainingTsk61
{
    public class DuctsUtils
    {
        public static List<DuctType> GetDuctTypes(ExternalCommandData commandData)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document doc = uidoc.Document;

            var ductTypes = new FilteredElementCollector(doc)
               .OfClass(typeof(DuctType))
               .Cast<DuctType>()
               .ToList();


            return ductTypes;
        }
    }
}
